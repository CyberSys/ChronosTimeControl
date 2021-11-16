#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;

namespace Chronos.PlayMaker
{
	[ActionCategory("Chronos")]
	[Tooltip("Sets a base speed used by a timeline.")]
	[HelpUrl("http://ludiq.io/chronos/documentation#Timeline")]
	public class SetBaseSpeed : ChronosComponentAction<Timeline>
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

		public Speed setSpeed;

		[RequiredField]
		public FsmFloat value;

		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			setSpeed = Speed.Animator;
			value = null;
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

			switch (setSpeed)
			{
				case Speed.Animator:
					timeline.animator.speed = value.Value;
					break;

				case Speed.Animation:
					timeline.animation.speed = value.Value;
					break;

				case Speed.Particle:
					timeline.particleSystem.playbackSpeed = value.Value;
					break;

				case Speed.Audio:
					timeline.audioSource.pitch = value.Value;
					break;

				case Speed.Navigation:
					timeline.navMeshAgent.speed = value.Value;
					break;

				case Speed.NavigationAngular:
					timeline.navMeshAgent.angularSpeed = value.Value;
					break;
			}
		}
	}
}

#endif
