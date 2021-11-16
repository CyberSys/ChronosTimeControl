using UnityEngine;

namespace Chronos
{
	public class AnimationTimeline : ComponentTimeline<Animation>
	{
		public AnimationTimeline(Timeline timeline, Animation component) : base(timeline, component) { }

		private float _speed;

		/// <summary>
		/// The speed that is applied to the animation before time effects. Use this property instead of AnimationState.speed, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float speed
		{
			get { return _speed; }
			set
			{
				_speed = value;
				AdjustProperties();
			}
		}

		public override void CopyProperties(Animation source)
		{
			float firstAnimationStateSpeed = 1;
			bool found = false;

			foreach (AnimationState animationState in source)
			{
				if (found && firstAnimationStateSpeed != animationState.speed)
				{
					Debug.LogWarning("Different animation speeds per state are not supported.");
				}

				firstAnimationStateSpeed = animationState.speed;
				found = true;
			}

			_speed = firstAnimationStateSpeed;
		}

		public override void AdjustProperties(float timeScale)
		{
			foreach (AnimationState state in component)
			{
				state.speed = speed * timeScale;
			}
		}
	}
}
