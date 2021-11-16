using UnityEngine;

namespace Chronos
{
	public interface IParticleSystemTimeline : IComponentTimeline<ParticleSystem>
	{
		/// <summary>
		/// The playback speed that is applied to the particle system before time effects. Use this property instead of ParticleSystem.playbackSpeed, which will be overwritten by the timeline at runtime. 
		/// </summary>
		float playbackSpeed { get; set; }

		/// <summary>
		/// The playback time of the particle system. Use this property instead of ParticleSystem.time, which will be overwritten by the timeline at runtime. 
		/// </summary>
		float time { get; set; }

		/// <summary>
		/// Determines whether the particle system should emit. Use this property instead of ParticleSystem.enableEmission, which will be overwritten by the timeline at runtime. 
		/// </summary>
		bool enableEmission { get; set; }

		/// <summary>
		/// Indicates whether the particle system is playing. Use this property instead of ParticleSystem.isPlaying, which will be overwritten by the timeline at runtime. 
		/// </summary>
		bool isPlaying { get; }

		/// <summary>
		/// Indicates whether the particle system is paused. Use this property instead of ParticleSystem.isPaused, which will be overwritten by the timeline at runtime. 
		/// </summary>
		bool isPaused { get; }

		/// <summary>
		/// Indicates whether the particle system is stopped. Use this property instead of ParticleSystem.isStopped, which will be overwritten by the timeline at runtime. 
		/// </summary>
		bool isStopped { get; }

		/// <summary>
		/// Plays the particle system. Use this method instead of ParticleSystem.Play(), which will be overwritten by the timeline at runtime. 
		/// </summary>
		void Play(bool withChildren = true);

		/// <summary>
		/// Pauses the particle system. Use this method instead of ParticleSystem.Pause(), which will be overwritten by the timeline at runtime. 
		/// </summary>
		void Pause(bool withChildren = true);

		/// <summary>
		/// Stops the particle system. Use this method instead of ParticleSystem.Stop(), which will be overwritten by the timeline at runtime. 
		/// </summary>
		void Stop(bool withChildren = true);

		/// <summary>
		/// Indicates whether the particle system is alive. Use this method instead of ParticleSystem.IsAlive, which will be overwritten by the timeline at runtime. 
		/// </summary>
		bool IsAlive(bool withChildren = true);
	}
}
