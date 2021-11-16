using System;

namespace Chronos.Reflection
{
	/// <summary>
	/// A simple representation of different groups of .NET types.
	/// </summary>
	[Flags]
	public enum TypeFamily
	{
		/// <summary>
		/// No type
		/// </summary>
		None = 0,

		/// <summary>
		/// All types
		/// </summary>
		All = ~0,

		/// <summary>
		/// Value types
		/// </summary>
		Value = 1,

		/// <summary>
		/// Reference types
		/// </summary>
		Reference = 2,

		/// <summary>
		/// Primitive types
		/// </summary>
		Primitive = 4,

		/// <summary>
		/// Arrays
		/// </summary>
		Array = 8,

		/// <summary>
		/// Classes
		/// </summary>
		Class = 16,

		/// <summary>
		/// Enumerations
		/// </summary>
		Enum = 32,
		
		/// <summary>
		/// Interfaces
		/// </summary>
		Interface = 64,

		/// <summary>
		/// Void
		/// </summary>
		Void = 128,
	}
}