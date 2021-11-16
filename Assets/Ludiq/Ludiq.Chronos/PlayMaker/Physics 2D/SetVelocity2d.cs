#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;
using UnityEngine;

namespace Chronos.PlayMaker
{
	[ActionCategory("Physics 2d (Chronos)")]
	[HutongGames.PlayMaker.Tooltip(
		"Sets the 2d Velocity of a Game Object. To leave any axis unchanged, set variable to 'None'. NOTE: Game object must have a rigidbody 2D."
		)]
	public class SetVelocity2d : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Variable)]
		public FsmVector2 vector;

		public FsmFloat x;
		public FsmFloat y;

		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			vector = null;
			// default axis to variable dropdown with None selected.
			x = new FsmFloat { UseVariable = true };
			y = new FsmFloat { UseVariable = true };

			everyFrame = false;
		}

		public override void Awake()
		{
			Fsm.HandleFixedUpdate = true;
		}

		// TODO: test this works in OnEnter!
		public override void OnEnter()
		{
			DoSetVelocity();

			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnFixedUpdate()
		{
			DoSetVelocity();

			if (!everyFrame)
				Finish();
		}

		private void DoSetVelocity()
		{
			if (!UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject))) return;

			// init position

			Vector2 velocity;

			if (vector.IsNone)
			{
				velocity = timeline.rigidbody2D.velocity;
			}
			else
			{
				velocity = vector.Value;
			}

			// override any axis

			if (!x.IsNone) velocity.x = x.Value;
			if (!y.IsNone) velocity.y = y.Value;

			// apply

			timeline.rigidbody2D.velocity = velocity;
		}
	}
}

#endif
