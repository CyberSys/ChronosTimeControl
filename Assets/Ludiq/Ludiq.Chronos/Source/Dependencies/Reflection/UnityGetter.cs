using System;
using System.Linq;
using System.Reflection;
using Chronos.Reflection.Internal;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Chronos.Reflection
{
	[Serializable]
	public class UnityGetter : UnityMember
	{
		private enum SourceType
		{
			Unknown,
			Field,
			Property,
			Method
		}

		private SourceType sourceType = SourceType.Unknown;

		/// <summary>
		/// The underlying reflected field, or null if the getter is a property or method.
		/// </summary>
		public FieldInfo fieldInfo { get; private set; }

		/// <summary>
		/// The underlying property field, or null if the getter is a field or method.
		/// </summary>
		public PropertyInfo propertyInfo { get; private set; }

		/// <summary>
		/// The underlying reflected method, or null if the getter is a field or property.
		/// </summary>
		public MethodInfo methodInfo { get; private set; }

		/// <summary>
		/// Whether the reflected method is an extension method.
		/// </summary>
		public bool isExtension { get; private set; }

		[SerializeField]
		private string[] _parameterTypes;
		private Type[] __parameterTypes;
		/// <summary>
		/// The types of the method's parameters.
		/// </summary>
		public Type[] parameterTypes
		{
			get { return __parameterTypes; }
			set { __parameterTypes = value; isReflected = false; }
		}

		#region Constructors

		public UnityGetter() { }

		public UnityGetter(string name) : base(name) { }
		public UnityGetter(string name, UnityObject target) : base(name, target) { }
		public UnityGetter(string component, string name) : base(component, name) { }
		public UnityGetter(string component, string name, UnityObject target) : base(component, name, target) { }

		public UnityGetter(string name, Type[] parameterTypes) : base(name) { this.parameterTypes = parameterTypes; }
		public UnityGetter(string name, Type[] parameterTypes, UnityObject target) : this(name, parameterTypes) { this.target = target; Reflect(); }
		public UnityGetter(string component, string name, Type[] parameterTypes) : base(component, name) { this.parameterTypes = parameterTypes; }
		public UnityGetter(string component, string name, Type[] parameterTypes, UnityObject target) : this(component, name, parameterTypes) { this.target = target; Reflect(); }

		#endregion

		/// <inheritdoc />
		public override void Reflect()
		{
			EnsureAssigned();
			EnsureTargeted();

			this.fieldInfo = null;
			this.propertyInfo = null;
			this.methodInfo = null;
			this.sourceType = SourceType.Unknown;

			MemberInfo variableInfo;
			MethodInfo methodInfo;
			UnityReflectionException exception;

			if (UnityMemberHelper.TryReflectVariable(out variableInfo, out exception, reflectionTarget, name))
			{
				fieldInfo = variableInfo as FieldInfo;
				propertyInfo = variableInfo as PropertyInfo;

				if (fieldInfo != null)
				{
					sourceType = SourceType.Field;
				}
				else if (propertyInfo != null)
				{
					sourceType = SourceType.Property;
				}
			}
			else if (UnityMemberHelper.TryReflectMethod(out methodInfo, out exception, reflectionTarget, name, parameterTypes))
			{
				this.methodInfo = methodInfo;
				isExtension = methodInfo.IsExtension();

				sourceType = SourceType.Method;
			}
			else
			{
				throw new UnityReflectionException("No matching field, property or method found.");
			}

			isReflected = true;
		}
		
		/// <summary>
		/// Retrieves the value of the getter.
		/// </summary>
		public object Get(params object[] parameters)
		{
			EnsureReflected();

			switch (sourceType)
			{
				case SourceType.Field: return fieldInfo.GetValue(reflectionTarget);
				case SourceType.Property: return propertyInfo.GetValue(reflectionTarget, null);
				case SourceType.Method: return UnityMemberHelper.InvokeMethod(reflectionTarget, methodInfo, isExtension, parameters);
				default: throw new UnityReflectionException();
			}
		}

		/// <summary>
		/// Retrieves the value of the getter casted to the specified type.
		/// </summary>
		public T Get<T>(params object[] parameters)
		{
			return (T)Get(parameters);
		}

		/// <summary>
		/// The return type of the reflected field, property of method.
		/// </summary>
		public Type returnType
		{
			get
			{
				EnsureReflected();

				switch (sourceType)
				{
					case SourceType.Field: return fieldInfo.FieldType;
					case SourceType.Property: return propertyInfo.PropertyType;
					case SourceType.Method: return methodInfo.ReturnType;
					default: throw new UnityReflectionException();
				}
			}
		}

		public override bool Corresponds(UnityMember other)
		{
			var corresponds = other is UnityGetter && base.Corresponds(other);
			
			corresponds &= (parameterTypes == null) == (((UnityGetter)other).parameterTypes == null);

			if (parameterTypes != null)
			{
				corresponds &= parameterTypes.SequenceEqual(((UnityGetter)other).parameterTypes);
			}

			return corresponds;
		}
	}
}