using UnityEngine;

namespace Chronos
{
	/// <summary>
	/// A component that transfers the effects of any ancestor Timeline in the hierarchy to the current GameObject.
	/// </summary>
	[AddComponentMenu("Time/Timeline Child")]
	[DisallowMultipleComponent]
	[HelpURL("http://ludiq.io/chronos/documentation#Timeline")]
	public class TimelineChild : TimelineEffector
	{
		/// <summary>
		/// The parent Timeline from which time effects are transfered.
		/// </summary>
		public Timeline parent { get; private set; }

		protected override Timeline timeline
		{
			get { return parent; }
		}

		protected override void Awake()
		{
			CacheParent();

			base.Awake();
		}

		/// <summary>
		/// The timeline hierarchy is cached for performance. If you change the transform hierarchy, you must call this method to update it.
		/// </summary>
		public void CacheParent()
		{
			var previousParent = parent;

			parent = GetComponentInParent<Timeline>();

			if (parent == null)
			{
				throw new ChronosException("Missing parent timeline for timeline child.");
			}

			if (previousParent != null)
			{
				previousParent.children.Remove(this);
			}

			parent.children.Add(this);
		}
	}
}