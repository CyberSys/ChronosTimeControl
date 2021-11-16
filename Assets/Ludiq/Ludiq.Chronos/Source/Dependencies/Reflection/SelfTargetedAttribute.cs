using System;

namespace Chronos.Reflection
{
	/// <summary>
	/// Indicates that the UnityMember has itself as a target.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class SelfTargetedAttribute : Attribute
	{

	}
}