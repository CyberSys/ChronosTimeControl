using System;
using System.Linq;
using System.Reflection;
using Chronos.Reflection.Internal;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Chronos.Reflection
{
	[Serializable]
	public class UnityMethod : UnityMember, ISerializationCallbackReceiver
	{
		/// <summary>
		/// The underlying reflected method.
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

		public void OnAfterDeserialize()
		{
			if (_parameterTypes != null)
			{
				parameterTypes = _parameterTypes.Select(typeName => TypeSerializer.Deserialize(typeName)).ToArray();
			}
		}

		public void OnBeforeSerialize()
		{
			if (parameterTypes != null)
			{
				_parameterTypes = parameterTypes.Select(type => TypeSerializer.Serialize(type)).ToArray();
			}
		}

		#region Constructors

		public UnityMethod() { }

		public UnityMethod(string name) : base(name) { }
		public UnityMethod(string name, UnityObject target) : base(name, target) { }
		public UnityMethod(string component, string name) : base(component, name) { }
		public UnityMethod(string component, string name, UnityObject target) : base(component, name, target) { }

		public UnityMethod(string name, Type[] parameterTypes) : base(name) { this.parameterTypes = parameterTypes; }
		public UnityMethod(string name, Type[] parameterTypes, UnityObject target) : this(name, parameterTypes) { this.target = target; Reflect(); }
		public UnityMethod(string component, string name, Type[] parameterTypes) : base(component, name) { this.parameterTypes = parameterTypes; }
		public UnityMethod(string component, string name, Type[] parameterTypes, UnityObject target) : this(component, name, parameterTypes) { this.target = target; Reflect(); }

		#endregion

		/// <inheritdoc />
		public override void Reflect()
		{
			EnsureAssigned();
			EnsureTargeted();
			
			methodInfo = UnityMemberHelper.ReflectMethod(reflectionTarget, name, parameterTypes);
			isExtension = methodInfo.IsExtension();
			isReflected = true;
		}

		/// <summary>
		/// Invokes the method with any number of arguments of any type and returns its return value, or null if there isn't any (void).
		/// </summary>
		public object Invoke(params object[] parameters)
		{
			EnsureReflected();

			return UnityMemberHelper.InvokeMethod(reflectionTarget, methodInfo, isExtension, parameters);
		}

		/// <summary>
		/// Invokes the method with any number of arguments of any type and returns its return value casted to the specified type, or null if there isn't any (void).
		/// </summary>
		public T Invoke<T>(params object[] parameters)
		{
			return (T)Invoke(parameters);
		}

		/// <summary>
		/// The return type of the reflected method.
		/// </summary>
		public Type returnType
		{
			get
			{
				EnsureReflected();

				return methodInfo.ReturnType;
			}
		}

		public override bool Corresponds(UnityMember other)
		{
			return
				other is UnityMethod &&
				base.Corresponds(other) &&
				parameterTypes.SequenceEqual(((UnityMethod)other).parameterTypes);
		}
	}
}