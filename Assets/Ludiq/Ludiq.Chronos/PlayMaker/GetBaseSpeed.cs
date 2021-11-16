#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Chronos")]
	[Tooltip("Gets a base speed used by a timeline.")]
	[HelpUrl("http://ludiq.io/chronos/documentation#Timeline")]
	public class GetBaseSpeed : ChronosComponentAction<Timeline>
	{
		public enum Speed
		{
			Animator,
			Animation,
			Particle,
			Audio,
			Navigation,
			NavigationAngular
		}

		[RequiredField]
		[CheckForComponent(typeof(Timeline))]
		public FsmOwnerDefault gameObject;

		public Speed getSpeed;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat storeValue;

		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			getSpeed = Speed.Animator;
			storeValue = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoAction();

			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoAction();
		}

		private void DoAction()
		{
			if (!UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject))) return;

			switch (getSpeed)
			{
				case Speed.Animator:
					storeValue.Value = timeline.animator.speed;
					break;

				case Speed.Animation:
					storeValue.Value = timeline.animation.speed;
					break;

				case Speed.Particle:
					storeValue.Value = timeline.particleSystem.playbackSpeed;
					break;

				case Speed.Audio:
					storeValue.Value = timeline.audioSource.pitch;
					break;

				case Speed.Navigation:
					storeValue.Value = timeline.navMeshAgent.speed;
					break;

				case Speed.NavigationAngular:
					storeValue.Value = timeline.navMeshAgent.angularSpeed;
					break;

				default:
					storeValue.Value = 0f;
					break;
			}
		}
	}
}

#endif
