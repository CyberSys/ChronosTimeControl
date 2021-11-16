using System.Collections.Generic;
using UnityEngine;

namespace Chronos
{
	// Currently bugged in Unity 5.5 due to a bug in the Simulate method at times higher than the system's duration
	// https://fogbugz.unity3d.com/default.asp?854431_5mmt5ltn2q6nuseh

	public class RewindableParticleSystemTimeline : ComponentTimeline<ParticleSystem>, IParticleSystemTimeline
	{
		#region Fields

		private float absoluteSimulationTime;
		private float loopedSimulationTime;
		private float relativeStartTime;

		#endregion

		#region Properties

		public float playbackSpeed { get; set; }

		public float time
		{
			get { return (loopedSimulationTime - relativeStartTime) % component.main.duration; }
			set { loopedSimulationTime = relativeStartTime + value; }
		}

		public bool isPlaying
		{
			get { return state == State.Playing || state == State.Stopping; }
		}

		public bool isPaused
		{
			get { return state == State.Paused; }
		}

		public bool isStopped
		{
			get { return state == State.Stopped; }
		}

		#endregion

		#region State and Emission

		private enum State
		{
			Playing,
			Paused,
			Stopping,
			Stopped
		}

		private enum EmissionAction
		{
			EnableEmission,
			DisableEmission,
			Play,
			Stop
		}

		private struct StateEvent
		{
			public State state;
			public float time;

			public StateEvent(State state, float time)
			{
				this.state = state;
				this.time = time;
			}
		}

		private struct EmissionEvent
		{
			public EmissionAction action;
			public float time;

			public EmissionEvent(EmissionAction action, float time)
			{
				this.action = action;
				this.time = time;
			}
		}

		private float stateEventsTime
		{
			get { return timeline.time; }
		}

		private float emissionEventsTime
		{
			get { return absoluteSimulationTime; }
		}

		private void RegisterState(State state)
		{
			stateEvents.Add(new StateEvent(state, stateEventsTime));
		}

		private void RegisterEmission(EmissionAction action)
		{
			emissionEvents.Add(new EmissionEvent(action, emissionEventsTime));
		}

		public RewindableParticleSystemTimeline(Timeline timeline, ParticleSystem component) : base(timeline, component)
		{
			emissionEvents = new List<EmissionEvent>();
			stateEvents = new List<StateEvent>();
		}

		private List<StateEvent> stateEvents;
		private State stateOnStart;

		private List<EmissionEvent> emissionEvents;
		private bool enableEmissionOnStart;

		private State _state;

		private State state
		{
			get { return _state; }
			set
			{
				if (!AssertForwardProperty("state", Severity.Error)) return;

				if (_state != value)
				{
					RegisterState(value);
					_state = value;
				}
			}
		}

		private bool _enableEmission;

		public bool enableEmission
		{
			get { return _enableEmission; }
			set
			{
				if (!AssertForwardProperty("enableEmission", Severity.Warn)) return;

				if (_enableEmission && !value)
				{
					RegisterEmission(EmissionAction.DisableEmission);
				}
				else if (!_enableEmission && value)
				{
					RegisterEmission(EmissionAction.EnableEmission);
				}

				_enableEmission = value;
			}
		}

		#endregion

		#region Timeline

		public override void CopyProperties(ParticleSystem source)
		{
			playbackSpeed = source.main.simulationSpeed;

			stateOnStart = state = source.main.playOnAwake ? State.Playing : State.Stopped;

			enableEmissionOnStart = _enableEmission = source.emission.enabled;

			time = 0;

			if (source.useAutoRandomSeed)
            {
				if (source.isPlaying)
				{
					source.Pause(true);
				}

				source.useAutoRandomSeed = false;
				source.randomSeed = (uint)Random.Range(1, int.MaxValue);
			}
		}

		public override void Update()
		{
			if (timeline.timeScale < 0)
			{
				// Determine state by consuming state events

				if (stateEvents.Count > 0)
				{
					StateEvent lastStateEvent = stateEvents[stateEvents.Count - 1];

					if (stateEventsTime <= lastStateEvent.time)
					{
						stateEvents.Remove(lastStateEvent);

						if (stateEvents.Count > 0)
						{
							_state = stateEvents[stateEvents.Count - 1].state;
						}
						else
						{
							_state = stateOnStart;
						}
					}
				}

				// Consume emission events

				for (int i = emissionEvents.Count - 1; i >= 0; i--)
				{
					if (emissionEvents[i].time > emissionEventsTime)
					{
						emissionEvents.RemoveAt(i);
					}
				}
			}

			// Known issue: low time scales / speed will cause stutter
			// Reported here: http://fogbugz.unity3d.com/default.asp?694191_dso514lin4rf5vbg
			
			component.Simulate(0, true, true);

			if (loopedSimulationTime > 0)
			{
				var emission = component.emission;

				emission.enabled = enableEmissionOnStart;

				float chunkStartTime = 0;

				for (int i = 0; i < emissionEvents.Count; i++)
				{
					EmissionEvent current = emissionEvents[i];

					component.Simulate(current.time - chunkStartTime, true, false);

					emission.enabled = current.action == EmissionAction.Play || current.action == EmissionAction.EnableEmission;

					chunkStartTime = current.time;
				}

				component.Simulate(loopedSimulationTime - chunkStartTime, true, false);

				if (state == State.Stopping && component.particleCount == 0 && timeline.timeScale > 0)
				{
					state = State.Stopped;
				}
			}

			if (state == State.Playing || state == State.Stopping)
			{
				absoluteSimulationTime += timeline.deltaTime * playbackSpeed;

				if (state == State.Playing && !component.main.loop && absoluteSimulationTime >= component.main.duration)
				{
					// A bit hacky to stop it here, as the real system just goes on playing,
					// just without emitting, but it shouldn't cause any problem. Unfortunately,
					// there is no check on Unity's side to see if it entered that final state.
					state = State.Stopping;
				}

				// Can be performance intensive at high times.
				// Limit it with a loop-multiple of its time (globally configurable)

				float maxLoops = Timekeeper.instance.maxParticleLoops;

				if (maxLoops > 0 && state != State.Stopping)
				{
					loopedSimulationTime = absoluteSimulationTime % (component.main.duration * maxLoops);
				}
				else
				{
					loopedSimulationTime = absoluteSimulationTime;
				}
			}
		}

