#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Chronos")]
	[Tooltip("Schedules an event at a specific delay on a timeline. If no event is specified, FINISHED will be used.")]
	[HelpUrl("http://ludiq.io/chronos/documentation#Timeline.Schedule")]
	public class Schedule : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		public FsmFloat delay;

		[Title("Event")]
		public FsmEvent scheduledEvent;

		public override void Reset()
		{
			gameObject = null;
			delay = 1f;
			scheduledEvent = null;
		}

		public override void OnEnter()
		{
			if (!UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject))) return;

			timeline.Schedule(timeline.time + delay.Value, delegate
			{
				if (!FsmEvent.IsNullOrEmpty(scheduledEvent))
				{
					Fsm.Event(scheduledEvent);
				}
				else
				{
					Finish();
				}
			});
		}
	}
}

#endif
