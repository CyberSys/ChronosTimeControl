namespace Chronos
{
	/// <summary>
	/// An occurrence action that is executed when time goes forward. Returns any object that will be transfered to the backward action if time is rewinded.
	/// </summary>
	public delegate T ForwardFunc<T>();

	/// <summary>
	/// An occurrence action that is executed when time goes backward. Uses the object returned by the forward action to remember state information.
	/// </summary>
	public delegate void BackwardFunc<T>(T transfer);

	internal sealed class FuncOccurence<T> : Occurrence
	{
		private ForwardFunc<T> forward { get; set; }
		private BackwardFunc<T> backward { get; set; }
		private T transfer { get; set; }

		public FuncOccurence(ForwardFunc<T> forward, BackwardFunc<T> backward)
		{
			this.forward = forward;
			this.backward = backward;
		}

		public override void Forward()
		{
			transfer = forward();
		}

		public override void Backward()
		{
			backward(transfer);
		}
	}
}
