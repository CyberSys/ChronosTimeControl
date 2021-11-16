#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;
using UnityEngine;

namespace Chronos.PlayMaker
{
	[ActionCategory("Physics (Chronos)")]
	[HutongGames.PlayMaker.Tooltip(
		"Adds a force to a Game Object. Use Vector3 variable and/or Float variables for each axis.")]
	public class AddForce : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(RigidbodyTimeline3D))]
		[HutongGames.PlayMaker.Tooltip("The GameObject to apply the force to.")]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip(
			"Optionally apply the force at a position on the object. This will also add some torque. The position is often returned from MousePick or GetCollisionInfo actions."
			)]
		public FsmVector3 atPosition;

		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip("A Vector3 force to add. Optionally override any axis with the X, Y, Z parameters.")]
		public FsmVector3 vector;

		[HutongGames.PlayMaker.Tooltip("Force along the X axis. To leave unchanged, set to 'None'.")]
		public FsmFloat x;

		[HutongGames.PlayMaker.Tooltip("Force along the Y axis. To leave unchanged, set to 'None'.")]
		public FsmFloat y;

		[HutongGames.PlayMaker.Tooltip("Force along the Z axis. To leave unchanged, set to 'None'.")]
		public FsmFloat z;

		[HutongGames.PlayMaker.Tooltip("Apply the force in world or local space.")]
		public Space space;

		[HutongGames.PlayMaker.Tooltip("The type of force to apply. See Unity Physics docs.")]
		public ForceMode forceMode;

		[HutongGames.PlayMaker.Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			atPosition = new FsmVector3 { UseVariable = true };
			vector = null;
			// default axis to variable dropdown with None selected.
			x = new FsmFloat { UseVariable = true };
			y = new FsmFloat { UseVariable = true };
			z = new FsmFloat { UseVariable = true };
			space = Space.World;
			forceMode = ForceMode.Force;
			everyFrame = false;
		}

		public override void Awake()
		{
			Fsm.HandleFixedUpdate = true;
		}

		public override void OnEnter()
		{
			DoAddForce();

			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnFixedUpdate()
		{
			DoAddForce();
		}

		private void DoAddForce()
		{
			var go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (!UpdateCache(go))
			{
				return;
			}

			var force = vector.IsNone ? new Vector3(x.Value, y.Value, z.Value) : vector.Value;

			// override any axis

			if (!x.IsNone) force.x = x.Value;
			if (!y.IsNone) force.y = y.Value;
			if (!z.IsNone) force.z = z.Value;

			// apply force			

			if (space == Space.World)
			{
				if (!atPosition.IsNone)
				{
					timeline.rigidbody.AddForceAtPosition(force, atPosition.Value, forceMode);
				}
				else
				{
					timeline.rigidbody.AddForce(force, forceMode);
				}
			}
			else
			{
				timeline.rigidbody.AddRelativeForce(force, forceMode);
			}
		}
	}
}

#endif
