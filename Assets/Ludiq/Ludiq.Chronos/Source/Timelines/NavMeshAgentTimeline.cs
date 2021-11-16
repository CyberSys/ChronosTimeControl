using UnityEngine;

namespace Chronos
{
	public class NavMeshAgentTimeline : ComponentTimeline<UnityEngine.AI.NavMeshAgent>
	{
		private float _speed;

		/// <summary>
		/// The speed that is applied to the agent before time effects. Use this property instead of NavMeshAgent.speed, which will be overwritten by the timeline at runtime. 
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

		private float _angularSpeed;

		/// <summary>
		/// The angular speed that is applied to the agent before time effects. Use this property instead of NavMeshAgent.angularSpeed, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float angularSpeed
		{
			get { return _angularSpeed; }
			set
			{
				_angularSpeed = value;
				AdjustProperties();
			}
		}

		public NavMeshAgentTimeline(Timeline timeline, UnityEngine.AI.NavMeshAgent component) : base(timeline, component) { }

		public override void Update()
		{
			if (timeline.lastTimeScale > 0 && timeline.timeScale == 0) // Arrived at halt
			{
				component.velocity = Vector3.zero; // Stop the smoothing
			}
		}

		public override void CopyProperties(UnityEngine.AI.NavMeshAgent source)
		{
			_speed = source.speed;
			_angularSpeed = source.angularSpeed;
		}

		public override void AdjustProperties(float timeScale)
		{
			component.speed = speed * timeScale;
			component.angularSpeed = angularSpeed * timeScale;
		}
	}
}
