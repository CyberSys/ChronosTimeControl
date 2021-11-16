using System;

namespace Chronos.Reflection
{
	/// <summary>
	/// An exception thrown by the Ludiq Reflection plugin.
	/// </summary>
	public class UnityReflectionException : Exception
	{
		public UnityReflectionException() : base() { }
		public UnityReflectionException(string message) : base(message) { }
		public UnityReflectionException(string message, Exception innerException) : base(message, innerException) { }
	}
}
