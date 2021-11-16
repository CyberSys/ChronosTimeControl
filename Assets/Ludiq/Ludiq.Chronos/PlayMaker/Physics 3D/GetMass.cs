#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Physics (Chronos)")]
	[Tooltip("Gets the Mass of a Game Object's Rigid Body.")]
	public class GetMass : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		[Tooltip("The GameObject that owns the Rigidbody")]
		public
			FsmOwnerDefault gameObject;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the mass in a float variable.")]
		public FsmFloat storeResult;

		public override void Reset()
		{
			gameObject = null;
			storeResult = null;
		}

		public override void OnEnter()
		{
			DoGetMass();

			Finish();
		}

		private void DoGetMass()
		{
			var go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (UpdateCache(go))
			{
				storeResult.Value = timeline.rigidbody.mass;
			}
		}
	}
}

#endif
