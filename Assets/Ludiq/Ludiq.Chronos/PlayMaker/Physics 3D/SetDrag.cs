#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Physics (Chronos)")]
	[HelpUrl("http://hutonggames.com/playmakerforum/index.php?topic=4734.0")]
	[Tooltip("Sets the Drag of a Game Object's Rigid Body.")]
	public class SetDrag : ChronosComponentAction<Timeline>
	{
		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[HasFloatSlider(0.0f, 10f)]
		public FsmFloat drag;

		[Tooltip("Repeat every frame. Typically this would be set to True.")]
		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			drag = 1;
		}

		public override void OnEnter()
		{
			DoSetDrag();

			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoSetDrag();
		}

		private void DoSetDrag()
		{
			var go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (UpdateCache(go))
			{
				timeline.rigidbody.drag = drag.Value;
			}
		}
	}
}

#endif
