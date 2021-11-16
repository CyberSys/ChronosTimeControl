namespace Chronos
{
	/// <summary>
	/// Indicates the state of time.
	/// </summary>
	public enum TimeState
	{
		/// <summary>
		/// Time is accelerated (time scale &gt; 1).
		/// </summary>
		Accelerated,

		/// <summary>
		/// Time is in real-time (time scale = 1).
		/// </summary>
		Normal,

		/// <summary>
		/// Time is slowed (0 &lt; time scale &lt; 1).
		/// </summary>
		Slowed,

		/// <summary>
		/// Time is paused (time scale = 0).
		/// </summary>
		Paused,

		/// <summary>
		/// Time is reversed (time scale &lt; 0).
		/// </summary>
		Reversed
	}
}
