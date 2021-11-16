using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chronos
{
	/// <summary>
	/// Determines how objects should behave when progressing within an area clock. 
	/// </summary>
	public enum AreaClockMode
	{
		/// <summary>
		/// Objects that enter the clock are instantly affected by its full time scale, without any smoothing. 
		/// </summary>
		Instant = 0,

		/// <summary>
		/// Objects that enter the clock are progressively affected by its time scale, depending on a A / B ratio where:
		/// <para>A is the distance between center and the object</para>
		/// <para>B is the distance between center and the collider's edge in the object's direction</para>
		/// </summary>
		PointToEdge = 1,

		/// <summary>
		/// Objects that enter the clock are progressively affected by its time scale, depending on a A / B ratio where: 
		/// <para>A is the distance between the object's entry point and its current position</para>
		/// <para>B is the value of padding</para>
		/// </summary>
		DistanceFromEntry = 2
	}

	public interface IAreaClock
	{
		void Release(Timeline timeline);
		void ReleaseAll();
		float TimeScale(Timeline timeline);
		float timeScale { get; }
		AreaClockMode mode { get; }
		ClockBlend innerBlend { get; set; }
		bool ContainsPoint(Vector3 point);
		AnimationCurve curve { get; set; }
	}

	[HelpURL("http://ludiq.io/chronos/documentation#AreaClock")]
	public abstract class AreaClock<TCollider, TVector> : Clock, IAreaClock
	{
		public AreaClock()
		{
			within = new Dictionary<Timeline, Vector3>();
			outOfBounds = new HashSet<Timeline>();
		}

		protected override void Awake()
		{
			base.Awake();

			CacheComponents();
		}

		protected override void OnDisable()
		{
			ReleaseAll();

			base.OnDisable();
		}

		#region Fields

		protected Dictionary<Timeline, Vector3> within;
		protected HashSet<Timeline> outOfBounds;
		protected new TCollider collider;

		#endregion

		#region Properties

		[SerializeField]
		private AreaClockMode _mode;

		/// <summary>
		/// Determines how objects should behave when progressing within area clock. 
		/// </summary>
		public AreaClockMode mode
		{
			get { return _mode; }
			set { _mode = value; }
		}

		[SerializeField]
		private AnimationCurve _curve = AnimationCurve.EaseInOut(0, 1, 1, 0);

		/// <summary>
		/// The curve of the area clock. This value is only used for the PointToEdge and DistanceFromEntry modes. 
		/// 
		/// <para>
		/// A good use for this curve is to dampen an area clock's effect to make it seem more natural. Think of the curve's left being the centermost part of the clock, and right being the edge.
		/// </para>
		/// 
		/// <para>
		/// The Y-axis of the curve indicates a multiplier of the clock's time scale, from 1 to -1.
		/// </para>
		/// 
		/// <para>
		/// The X-axis of the curve indicates the object's progress.
		/// </para>
		/// 
		/// <para>
		/// For PointToEdge, 
		/// X = 0 is at the center; 
		/// X = 1 is at the collider's edge.
		/// </para>
		/// 
		/// <para>
		/// For DistanceFromEntry,
		/// X = 1 is at entry, and 
		/// X = 0 is at a distance of padding or more from entry.
		/// </para>
		/// </summary>
		public AnimationCurve curve
		{
			get { return _curve; }
			set { _curve = value; }
		}

		[SerializeField]
		private TVector _center;

		/// <summary>
		/// The center of the area clock. This value is only used for the PointToEdge mode. 
		/// </summary>
		public TVector center
		{
			get { return _center; }
			set { _center = value; }
		}

		[SerializeField]
		private float _padding = 0.5f;

		/// <summary>
		/// The padding of the area clock. This value is only used for the DistanceFromEntry mode. 
		/// </summary>
		public float padding
		{
			get { return _padding; }
			set { _padding = value; }
		}

		[SerializeField]
		private ClockBlend _innerBlend = ClockBlend.Multiplicative;

		/// <summary>
		/// Determines how the clock combines its time scale with that of the timelines within.
		/// </summary>
		public ClockBlend innerBlend
		{
			get { return _innerBlend; }
			set { _innerBlend = value; }
		}

		#endregion

		#region Timelines

		protected virtual void Capture(Timeline timeline, Vector3 entry)
		{
			if (timeline == null) throw new ArgumentNullException("timeline");

			if (!within.ContainsKey(timeline))
			{
				within.Add(timeline, entry);
			}

			if (!timeline.areaClocks.Contains(this))
			{
				timeline.areaClocks.Add(this);
			}
		}

		/// <summary>
		/// Releases the specified timeline from the clock's effects. 
		/// </summary>
		public virtual void Release(Timeline timeline)
		{
			if (timeline == null) throw new ArgumentNullException("timeline");

			if (within.ContainsKey(timeline))
			{
				within.Remove(timeline);
			}

			if (timeline.areaClocks.Contains(this))
			{
				timeline.areaClocks.Remove(this);
			}
		}

		/// <summary>
		/// Releases all timelines within the clock. 
		/// </summary>
		public virtual void ReleaseAll()
		{
			foreach (Timeline timeline in within.Keys.Where(w => w != null))
			{
				if (timeline.areaClocks.Contains(this))
				{
					timeline.areaClocks.Remove(this);
				}
			}

			within.Clear();
		}

		#endregion

		#region Time scale
		
		float IAreaClock.TimeScale(Timeline timeline)
		{
			if (timeline == null) throw new ArgumentNullException("timeline");

			if (mode == AreaClockMode.Instant)
			{
				return timeScale;
			}
			else if (mode == AreaClockMode.PointToEdge)
			{
				Vector3 position = timeline.gameObject.transform.position;
				return PointToEdgeTimeScale(position);
			}
			else if (mode == AreaClockMode.DistanceFromEntry)
			{
				Vector3 position = timeline.gameObject.transform.position;
				Vector3 entry = within[timeline];
				return DistanceFromEntryTimeScale(entry, position);
			}
			else
			{
				throw new ChronosException("Unknown area clock mode.");
			}
		}

		protected virtual float DistanceFromEntryTimeScale(Vector3 entry, Vector3 position)
		{
			if (padding < 0)
			{
				throw new ChronosException("Area clock padding must be positive.");
			}

			entry = transform.TransformPoint(entry); // Get back to global coordinates

			Vector3 delta = position - entry;
			Vector3 direction = delta.normalized;
			float distance = delta.magnitude;
			Vector3 innerEdge = entry + padding * direction;
			float noEffect = innerBlend == ClockBlend.Multiplicative ? 1 : 0;

			if (distance < padding)
			{
				if (Timekeeper.instance.debug)
				{
					Debug.DrawLine(entry, innerEdge, Color.cyan);
					Debug.DrawLine(entry, position, Color.magenta);
				}

				return Mathf.Lerp(noEffect, timeScale, curve.Evaluate(1 - (distance / padding)));
			}
			else
			{
				if (Timekeeper.instance.debug)
				{
					Debug.DrawLine(entry, position, Color.magenta);
				}

				return timeScale;
			}
		}

		protected abstract float PointToEdgeTimeScale(Vector3 position);

		// Required as public because of the interface. Never call directly.
		// http://stackoverflow.com/questions/795108
		public abstract bool ContainsPoint(Vector3 point);

		#endregion

		#region Components

		public virtual void CacheComponents() { }

		#endregion
	}
}