		#endregion

		#region Methods

		public void Play(bool withChildren = true)
		{
			if (!AssertForwardMethod("Play", Severity.Warn)) return;

			if (state != State.Paused)
			{
				RegisterEmission(EmissionAction.Play);
				relativeStartTime = loopedSimulationTime;
			}

			state = State.Playing;

			if (withChildren)
			{
				ExecuteOnChildren(ps => ps.Play(false), ps => ps.Play(false));
			}
		}

		public void Pause(bool withChildren = true)
		{
			if (!AssertForwardMethod("Pause", Severity.Warn)) return;

			state = State.Paused;

			if (withChildren)
			{
				ExecuteOnChildren(ps => ps.Pause(false), ps => ps.Pause(false));
			}
		}

		public void Stop(bool withChildren = true)
		{
			if (!AssertForwardMethod("Stop", Severity.Warn)) return;

			state = State.Stopping;

			RegisterEmission(EmissionAction.Stop);

			if (withChildren)
			{
				ExecuteOnChildren(ps => ps.Stop(false), ps => ps.Stop(false));
			}
		}

		public bool IsAlive(bool withChildren = true)
		{
			if (state == State.Stopped)
			{
				return false;
			}

			if (withChildren)
			{
				return CheckOnChildren(ps => ps.IsAlive(false), ps => ps.IsAlive(false));
			}

			return true;
		}

		#endregion

		#region Hierarchy

		private delegate void ChildNativeAction(ParticleSystem target);

		private delegate void ChildChronosAction(IParticleSystemTimeline target);

		private delegate bool ChildNativeCheck(ParticleSystem target);

		private delegate bool ChildChronosCheck(IParticleSystemTimeline target);

		private void ExecuteOnChildren(ChildNativeAction native, ChildChronosAction chronos)
		{
			foreach (ParticleSystem childParticleSystem in timeline.GetComponentsInChildren<ParticleSystem>())
			{
				if (childParticleSystem == component)
				{
					continue;
				}

				Timeline childTimeline = childParticleSystem.GetComponent<Timeline>();

				if (childTimeline != null)
				{
					chronos(childTimeline.particleSystem);
				}
				else
				{
					native(childParticleSystem);
				}
			}
		}

		private bool CheckOnChildren(ChildNativeCheck native, ChildChronosCheck chronos)
		{
			foreach (ParticleSystem childParticleSystem in timeline.GetComponentsInChildren<ParticleSystem>())
			{
				if (childParticleSystem == component)
				{
					continue;
				}

				Timeline childTimeline = childParticleSystem.GetComponent<Timeline>();

				if (childTimeline != null)
				{
					if (!chronos(childTimeline.particleSystem))
					{
						return false;
					}
				}
				else
				{
					if (!native(childParticleSystem))
					{
						return false;
					}
				}
			}

			return true;
		}

		#endregion

		#region Utility

		private bool AssertForwardMethod(string method, Severity severity)
		{
			if (timeline.timeScale <= 0)
			{
				if (severity == Severity.Error)
				{
					throw new ChronosException("Cannot call " + method + " on the particle system while time is paused or rewinding.");
				}
				else if (severity == Severity.Warn)
				{
					Debug.LogWarning("Trying to call " + method +
									 " on the particle system while time is paused or rewinding, ignoring.");
				}
			}

			return timeline.timeScale > 0;
		}

		private bool AssertForwardProperty(string property, Severity severity)
		{
			if (timeline.timeScale <= 0)
			{
				if (severity == Severity.Error)
				{
					throw new ChronosException("Cannot set " + property + " on the particle system while time is paused or rewinding.");
				}
				else if (severity == Severity.Warn)
				{
					Debug.LogWarning("Trying to set " + property +
									 " on the particle system while time is paused or rewinding, ignoring.");
				}
			}

			return timeline.timeScale > 0;
		}

		#endregion
	}
}
