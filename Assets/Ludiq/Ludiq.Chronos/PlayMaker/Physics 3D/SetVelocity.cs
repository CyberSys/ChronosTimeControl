#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;
using UnityEngine;

namespace Chronos.PlayMaker
{
	[ActionCategory("Physics (Chronos)")]
	[HutongGames.PlayMaker.Tooltip(
		"Sets the Velocity of a Game Object. To leave any axis unchanged, set variable to 'None'. NOTE: Game object must have a rigidbody."
		)]
	public class SetVelocity : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Variable)]
		public FsmVector3 vector;

		public FsmFloat x;
		public FsmFloat y;
		public FsmFloat z;

		public Space space;

		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			vector = null;
			// default axis to variable dropdown with None selected.
			x = new FsmFloat { UseVariable = true };
			y = new FsmFloat { UseVariable = true };
			z = new FsmFloat { UseVariable = true };
			space = Space.Self;
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
			var go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (!UpdateCache(go))
			{
				return;
			}

			// init position

			Vector3 velocity;

			if (vector.IsNone)
			{
				velocity = space == Space.World
					? timeline.rigidbody.velocity
					: go.transform.InverseTransformDirection(timeline.rigidbody.velocity);
			}
			else
			{
				velocity = vector.Value;
			}

			// override any axis

			if (!x.IsNone) velocity.x = x.Value;
			if (!y.IsNone) velocity.y = y.Value;
			if (!z.IsNone) velocity.z = z.Value;

			// apply

			timeline.rigidbody.velocity = space == Space.World ? velocity : go.transform.TransformDirection(velocity);
		}
	}
}

#endif
