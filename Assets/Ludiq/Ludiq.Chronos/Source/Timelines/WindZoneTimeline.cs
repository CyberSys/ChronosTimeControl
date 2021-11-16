using UnityEngine;

namespace Chronos
{
	public class WindZoneTimeline : ComponentTimeline<WindZone>
	{
		private float _windMain;
		private float _windTurbulence;
		private float _windPulseFrequency;
		private float _windPulseMagnitude;

		/// <summary>
		/// The wind that is applied to the wind zone before time effects. Use this property instead of WindZone.windMain, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float windMain
		{
			get { return _windMain; }
			set
			{
				_windMain = value;
				AdjustProperties();
			}
		}

		/// <summary>
		/// The turbulence that is applied to the wind zone before time effects. Use this property instead of WindZone.windTurbulence, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float windTurbulence
		{
			get { return _windTurbulence; }
			set
			{
				_windTurbulence = value;
				AdjustProperties();
			}
		}

		/// <summary>
		/// The pulse magnitude that is applied to the wind zone before time effects. Use this property instead of WindZone.windPulseMagnitude, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float windPulseMagnitude
		{
			get { return _windPulseMagnitude; }
			set
			{
				_windPulseMagnitude = value;
				AdjustProperties();
			}
		}

		/// <summary>
		/// The pulse frquency that is applied to the wind zone before time effects. Use this property instead of WindZone.windPulseFrequency, which will be overwritten by the timeline at runtime. 
		/// </summary>
		public float windPulseFrequency
		{
			get { return _windPulseFrequency; }
			set
			{
				_windPulseFrequency = value;
				AdjustProperties();
			}
		}

		public WindZoneTimeline(Timeline timeline, WindZone component) : base(timeline, component) { }

		public override void CopyProperties(WindZone source)
		{
			_windMain = source.windMain;
			_windTurbulence = source.windTurbulence;
			_windPulseFrequency = source.windPulseFrequency;
			_windPulseMagnitude = source.windPulseMagnitude;
		}

		public override void AdjustProperties(float timeScale)
		{
			component.windTurbulence = windTurbulence * timeScale * Mathf.Abs(timeScale);
			component.windPulseFrequency = windPulseFrequency * timeScale;
			component.windPulseMagnitude = windPulseMagnitude * Mathf.Sign(timeScale);
		}
	}
}
