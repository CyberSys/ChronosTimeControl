#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Physics (Chronos)")]
	[Tooltip("Sets the Mass of a Game Object's Rigid Body.")]
	public class SetMass : ChronosComponentAction<Timeline>
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
			var go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (UpdateCache(go))
			{
				timeline.rigidbody.mass = mass.Value;
			}
		}
	}
}

#endif
