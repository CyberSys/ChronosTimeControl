using UnityEngine;

namespace Chronos
{
	internal sealed class ForwardActionOccurence : Occurrence
	{
		private ForwardAction forward { get; set; }

		public ForwardActionOccurence(ForwardAction forward)
		{
			this.forward = forward;
		}

		public override void Forward()
		{
			forward();
		}

		public override void Backward()
		{
			Debug.LogWarning("Trying to revert a forward-only occurrence.");
		}
	}
}
