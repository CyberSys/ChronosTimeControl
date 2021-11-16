using UnityEngine;

namespace Chronos
{
	/// <summary>
	/// A Clock that only affects a Timeline attached to the same GameObject. 
	[AddComponentMenu("Time/Local Clock")]
	[DisallowMultipleComponent]
	[HelpURL("http://ludiq.io/chronos/documentation#LocalClock")]
	public class LocalClock : Clock
	{
		protected override void Awake()
		{
			base.Awake();

			CacheComponents();
		}

		#region Fields

		protected Timeline timeline;

		#endregion

		/// <summary>
		/// The components used by the local clock are cached for performance optimization. If you  add or remove the Timeline on the GameObject, you need to call this method to update the local clock accordingly. 
		/// </summary>
		public virtual void CacheComponents()
		{
			timeline = GetComponent<Timeline>();

			if (timeline == null)
			{
				throw new ChronosException(string.Format("Missing timeline for local clock."));
			}
		}
	}
}
