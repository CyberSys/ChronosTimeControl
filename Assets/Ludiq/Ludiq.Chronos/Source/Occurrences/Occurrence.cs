namespace Chronos
{
	/// <summary>
	/// An event anchored at a specified moment in time composed of two actions: one when time goes forward, and another when time goes backward. The latter is most often used to revert the former. 
	/// </summary>
	public abstract class Occurrence
	{
		/// <summary>
		/// The time in seconds on the parent timeline at which the occurrence will happen. 
		/// </summary>
		public float time { get; internal set; }

		/// <summary>
		/// Indicates whether this occurrence can happen more than once; that is, whether it will stay on the timeline once it has been rewound. 
		/// </summary>
		public bool repeatable { get; internal set; }

		/// <summary>
		/// The action that is executed when time goes forward.
		/// </summary>
		public abstract void Forward();

		/// <summary>
		/// The action that is executed when time goes backward.
		/// </summary>
		public abstract void Backward();
	}
}
