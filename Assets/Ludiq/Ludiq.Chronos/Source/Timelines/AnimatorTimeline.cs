using UnityEngine;

namespace Chronos
{
	public class AnimatorTimeline : ComponentTimeline<Animator>
	{
		public AnimatorTimeline(Timeline timeline, Animator component) : base(timeline, component) { }

		private float _speed;

		/// <summary>
		/// The speed that is applied to the animator before time effects. Use this property instead of Animator.speed, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float speed
		{
			get { return _speed; }
			set
			{
				_speed = value;
				AdjustProperties();
			}
		}

		private int recordedFrames
		{
			get
			{
				// TODO: Proper FPS anticipation, with Application.targetFrameRate and v-sync
				// http://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html
				return Mathf.Clamp((int)(timeline.recordingDuration * 60), 1, 10000);
			}
		}

		public override void CopyProperties(Animator source)
		{
			_speed = source.speed;
		}

		public override void AdjustProperties(float timeScale)
		{
			if (timeScale > 0)
			{
				component.speed = speed * timeScale;
			}
			else
			{
				component.speed = 0;
			}
		}

		public override void OnStartOrReEnable()
		{
			if (timeline.rewindable)
			{
				component.StartRecording(recordedFrames);
			}
		}

		public override void OnDisable()
		{
			if (timeline.rewindable)
			{
				component.StopRecording();
			}
		}

		public override void Update()
		{
			if (timeline.rewindable)
			{
				float timeScale = timeline.timeScale;
				float lastTimeScale = timeline.lastTimeScale;

				// TODO: Refactor and put the recorder offline when time is paused

				if (lastTimeScale >= 0 && timeScale < 0) // Started rewind
				{
					component.StopRecording();

					// There seems to be a bug in some cases in which no data is recorded
					// and recorder start and stop time are at -1. Can't seem to figure
					// when or why it happens, though. Temporary hotfix to disable playback
					// in that case.
					if (component.recorderStartTime < 0)
					{
						Debug.LogWarning("Animator timeline failed to record for unknown reasons.\nSee: http://forum.unity3d.com/threads/341203/", component);
					}
					else
					{
						component.StartPlayback();
						component.playbackTime = component.recorderStopTime;
					}
				}
				else if (component.recorderMode == AnimatorRecorderMode.Playback && timeScale > 0) // Stopped rewind
				{
					component.StopPlayback();
					component.StartRecording(recordedFrames);
				}
				else if (timeScale < 0 && component.recorderMode == AnimatorRecorderMode.Playback) // Rewinding
				{
					float playbackTime = Mathf.Max(component.recorderStartTime, component.playbackTime + timeline.deltaTime);

					component.playbackTime = playbackTime;
				}
			}
		}
	}
}
