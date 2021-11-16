using UnityEngine;

namespace Chronos
{
	public class TerrainTimeline : ComponentTimeline<Terrain>
	{
		private float _wavingGrassSpeed;

		/// <summary>
		/// The pitch that is applied to the audio source before time effects. Use this property instead of AudioSource.pitch, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float wavingGrassSpeed
		{
			get { return _wavingGrassSpeed; }
			set
			{
				_wavingGrassSpeed = value;
				AdjustProperties();
			}
		}

		public TerrainTimeline(Timeline timeline, Terrain component) : base(timeline, component) { }

		public override void CopyProperties(Terrain source)
		{
			_wavingGrassSpeed = source.terrainData.wavingGrassSpeed;
		}

		public override void AdjustProperties(float timeScale)
		{
			// Currently bugged in Unity
			// component.terrainData.wavingGrassSpeed = wavingGrassSpeed * timeScale;
		}
	}
}
