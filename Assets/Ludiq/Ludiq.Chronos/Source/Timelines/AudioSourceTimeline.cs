using UnityEngine;

namespace Chronos
{
	public class AudioSourceTimeline : ComponentTimeline<AudioSource>
	{
		private float _pitch;

		/// <summary>
		/// The pitch that is applied to the audio source before time effects. Use this property instead of AudioSource.pitch, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float pitch
		{
			get { return _pitch; }
			set
			{
				_pitch = value;
				AdjustProperties();
			}
		}

		public AudioSourceTimeline(Timeline timeline, AudioSource component) : base(timeline, component) { }

		public override void CopyProperties(AudioSource source)
		{
			_pitch = source.pitch;
		}

		public override void AdjustProperties(float timeScale)
		{
			component.pitch = pitch * timeScale;
		}
	}
}
