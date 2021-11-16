using System;
using System.Reflection;
using Chronos.Reflection.Internal;
using UnityObject = UnityEngine.Object;

namespace Chronos.Reflection
{
	[Serializable]
	public class UnityVariable : UnityMember
	{
		private enum SourceType
		{
			Unknown,
			Field,
			Property
		}

		private SourceType sourceType = SourceType.Unknown;

		/// <summary>
		/// The underlying reflected field, or null if the variable is a property.
		/// </summary>
		public FieldInfo fieldInfo { get; private set; }

		/// <summary>
		/// The underlying property field, or null if the variable is a field.
		/// </summary>
		public PropertyInfo propertyInfo { get; private set; }

		#region Constructors

		public UnityVariable() { }
		public UnityVariable(string name) : base(name) { }
		public UnityVariable(string name, UnityObject target) : base(name, target) { }
		public UnityVariable(string component, string name) : base(component, name) { }
		public UnityVariable(string component, string name, UnityObject target) : base(component, name, target) { }

		#endregion

		/// <inheritdoc />
		public override void Reflect()
		{
			EnsureAssigned();
			EnsureTargeted();

			fieldInfo = null;
			propertyInfo = null;
			sourceType = SourceType.Unknown;

			var memberInfo = UnityMemberHelper.ReflectVariable(reflectionTarget, name);
			fieldInfo = memberInfo as FieldInfo;
			propertyInfo = memberInfo as PropertyInfo;

			if (fieldInfo != null)
			{
				sourceType = SourceType.Field;
			}
			else if (propertyInfo != null)
			{
				sourceType = SourceType.Property;
			}

			isReflected = true;
		}

		/// <summary>
		/// Retrieves the value of the variable.
		/// </summary>
		public object Get()
		{
			EnsureReflected();

			switch (sourceType)
			{
				case SourceType.Field: return fieldInfo.GetValue(reflectionTarget);
				case SourceType.Property: return propertyInfo.GetValue(reflectionTarget, null);
				default: throw new UnityReflectionException();
			}
		}

		/// <summary>
		/// Retrieves the value of the variable casted to the specified type.
		/// </summary>
		public T Get<T>()
		{
			return (T)Get();
		}

		/// <summary>
		/// Assigns a new value to the variable.
		/// </summary>
		public void Set(object value)
		{
			EnsureReflected();

			switch (sourceType)
			{
				case SourceType.Field: fieldInfo.SetValue(reflectionTarget, value); break;
				case SourceType.Property: propertyInfo.SetValue(reflectionTarget, value, null); break;
				default: throw new UnityReflectionException();
			}
		}

		/// <summary>
		/// The type of the reflected field or property.
		/// </summary>
		public Type type
		{
			get
			{
				EnsureReflected();

				switch (sourceType)
				{
					case SourceType.Field: return fieldInfo.FieldType;
					case SourceType.Property: return propertyInfo.PropertyType;
					default: throw new UnityReflectionException();
				}
			}
		}

		public override bool Corresponds(UnityMember other)
		{
			return other is UnityVariable && base.Corresponds(other);
		}
	}
}