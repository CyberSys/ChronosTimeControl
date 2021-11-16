#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Physics 2d (Chronos)")]
	[Tooltip("Sets the Mass of a Game Object's Rigid Body 2D.")]
	public class SetMass2d : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[HasFloatSlider(0.1f, 10f)]
		public FsmFloat mass;

		public override void Reset()
		{
			gameObject = null;
			mass = 1;
		}

		public override void OnEnter()
		{
			DoSetMass();

			Finish();
		}

		private void DoSetMass()
		{
			if (!UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject))) return;

			timeline.rigidbody2D.mass = mass.Value;
		}
	}
}

#endif
