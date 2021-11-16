#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Chronos")]
	[Tooltip("Gets various useful time measurements from a clock.")]
	[HelpUrl("http://ludiq.io/chronos/documentation#Clock")]
	public class GetClockInfo : ChronosComponentAction<Clock>
	{
		public enum TimeInfo
		{
			DeltaTime,
			FixedDeltaTime,
			LocalTimeScale,
			TimeScale,
			Time,
			UnscaledTime,
			StartTime,
		}

		[RequiredField]
		[CheckForComponent(typeof(Clock))]
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
					storeValue.Value = clock.deltaTime;
					break;

				case TimeInfo.FixedDeltaTime:
					storeValue.Value = clock.fixedDeltaTime;
					break;

				case TimeInfo.LocalTimeScale:
					storeValue.Value = clock.localTimeScale;
					break;

				case TimeInfo.TimeScale:
					storeValue.Value = clock.timeScale;
					break;

				case TimeInfo.Time:
					storeValue.Value = clock.time;
					break;

				case TimeInfo.UnscaledTime:
					storeValue.Value = clock.unscaledTime;
					break;

				case TimeInfo.StartTime:
					storeValue.Value = clock.startTime;
					break;

				default:
					storeValue.Value = 0f;
					break;
			}
		}
	}
}

#endif
