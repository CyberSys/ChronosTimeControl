using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chronos
{
	/// <summary>
	/// A global singleton tasked with keeping track of global clocks in the scene. One and only one Timekeeper is required per scene. 
	/// </summary>
	[DisallowMultipleComponent]
	[HelpURL("http://ludiq.io/chronos/documentation#Timekeeper")]
	public class Timekeeper : Singleton<Timekeeper>
	{
		public const int DefaultMaxParticleLoops = 10;

		public Timekeeper()
			: base(false, false)
		{
			_clocks = new Dictionary<string, GlobalClock>();
		}

		protected virtual void Awake()
		{
			foreach (GlobalClock globalClock in GetComponents<GlobalClock>())
			{
				_clocks.Add(globalClock.key, globalClock);
			}
		}

		#region Properties

		[SerializeField]
		private bool _debug = false;

		/// <summary>
		/// Determines whether Chronos should display debug messages and gizmos in the editor. 
		/// </summary>
		public bool debug
		{
			get { return _debug; }
			set { _debug = value; }
		}

		[SerializeField]
		private int _maxParticleLoops = DefaultMaxParticleLoops;

		/// <summary>
		/// The maximum loops during which particle systems should be allowed to run before resetting.
		/// </summary>
		public int maxParticleLoops
		{
			get { return _maxParticleLoops; }
			set { _maxParticleLoops = value; }
		}

		#endregion

		#region Clocks

		protected Dictionary<string, GlobalClock> _clocks;

		/// <summary>
		/// An enumeration of all the global clocks on the timekeeper.
		/// </summary>
		public IEnumerable<GlobalClock> clocks
		{
			get { return _clocks.Values; }
		}

		/// <summary>
		/// Determines whether the timekeeper has a global clock with the specified key. 
		/// </summary>
		public virtual bool HasClock(string key)
		{
			if (key == null) throw new ArgumentNullException("key");

			return _clocks.ContainsKey(key);
		}

		/// <summary>
		/// Returns the global clock with the specified key. 
		/// </summary>
		public virtual GlobalClock Clock(string key)
		{
			if (key == null) throw new ArgumentNullException("key");

			if (!HasClock(key))
			{
				throw new ChronosException(string.Format("Unknown global clock '{0}'.", key));
			}

			return _clocks[key];
		}

		/// <summary>
		/// Adds a global clock with the specified key and returns it.
		/// </summary>
		public virtual GlobalClock AddClock(string key)
		{
			if (key == null) throw new ArgumentNullException("key");

			if (HasClock(key))
			{
				throw new ChronosException(string.Format("Global clock '{0}' already exists.", key));
			}

			GlobalClock clock = gameObject.AddComponent<GlobalClock>();
			clock.key = key;
			_clocks.Add(key, clock);
			return clock;
		}

		/// <summary>
		/// Removes the global clock with the specified key.
		/// </summary>
		public virtual void RemoveClock(string key)
		{
			if (key == null) throw new ArgumentNullException("key");

			if (!HasClock(key))
			{
				throw new ChronosException(string.Format("Unknown global clock '{0}'.", key));
			}

			_clocks.Remove(key);
		}

		#endregion

		internal static TimeState GetTimeState(float timeScale)
		{
			if (timeScale < 0)
			{
				return TimeState.Reversed;
			}
			else if (timeScale == 0)
			{
				return TimeState.Paused;
			}
			else if (timeScale < 1)
			{
				return TimeState.Slowed;
			}
			else if (timeScale == 1)
			{
				return TimeState.Normal;
			}
			else // if (timeScale > 1)
			{
				return TimeState.Accelerated;
			}
		}

		// Unscaled delta time does not automatically behave like delta time.
		// Hacky workaround for now, fixing 2 / 3 issues
		// See: http://forum.unity3d.com/threads/138432/#post-2251561
		internal static float unscaledDeltaTime
		{
			get
			{
				if (Time.frameCount <= 2)
				{
					return 0.02f;
				}
				else
				{
					return Mathf.Min(Time.unscaledDeltaTime, Time.maximumDeltaTime);
				}
			}
		}
	}
}
