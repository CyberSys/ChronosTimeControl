#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;
using UnityEngine;

namespace Chronos.PlayMaker
{
	[ActionCategory("Physics 2d (Chronos)")]
	[HutongGames.PlayMaker.Tooltip("Controls whether 2D physics affects the Game Object.")]
	public class SetBodyType2d : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[ObjectType(typeof(RigidbodyType2D))]
		public FsmEnum bodyType;

		public override void Reset()
		{
			gameObject = null;
			bodyType = RigidbodyType2D.Dynamic;
		}

		public override void OnEnter()
		{
			DoSetIsKinematic();
			Finish();
		}

		private void DoSetIsKinematic()
		{
			if (!UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject))) return;

			timeline.rigidbody2D.bodyType = (RigidbodyType2D)bodyType.Value;
		}
	}
}

#endif
