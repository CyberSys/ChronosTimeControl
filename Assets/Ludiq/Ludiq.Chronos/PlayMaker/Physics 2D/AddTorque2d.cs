#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Physics 2d (Chronos)")]
	[Tooltip("Adds a 2d torque (rotational force) to a Game Object.")]
	public class AddTorque2d : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		[Tooltip("The GameObject to add torque to.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Torque")]
		public FsmFloat torque;

		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

		public override void Awake()
		{
			Fsm.HandleFixedUpdate = true;
		}

		public override void Reset()
		{
			gameObject = null;

			torque = null;

			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoAddTorque();

			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnFixedUpdate()
		{
			DoAddTorque();
		}

		private void DoAddTorque()
		{
			if (!UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject))) return;

			timeline.rigidbody2D.AddTorque(torque.Value);
		}
	}
}

#endif
