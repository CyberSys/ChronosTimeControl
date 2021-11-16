using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Chronos
{
	/// <summary>
	/// Determines what type of clock a timeline observes. 
	/// </summary>
	public enum TimelineMode
	{
		/// <summary>
		/// The timeline observes a LocalClock attached to the same GameObject.
		/// </summary>
		Local,

		/// <summary>
		/// The timeline observes a GlobalClock referenced by globalClockKey. 
		/// </summary>
		Global
	}

	/// <summary>
	/// A component that combines timing measurements from an observed LocalClock or GlobalClock and any AreaClock within which it is. This component should be attached to any GameObject that should be affected by Chronos. 
	/// </summary>
	[AddComponentMenu("Time/Timeline")]
	[DisallowMultipleComponent]
	[HelpURL("http://ludiq.io/chronos/documentation#Timeline")]
	public class Timeline : TimelineEffector
	{
		protected internal const float DefaultRecordingDuration = 30;

		public Timeline()
		{
			children = new List<TimelineChild>();
			areaClocks = new List<IAreaClock>();
			occurrences = new List<Occurrence>();
			handledOccurrences = new HashSet<Occurrence>();
			previousDeltaTimes = new Queue<float>();
			timeScale = lastTimeScale = 1;
		}
		
		protected override void OnStartOrReEnable()
		{
			timeScale = lastTimeScale = clock.timeScale;
			
			base.OnStartOrReEnable();
		}

		protected override void Update()
		{
			TriggerEvents();

			lastTimeScale = timeScale;

			timeScale = clock.timeScale; // Start with the time scale from local / global clock

			for (int i = 0; i < areaClocks.Count; i++) // Blend it with the time scale of each area clock
			{
				var areaClock = areaClocks[i];

				if (areaClock != null)
				{
					float areaClockTimeScale = areaClock.TimeScale(this);

					if (areaClock.innerBlend == ClockBlend.Multiplicative)
					{
						timeScale *= areaClockTimeScale;
					}
					else // if (areaClock.innerBlend == ClockBlend.Additive)
					{
						timeScale += areaClockTimeScale;
					}
				}
			}

			if (!rewindable) // Cap to 0 for non-rewindable timelines
			{
				timeScale = Mathf.Max(0, timeScale);
			}

			if (timeScale != lastTimeScale)
			{
				for (int i = 0; i < components.Count; i++)
				{
					components[i].AdjustProperties();
				}

				for (int i = 0; i < children.Count; i++)
				{
					for (int j = 0; j < children[i].components.Count; j++)
					{
						children[i].components[j].AdjustProperties();
					}
				}
			}

			float unscaledDeltaTime = Timekeeper.unscaledDeltaTime;
			deltaTime = unscaledDeltaTime * timeScale;
			fixedDeltaTime = Time.fixedDeltaTime * timeScale;
			time += deltaTime;
			unscaledTime += unscaledDeltaTime;

			RecordSmoothing();

			base.Update();

			if (timeScale > 0)
			{
				TriggerForwardOccurrences();
			}
			else if (timeScale < 0)
			{
				TriggerBackwardOccurrences();
			}
		}
		
		protected override void OnDisable()
		{
			ReleaseFromAll();

			base.OnDisable();
		}

		public override void Reset()
		{
			base.Reset();
			timeScale = lastTimeScale = 1;
			previousDeltaTimes.Clear();
			occurrences.Clear();
			handledOccurrences.Clear();
			nextForwardOccurrence = null;
			nextBackwardOccurrence = null;
		}

		#region Fields

		protected internal float lastTimeScale;
		protected Queue<float> previousDeltaTimes;
		protected List<Occurrence> occurrences;
		protected HashSet<Occurrence> handledOccurrences;
		protected Occurrence nextForwardOccurrence;
		protected Occurrence nextBackwardOccurrence;
		protected internal List<IAreaClock> areaClocks;
		protected internal List<TimelineChild> children;

		#endregion

		#region Properties
		
		protected override Timeline timeline
		{
			get { return this; }
		}

		[SerializeField]
		private TimelineMode _mode;

		/// <summary>
		/// Determines what type of clock the timeline observes. 
		/// </summary>
		public TimelineMode mode
		{
			get { return _mode; }
			set
			{
				_mode = value;
				_clock = null;
			}
		}

		[SerializeField, GlobalClock]
		private string _globalClockKey;

		/// <summary>
		/// The key of the GlobalClock that is observed by the timeline. This value is only used for the Global mode. 
		/// </summary>
		public string globalClockKey
		{
			get { return _globalClockKey; }
			set
			{
				_globalClockKey = value;
				_clock = null;
			}
		}

		private Clock _clock;

		/// <summary>
		/// The clock observed by the timeline. 
		/// </summary>
		public Clock clock
		{
			get
			{
				if (_clock == null)
				{
					_clock = FindClock();
				}

				return _clock;
			}
		}

		/// <summary>
		/// The time scale of the timeline, computed from all observed clocks. For more information, see Clock.timeScale. 
		/// </summary>
		public float timeScale { get; protected set; }

		/// <summary>
		/// The delta time of the timeline, computed from all observed clocks. For more information, see Clock.deltaTime. 
		/// </summary>
		public float deltaTime { get; protected set; }

		/// <summary>
		/// The fixed delta time of the timeline, computed from all observed clocks. For more information, see Clock.fixedDeltaTime. 
		/// </summary>
		public float fixedDeltaTime { get; protected set; }

		/// <summary>
		/// A smoothed out delta time. Use this value if you need to avoid spikes and fluctuations in delta times. The amount of frames over which this value is smoothed can be adjusted via smoothingDeltas. 
		/// </summary>
		public float smoothDeltaTime
		{
			get { return (deltaTime + previousDeltaTimes.Sum()) / (previousDeltaTimes.Count + 1); }
		}

		/// <summary>
		/// The amount of frames over which smoothDeltaTime is smoothed. 
		/// </summary>
		public static int smoothingDeltas = 5;

		/// <summary>
		/// The time in seconds since the creation of this timeline, computed from all observed clocks. For more information, see Clock.time. 
		/// </summary>
		public float time { get; protected internal set; }

		/// <summary>
		/// The unscaled time in seconds since the creation of this timeline. For more information, see Clock.unscaledTime. 
		/// </summary>
		public float unscaledTime { get; protected set; }

		/// <summary>
		/// Indicates the state of the timeline. 
		/// </summary>
		public TimeState state
		{
			get { return Timekeeper.GetTimeState(timeScale); }
		}

		#endregion

		#region Timing

		protected virtual Clock FindClock()
		{
			if (mode == TimelineMode.Local)
			{
				LocalClock localClock = GetComponent<LocalClock>();

				if (localClock == null)
				{
					throw new ChronosException(string.Format("Missing local clock for timeline."));
				}

				return localClock;
			}
			else if (mode == TimelineMode.Global)
			{
				GlobalClock oldGlobalClock = _clock as GlobalClock;

				if (oldGlobalClock != null)
				{
					oldGlobalClock.Unregister(this);
				}

				if (!Timekeeper.instance.HasClock(globalClockKey))
				{
					throw new ChronosException(string.Format("Missing global clock for timeline: '{0}'.", globalClockKey));
				}

				GlobalClock globalClock = Timekeeper.instance.Clock(globalClockKey);

				globalClock.Register(this);

				return globalClock;
			}
			else
			{
				throw new ChronosException(string.Format("Unknown timeline mode: '{0}'.", mode));
			}
		}

		/// <summary>
		/// Releases the timeline from the specified area clock's effects. 
		/// </summary>
		public virtual void ReleaseFrom(IAreaClock areaClock)
		{
			areaClock.Release(this);
		}

		/// <summary>
		/// Releases the timeline from the effects of all the area clocks within which it is. 
		/// </summary>
		public virtual void ReleaseFromAll()
		{
			foreach (IAreaClock areaClock in areaClocks.Where(ac => ac != null).ToArray())
			{
				areaClock.Release(this);
			}

			areaClocks.Clear();
		}

		protected virtual void TriggerEvents()
		{
			if (lastTimeScale != 0 && timeScale == 0)
			{
				SendMessage("OnStartPause", SendMessageOptions.DontRequireReceiver);
			}

			if (lastTimeScale == 0 && timeScale != 0)
			{
				SendMessage("OnStopPause", SendMessageOptions.DontRequireReceiver);
			}

			if (lastTimeScale >= 0 && timeScale < 0)
			{
				SendMessage("OnStartRewind", SendMessageOptions.DontRequireReceiver);
			}

			if (lastTimeScale < 0 && timeScale >= 0)
			{
				SendMessage("OnStopRewind", SendMessageOptions.DontRequireReceiver);
			}

			if ((lastTimeScale <= 0 || lastTimeScale >= 1) && (timeScale > 0 && timeScale < 1))
			{
				SendMessage("OnStartSlowDown", SendMessageOptions.DontRequireReceiver);
			}

			if ((lastTimeScale > 0 && lastTimeScale < 1) && (timeScale <= 0 || timeScale >= 1))
			{
				SendMessage("OnStopSlowDown", SendMessageOptions.DontRequireReceiver);
			}

			if (lastTimeScale <= 1 && timeScale > 1)
			{
				SendMessage("OnStartFastForward", SendMessageOptions.DontRequireReceiver);
			}

			if (lastTimeScale > 1 && timeScale <= 1)
			{
				SendMessage("OnStopFastForward", SendMessageOptions.DontRequireReceiver);
			}
		}

		protected virtual void RecordSmoothing()
		{
			if (deltaTime != 0)
			{
				previousDeltaTimes.Enqueue(deltaTime);
			}

			if (previousDeltaTimes.Count > smoothingDeltas)
			{
				previousDeltaTimes.Dequeue();
			}
		}

		#endregion

		#region Occurrences

		protected void TriggerForwardOccurrences()
		{
			handledOccurrences.Clear();

			while (nextForwardOccurrence != null && nextForwardOccurrence.time <= time)
			{
				nextForwardOccurrence.Forward();

				handledOccurrences.Add(nextForwardOccurrence);

				nextBackwardOccurrence = nextForwardOccurrence;

				nextForwardOccurrence = OccurrenceAfter(nextForwardOccurrence.time, handledOccurrences);
			}
		}

		protected void TriggerBackwardOccurrences()
		{
			handledOccurrences.Clear();

			while (nextBackwardOccurrence != null && nextBackwardOccurrence.time >= time)
			{
				nextBackwardOccurrence.Backward();

				if (nextBackwardOccurrence.repeatable)
				{
					handledOccurrences.Add(nextBackwardOccurrence);

					nextForwardOccurrence = nextBackwardOccurrence;
				}
				else
				{
					occurrences.Remove(nextBackwardOccurrence);
				}

				nextBackwardOccurrence = OccurrenceBefore(nextBackwardOccurrence.time, handledOccurrences);
			}
		}

		protected Occurrence OccurrenceAfter(float time, params Occurrence[] ignored)
		{
			return OccurrenceAfter(time, (IEnumerable<Occurrence>) ignored);
		}

		protected Occurrence OccurrenceAfter(float time, IEnumerable<Occurrence> ignored)
		{
			Occurrence after = null;

			for (int i = 0; i < occurrences.Count; i++)
			{
				var occurrence = occurrences[i];

				if (occurrence.time >= time &&
				    !ignored.Contains(occurrence) &&
				    (after == null || occurrence.time < after.time))
				{
					after = occurrence;
				}
			}

			return after;
		}

		protected Occurrence OccurrenceBefore(float time, params Occurrence[] ignored)
		{
			return OccurrenceBefore(time, (IEnumerable<Occurrence>) ignored);
		}

		protected Occurrence OccurrenceBefore(float time, IEnumerable<Occurrence> ignored)
		{
			Occurrence before = null;

			for (int i = 0; i < occurrences.Count; i++)
			{
				var occurrence = occurrences[i];

				if (occurrence.time <= time &&
				    !ignored.Contains(occurrence) &&
				    (before == null || occurrence.time > before.time))
				{
					before = occurrence;
				}
			}

			return before;
		}

		protected virtual void PlaceOccurence(Occurrence occurrence, float time)
		{
			if (time == this.time)
			{
				if (timeScale >= 0)
				{
					occurrence.Forward();
					nextBackwardOccurrence = occurrence;
				}
				else
				{
					occurrence.Backward();
					nextForwardOccurrence = occurrence;
				}
			}
			else if (time > this.time)
			{
				if (nextForwardOccurrence == null ||
				    nextForwardOccurrence.time > time)
				{
					nextForwardOccurrence = occurrence;
				}
			}
			else if (time < this.time)
			{
				if (nextBackwardOccurrence == null ||
				    nextBackwardOccurrence.time < time)
				{
					nextBackwardOccurrence = occurrence;
				}
			}
		}

		/// <summary>
		/// Schedules an occurrence at a specified absolute time in seconds on the timeline. 
		/// </summary>
		public virtual Occurrence Schedule(float time, bool repeatable, Occurrence occurrence)
		{
			occurrence.time = time;
			occurrence.repeatable = repeatable;
			occurrences.Add(occurrence);
			PlaceOccurence(occurrence, time);
			return occurrence;
		}

		/// <summary>
		/// Executes an occurrence now and places it on the schedule for rewinding. 
		/// </summary>
		public Occurrence Do(bool repeatable, Occurrence occurrence)
		{
			return Schedule(time, repeatable, occurrence);
		}

		/// <summary>
		/// Plans an occurrence to be executed in the specified delay in seconds. 
		/// </summary>
		public Occurrence Plan(float delay, bool repeatable, Occurrence occurrence)
		{
			if (delay <= 0)
			{
				throw new ChronosException("Planned occurrences must be in the future.");
			}

			return Schedule(time + delay, repeatable, occurrence);
		}

		/// <summary>
		/// Creates a "memory" of an occurrence at a specified "past-delay" in seconds. This means that the occurrence will only be executed if time is rewound, and that it will be executed backward first. 
		/// </summary>
		public Occurrence Memory(float delay, bool repeatable, Occurrence occurrence)
		{
			if (delay >= 0)
			{
				throw new ChronosException("Memory occurrences must be in the past.");
			}

			return Schedule(time + delay, repeatable, occurrence);
		}

		#region Func<T>

		public Occurrence Schedule<T>(float time, bool repeatable, ForwardFunc<T> forward, BackwardFunc<T> backward)
		{
			return Schedule(time, repeatable, new FuncOccurence<T>(forward, backward));
		}

		public Occurrence Do<T>(bool repeatable, ForwardFunc<T> forward, BackwardFunc<T> backward)
		{
			return Do(repeatable, new FuncOccurence<T>(forward, backward));
		}

		public Occurrence Plan<T>(float delay, bool repeatable, ForwardFunc<T> forward, BackwardFunc<T> backward)
		{
			return Plan(delay, repeatable, new FuncOccurence<T>(forward, backward));
		}

		public Occurrence Memory<T>(float delay, bool repeatable, ForwardFunc<T> forward, BackwardFunc<T> backward)
		{
			return Memory(delay, repeatable, new FuncOccurence<T>(forward, backward));
		}

		#endregion

		#region Action

		public Occurrence Schedule(float time, bool repeatable, ForwardAction forward, BackwardAction backward)
		{
			return Schedule(time, repeatable, new ActionOccurence(forward, backward));
		}

		public Occurrence Do(bool repeatable, ForwardAction forward, BackwardAction backward)
		{
			return Do(repeatable, new ActionOccurence(forward, backward));
		}

		public Occurrence Plan(float delay, bool repeatable, ForwardAction forward, BackwardAction backward)
		{
			return Plan(delay, repeatable, new ActionOccurence(forward, backward));
		}

		public Occurrence Memory(float delay, bool repeatable, ForwardAction forward, BackwardAction backward)
		{
			return Memory(delay, repeatable, new ActionOccurence(forward, backward));
		}

		#endregion

		#region Forward Action

		public Occurrence Schedule(float time, ForwardAction forward)
		{
			return Schedule(time, false, new ForwardActionOccurence(forward));
		}

		public Occurrence Plan(float delay, ForwardAction forward)
		{
			return Plan(delay, false, new ForwardActionOccurence(forward));
		}

		public Occurrence Memory(float delay, ForwardAction forward)
		{
			return Memory(delay, false, new ForwardActionOccurence(forward));
		}

		#endregion

		/// <summary>
		/// Removes the specified occurrence from the timeline. 
		/// </summary>
		public void Cancel(Occurrence occurrence)
		{
			if (!occurrences.Contains(occurrence))
			{
				throw new ChronosException("Occurrence to cancel not found on timeline.");
			}
			else
			{
				if (occurrence == nextForwardOccurrence)
				{
					nextForwardOccurrence = OccurrenceAfter(occurrence.time, occurrence);
				}

				if (occurrence == nextBackwardOccurrence)
				{
					nextBackwardOccurrence = OccurrenceBefore(occurrence.time, occurrence);
				}

				occurrences.Remove(occurrence);
			}
		}

		/// <summary>
		/// Removes the specified occurrence from the timeline and returns true if it is found. Otherwise, returns false. 
		/// </summary>
		public bool TryCancel(Occurrence occurrence)
		{
			if (!occurrences.Contains(occurrence))
			{
				return false;
			}
			else
			{
				Cancel(occurrence);
				return true;
			}
		}

		/// <summary>
		/// Change the absolute time in seconds of the specified occurrence on the timeline.
		/// </summary>
		public void Reschedule(Occurrence occurrence, float time)
		{
			occurrence.time = time;
			PlaceOccurence(occurrence, time);
		}

		/// <summary>
		/// Moves the specified occurrence forward on the timeline by the specified delay in seconds.
		/// </summary>
		public void Postpone(Occurrence occurrence, float delay)
		{
			Reschedule(occurrence, time + delay);
		}

		/// <summary>
		/// Moves the specified occurrence backward on the timeline by the specified delay in seconds.
		/// </summary>
		public void Prepone(Occurrence occurrence, float delay)
		{
			Reschedule(occurrence, time - delay);
		}

		#endregion

		#region Coroutines

		/// <summary>
		/// Suspends the coroutine execution for the given amount of seconds. This method should only be used with a yield statement in coroutines. 
		/// </summary>
		public Coroutine WaitForSeconds(float seconds)
		{
			return StartCoroutine(WaitingForSeconds(seconds));
		}

		protected IEnumerator WaitingForSeconds(float seconds)
		{
			float start = time;

			while (time < start + seconds)
			{
				yield return null;
			}
		}

		#endregion

		#region Rewinding / Recording

		[SerializeField, FormerlySerializedAs("_recordTransform")]
		private bool _rewindable = true;

		/// <summary>
		/// Determines whether the timeline should record support rewind.
		/// </summary>
		public bool rewindable
		{
			get { return _rewindable; }
			set
			{
				_rewindable = value;
				
				if (particleSystem != null)
				{
					CacheComponents();
				}
			}
		}

		[SerializeField]
		private float _recordingDuration = DefaultRecordingDuration;

		/// <summary>
		/// The maximum duration in seconds during which snapshots will be recorded. Higher values offer more rewind time but require more memory. 
		/// </summary>
		public float recordingDuration
		{
			get { return _recordingDuration; }
			set
			{
				_recordingDuration = value;

				if (recorder != null)
				{
					recorder.Reset();
				}
			}
		}

		/// <summary>
		/// Returns the available rewind duration in seconds. 
		/// </summary>
		public float availableRewindDuration
		{
			get
			{
				return transform.availableRewindDuration;
			}
		}

		#endregion
	}
}
