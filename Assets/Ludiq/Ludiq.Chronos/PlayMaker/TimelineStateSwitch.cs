#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Chronos")]
	[Tooltip("Sends events based on the current state of a timeline.")]
	[HelpUrl("http://ludiq.io/chronos/documentation#Timeline.state")]
	public class TimelineStateSwitch : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		public FsmOwnerDefault gameObject;

		public FsmEvent accelerated;
		public FsmEvent normal;
		public FsmEvent slowed;
		public FsmEvent paused;
		public FsmEvent reversed;

		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			accelerated = null;
			normal = null;
			slowed = null;
			paused = null;
			reversed = null;
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

			switch (timeline.state)
			{
				case TimeState.Accelerated:
					Fsm.Event(accelerated);
					break;

				case TimeState.Normal:
					Fsm.Event(normal);
					break;

				case TimeState.Slowed:
					Fsm.Event(slowed);
					break;

				case TimeState.Paused:
					Fsm.Event(paused);
					break;

				case TimeState.Reversed:
					Fsm.Event(reversed);
					break;
			}
		}
	}
}

#endif
