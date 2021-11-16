using System;
using System.Collections.Generic;

namespace Chronos.Reflection
{
	/// <summary>
	/// Filters the list of members displayed in the inspector drawer.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public sealed class FilterAttribute : Attribute
	{
		/// <summary>
		/// Whether to display members defined in the types's ancestors.
		/// </summary>
		public bool Inherited { get; set; }

		/// <summary>
		/// Whether to display instance members.
		/// </summary>
		public bool Instance { get; set; }

		/// <summary>
		/// Whether to display static members.
		/// </summary>
		public bool Static { get; set; }

		/// <summary>
		/// Whether to display public members.
		/// </summary>
		public bool Public { get; set; }

		/// <summary>
		/// Whether to display private and protected members.
		/// </summary>
		public bool NonPublic { get; set; }

		/// <summary>
		/// Whether to display read-only fields and properties.
		/// </summary>
		public bool ReadOnly { get; set; }

		/// <summary>
		/// Whether to display write-only properties.
		/// </summary>
		public bool WriteOnly { get; set; }

		/// <summary>
		/// Whether to display extension methods.
		/// </summary>
		public bool Extension { get; set; }

		/// <summary>
		/// Whether to display methods with parameters.
		/// </summary>
		public bool Parameters { get; set; }

		/// <summary>
		/// The type families to display.
		/// </summary>
		public TypeFamily TypeFamilies { get; set; }

		private readonly List<Type> types;
		/// <summary>
		/// The types to display, or empty for any.
		/// </summary>
		public List<Type> Types
		{
			get { return types; }
		}

		/// <summary>
		/// Filters the list of members displayed in the inspector drawer.
		/// </summary>
		/// <param name="types">The types to display, or none for any.</param>
		public FilterAttribute(params Type[] types)
		{
			this.types = new List<Type>(types);

			Inherited = false;
			Instance = true;
			Static = false;
			Public = true;
			NonPublic = false;
			ReadOnly = true;
			WriteOnly = true;
			Extension = true;
			Parameters = true;
			TypeFamilies = TypeFamily.All;
		}
	}
}