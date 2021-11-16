#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;
using UnityEngine;

namespace Chronos.PlayMaker
{
	[ActionCategory("Physics 2d (Chronos)")]
	[HutongGames.PlayMaker.Tooltip("Tests if a Game Object's Rigid Body 2D is simulated.")]
	public class GetBodyType2d : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Variable)]
		[ObjectType(typeof(RigidbodyType2D))]
		public FsmEnum store;
		
		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			store = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoIsSimulated();

			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoIsSimulated();
		}

		private void DoIsSimulated()
		{
			if (!UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject))) return;
			
			store.Value = timeline.rigidbody2D.bodyType;
		}
	}
}

#endif
