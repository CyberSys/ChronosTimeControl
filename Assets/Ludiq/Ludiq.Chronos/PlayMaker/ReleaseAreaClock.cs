#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Chronos")]
	[Tooltip("Releases all timelines within an area clock.")]
	[HelpUrl("http://ludiq.io/chronos/documentation#AreaClock.ReleaseAll")]
	public class ReleaseAreaClock : ChronosComponentAction<Clock>
	{
		[RequiredField]
		[CheckForComponent(typeof(IAreaClock))]
		public FsmOwnerDefault gameObject;

		public override void Reset()
		{
			gameObject = null;
		}

		public override void OnEnter()
		{
			DoAction();
		}

		private void DoAction()
		{
			if (!UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject))) return;

			areaClock.ReleaseAll();
		}
	}
}

#endif
