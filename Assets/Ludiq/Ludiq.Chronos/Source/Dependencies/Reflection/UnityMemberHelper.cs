using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityObject = UnityEngine.Object;

namespace Chronos.Reflection.Internal
{
	internal static class UnityMemberHelper
	{
		internal static bool TryReflectMethod(out MethodInfo methodInfo, out UnityReflectionException exception, UnityObject reflectionTarget, string name, Type[] parameterTypes)
		{
#if !NETFX_CORE
			methodInfo = null;

			Type type = reflectionTarget.GetType();
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

			if (parameterTypes != null) // Explicit matching
			{
				methodInfo = type.GetMethod(name, flags, null, parameterTypes, null);

				if (methodInfo == null)
				{
					methodInfo = type.GetExtensionMethods()
						.Where(extension => extension.Name == name)
						.Where(extension => Enumerable.SequenceEqual(extension.GetParameters().Select(paramInfo => paramInfo.ParameterType), parameterTypes))
						.FirstOrDefault();
				}

				if (methodInfo == null)
				{
					exception = new UnityReflectionException(string.Format("No matching method found: '{0}.{1} ({2})'", type.Name, name, string.Join(", ", parameterTypes.Select(t => t.Name).ToArray())));
					return false;
				}
			}
			else // Implicit matching
			{
				var normalMethods = type.GetMember(name, MemberTypes.Method, flags).OfType<MethodInfo>().ToList();
				var extensionMethods = type.GetExtensionMethods().Where(extension => extension.Name == name).ToList();
				var methods = new List<MethodInfo>();
				methods.AddRange(normalMethods);
				methods.AddRange(extensionMethods);

				if (methods.Count == 0)
				{
					exception = new UnityReflectionException(string.Format("No matching method found: '{0}.{1}'", type.Name, name));
					return false;
				}

				if (methods.Count > 1)
				{
					exception = new UnityReflectionException(string.Format("Multiple method signatures found for '{0}.{1}'\nSpecify the parameter types explicitly.", type.FullName, name));
					return false;
				}

				methodInfo = methods[0];
			}

			exception = null;
			return true;
#else
			throw new Exception("Reflection is not supported in .NET Core.");
#endif
		}

		internal static MethodInfo ReflectMethod(UnityObject reflectionTarget, string name, Type[] parameterTypes)
		{
			MethodInfo methodInfo;
			UnityReflectionException exception;

			if (!TryReflectMethod(out methodInfo, out exception, reflectionTarget, name, parameterTypes))
            {
				throw exception;
			}

			return methodInfo;
		}

		internal static object InvokeMethod(UnityObject reflectionTarget, MethodInfo methodInfo, bool isExtension, params object[] parameters)
		{
			if (isExtension)
			{
				var fullParameters = new object[parameters.Length + 1];
				fullParameters[0] = reflectionTarget;
				Array.Copy(parameters, 0, fullParameters, 1, parameters.Length);
				parameters = fullParameters;
			}

			return methodInfo.Invoke(reflectionTarget, parameters);
		}

		internal static bool TryReflectVariable(out MemberInfo variableInfo, out UnityReflectionException exception, UnityObject reflectionTarget, string name)
		{
#if !NETFX_CORE
			variableInfo = null;

			Type type = reflectionTarget.GetType();
			MemberTypes types = MemberTypes.Property | MemberTypes.Field;
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

			MemberInfo[] variables = type.GetMember(name, types, flags);

			if (variables.Length == 0)
			{
				exception = new UnityReflectionException(string.Format("No matching field or property found: '{0}.{1}'", type.Name, name));
				return false;
			}

			variableInfo = variables[0]; // Safe, because there can't possibly be more than one variable of the same name
			exception = null;
			return true;
#else
			throw new Exception("Reflection is not supported in .NET Core.");
#endif
		}

		internal static MemberInfo ReflectVariable(UnityObject reflectionTarget, string name)
		{
			MemberInfo variableInfo;
			UnityReflectionException exception;

			if (!TryReflectVariable(out variableInfo, out exception, reflectionTarget, name))
			{
				throw exception;
			}

			return variableInfo;
		}
	}
}