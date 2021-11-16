#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;
using UnityEngine;

namespace Chronos.PlayMaker
{
	[ActionCategory("Physics 2d (Chronos)")]
	[HutongGames.PlayMaker.Tooltip(
		"Adds a 2d force to a Game Object. Use Vector2 variable and/or Float variables for each axis.")]
	public class AddForce2d : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		[HutongGames.PlayMaker.Tooltip("The GameObject to apply the force to.")]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip(
			"Optionally apply the force at a position on the object. This will also add some torque. The position is often returned from MousePick or GetCollision2dInfo actions."
			)]
		public FsmVector2 atPosition;

		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip("A Vector2 force to add. Optionally override any axis with the X, Y parameters.")]
		public FsmVector2 vector;

		[HutongGames.PlayMaker.Tooltip("Force along the X axis. To leave unchanged, set to 'None'.")]
		public FsmFloat x;

		[HutongGames.PlayMaker.Tooltip("Force along the Y axis. To leave unchanged, set to 'None'.")]
		public FsmFloat y;

		[HutongGames.PlayMaker.Tooltip("A Vector3 force to add. z is ignored")]
		public FsmVector3 vector3;

		[HutongGames.PlayMaker.Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			atPosition = new FsmVector2 { UseVariable = true };
			vector = null;
			vector3 = new FsmVector3 { UseVariable = true };

			// default axis to variable dropdown with None selected.
			x = new FsmFloat { UseVariable = true };
			y = new FsmFloat { UseVariable = true };

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
			if (!UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject))) return;

			Vector2 force = vector.IsNone ? new Vector2(x.Value, y.Value) : vector.Value;

			if (!vector3.IsNone)
			{
				force.x = vector3.Value.x;
				force.y = vector3.Value.y;
			}

			// override any axis

			if (!x.IsNone) force.x = x.Value;
			if (!y.IsNone) force.y = y.Value;

			// apply force			
			if (!atPosition.IsNone)
			{
				timeline.rigidbody2D.AddForceAtPosition(force, atPosition.Value);
			}
			else
			{
				timeline.rigidbody2D.AddForce(force);
			}
		}
	}
}

#endif
