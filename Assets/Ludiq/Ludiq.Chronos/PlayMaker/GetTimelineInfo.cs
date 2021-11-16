#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Chronos")]
	[Tooltip("Gets various useful time measurements from a timeline.")]
	[HelpUrl("http://ludiq.io/chronos/documentation#Timeline")]
	public class GetTimelineInfo : ChronosComponentAction<Timeline>
	{
		public enum TimeInfo
		{
			DeltaTime,
			FixedDeltaTime,
			SmoothDeltaTime,
			TimeScale,
			Time,
			UnscaledTime,
			TimeInCurrentState,
		}

		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		public FsmOwnerDefault gameObject;

		public TimeInfo getInfo;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat storeValue;

		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			getInfo = TimeInfo.DeltaTime;
			storeValue = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoAction();

			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoAction();
		}

		private void DoAction()
		{
			if (!UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject))) return;

			switch (getInfo)
			{
				case TimeInfo.DeltaTime:
					storeValue.Value = timeline.deltaTime;
					break;

				case TimeInfo.FixedDeltaTime:
					storeValue.Value = timeline.fixedDeltaTime;
					break;

				case TimeInfo.SmoothDeltaTime:
					storeValue.Value = timeline.smoothDeltaTime;
					break;

				case TimeInfo.TimeScale:
					storeValue.Value = timeline.timeScale;
					break;

				case TimeInfo.Time:
					storeValue.Value = timeline.time;
					break;

				case TimeInfo.UnscaledTime:
					storeValue.Value = timeline.unscaledTime;
					break;

				case TimeInfo.TimeInCurrentState:
					storeValue.Value = timeline.time - State.RealStartTime;
					break;

				default:
					storeValue.Value = 0f;
					break;
			}
		}
	}
}

#endif
