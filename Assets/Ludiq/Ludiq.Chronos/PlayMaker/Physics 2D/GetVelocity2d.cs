#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;
using UnityEngine;

namespace Chronos.PlayMaker
{
	[ActionCategory("Physics 2d (Chronos)")]
	[HutongGames.PlayMaker.Tooltip(
		"Gets the 2d Velocity of a Game Object and stores it in a Vector2 Variable or each Axis in a Float Variable. NOTE: The Game Object must have a Rigid Body 2D."
		)]
	public class GetVelocity2d : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Variable)]
		public FsmVector2 vector;

		[UIHint(UIHint.Variable)]
		public FsmFloat x;

		[UIHint(UIHint.Variable)]
		public FsmFloat y;

		public Space space;
		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			vector = null;
			x = null;
			y = null;

			space = Space.World;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoGetVelocity();

			if (!everyFrame)
				Finish();
		}

		public override void OnUpdate()
		{
			DoGetVelocity();
		}

		private void DoGetVelocity()
		{
			if (!UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject))) return;

			Vector2 velocity = timeline.rigidbody2D.velocity;

			if (space == Space.Self)
				velocity = timeline.gameObject.transform.InverseTransformDirection(velocity);

			vector.Value = velocity;
			x.Value = velocity.x;
			y.Value = velocity.y;
		}
	}
}

#endif
