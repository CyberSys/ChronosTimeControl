using System;
using UnityEngine;

namespace Chronos
{
	/// <summary>
	/// Determines how a clock combines its time scale with that of others.
	/// </summary>
	public enum ClockBlend
	{
		/// <summary>
		/// The clock's time scale is multiplied with that of others.
		/// </summary>
		Multiplicative = 0,

		/// <summary>
		/// The clock's time scale is added to that of others.
		/// </summary>
		Additive = 1
	}

	/// <summary>
	/// An abstract base component that provides common timing functionality to all types of clocks. 
	/// </summary>
	[HelpURL("http://ludiq.io/chronos/documentation#Clock")]
	public abstract class Clock : MonoBehaviour
	{
		private bool enabledOnce;

		protected virtual void Awake() { }
		
		private void Start()
		{
			OnStartOrReEnable();
		}

		private void OnEnable()
		{
			if (enabledOnce)
			{
				OnStartOrReEnable();
			}
			else
			{
				enabledOnce = true;
			}
		}

		protected virtual void OnStartOrReEnable()
		{
			if (string.IsNullOrEmpty(_parentKey))
			{
				parent = null;
			}
			else if (Timekeeper.instance.HasClock(_parentKey))
			{
				parent = Timekeeper.instance.Clock(_parentKey);
			}
			else
			{
				throw new ChronosException(string.Format("Missing parent clock: '{0}'.", _parentKey));
			}

			startTime = Time.unscaledTime;

			if (parent != null)
			{
				parent.Register(this);
				parent.ComputeTimeScale();
			}

			ComputeTimeScale();
		}

		protected virtual void Update()
		{
			if (isLerping)
			{
				_localTimeScale = Mathf.Lerp(lerpFrom, lerpTo, (unscaledTime - lerpStart) / (lerpEnd - lerpStart));

				if (unscaledTime >= lerpEnd)
				{
					isLerping = false;
				}
			}

			ComputeTimeScale();

			float unscaledDeltaTime = Timekeeper.unscaledDeltaTime;
			deltaTime = unscaledDeltaTime * timeScale;
			fixedDeltaTime = Time.fixedDeltaTime * timeScale;
			time += deltaTime;
			unscaledTime += unscaledDeltaTime;
		}

		protected virtual void OnDisable()
		{
			if (parent != null)
			{
				parent.Unregister(this);
			}
		}

		#region Fields

		protected bool isLerping;
		protected float lerpStart;
		protected float lerpEnd;
		protected float lerpFrom;
		protected float lerpTo;

		#endregion

		#region Properties

		[SerializeField]
		private float _localTimeScale = 1;

		/// <summary>
		/// The scale at which the time is passing for the clock. This can be used for slow motion, acceleration, pause or even rewind effects. 
		/// </summary>
		public float localTimeScale
		{
			get { return _localTimeScale; }
			set { _localTimeScale = value; }
		}

		/// <summary>
		/// The computed time scale of the clock. This value takes into consideration all of the clock's parameters (parent, paused, etc.) and multiplies their effect accordingly. 
		/// </summary>
		public float timeScale { get; protected set; }

		/// <summary>
		/// The time in seconds since the creation of the clock, affected by the time scale. Returns the same value if called multiple times in a single frame.
		/// Unlike Time.time, this value will not return Time.fixedTime when called inside MonoBehaviour's FixedUpdate. 
		/// </summary>
		public float time { get; protected set; }

		/// <summary>
		/// The time in seconds since the creation of the clock, regardless of the time scale. Returns the same value if called multiple times in a single frame. 
		/// </summary>
		public float unscaledTime { get; protected set; }

		/// <summary>
		/// The time in seconds it took to complete the last frame, multiplied by the time scale. Returns the same value if called multiple times in a single frame. 
		/// Unlike Time.deltaTime, this value will not return Time.fixedDeltaTime when called inside MonoBehaviour's FixedUpdate. 
		/// </summary>
		public float deltaTime { get; protected set; }

		/// <summary>
		/// The interval in seconds at which physics and other fixed frame rate updates, multiplied by the time scale.
		/// </summary>
		public float fixedDeltaTime { get; protected set; }

		/// <summary>
		/// The unscaled time in seconds between the start of the game and the creation of the clock. 
		/// </summary>
		public float startTime { get; protected set; }

		[SerializeField]
		private bool _paused;

		/// <summary>
		/// Determines whether the clock is paused. This toggle is especially useful if you want to pause a clock without having to worry about storing its previous time scale to restore it afterwards. 
		/// </summary>
		public bool paused
		{
			get { return _paused; }
			set { _paused = value; }
		}

		[SerializeField, GlobalClock]
		private string _parentKey;

		private GlobalClock _parent;

		/// <summary>
		/// The parent global clock. The parent clock will multiply its time scale with all of its children, allowing for cascading time effects.
		/// </summary>
		public GlobalClock parent
		{
			get { return _parent; }
			set
			{
				if (_parent != null)
				{
					_parent.Unregister(this);
				}

				if (value != null)
				{
					if (value == this)
					{
						throw new ChronosException("Global clock parent cannot be itself.");
					}

					_parentKey = value.key;
					_parent = value;
					_parent.Register(this);
				}
				else
				{
					_parentKey = null;
					_parent = null;
				}
			}
		}

		[SerializeField]
		private ClockBlend _parentBlend = ClockBlend.Multiplicative;

		/// <summary>
		/// Determines how the clock combines its time scale with that of its parent.
		/// </summary>
		public ClockBlend parentBlend
		{
			get { return _parentBlend; }
			set { _parentBlend = value; }
		}

		/// <summary>
		/// Indicates the state of the clock. 
		/// </summary>
		public TimeState state
		{
			get { return Timekeeper.GetTimeState(timeScale); }
		}

		#endregion

		/// <summary>
		/// The time scale is only computed once per update to improve performance. If you need to ensure it is correct in the same frame, call this method to compute it manually.
		/// </summary>
		public virtual void ComputeTimeScale()
		{
			if (paused)
			{
				timeScale = 0;
			}
			else if (parent == null)
			{
				timeScale = localTimeScale;
			}
			else
			{
				if (parentBlend == ClockBlend.Multiplicative)
				{
					timeScale = parent.timeScale * localTimeScale;
				}
				else // if (combinationMode == ClockCombinationMode.Additive)
				{
					timeScale = parent.timeScale + localTimeScale;
				}
			}
		}

		/// <summary>
		/// Changes the local time scale smoothly over the given duration in seconds.
		/// </summary>
		public void LerpTimeScale(float timeScale, float duration, bool steady = false)
		{
			if (duration < 0) throw new ArgumentException("Duration must be positive.", "duration");

			if (duration == 0)
			{
				localTimeScale = timeScale;
				isLerping = false;
				return;
			}

			float modifier = 1;

			if (steady)
			{
				modifier = Mathf.Abs(localTimeScale - timeScale);
			}

			if (modifier != 0)
			{
				lerpFrom = localTimeScale;
				lerpStart = unscaledTime;

				lerpTo = timeScale;
				lerpEnd = lerpStart + (duration * modifier);

				isLerping = true;
			}
		}
	}
}
