using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Chronos.Reflection.Internal
{
	public static class Extensions
	{
		/// <summary>
		/// Finds the intersection of a group of groups.
		/// </summary>
		public static IEnumerable<T> IntersectAll<T>(this IEnumerable<IEnumerable<T>> groups)
		{
			HashSet<T> hashSet = null;

			foreach (IEnumerable<T> group in groups)
			{
				if (hashSet == null)
				{
					hashSet = new HashSet<T>(group);
				}
				else
				{
					hashSet.IntersectWith(group);
				}
			}

			return hashSet == null ? Enumerable.Empty<T>() : hashSet.AsEnumerable();
		}

		/// <summary>
		/// Determines if an enum has the given flag defined bitwise.
		/// Fallback equivalent to .NET's Enum.HasFlag().
		/// </summary>
		public static bool HasFlag(this Enum value, Enum flag)
		{
			long lValue = Convert.ToInt64(value);
			long lFlag = Convert.ToInt64(flag);
			return (lValue & lFlag) != 0;
		}

#if !NETFX_CORE
		private static MethodInfo[] extensionMethodsCache;

		/// <summary>
		/// Searches all assemblies for extension methods for a given type.
		/// </summary>
		public static IEnumerable<MethodInfo> GetExtensionMethods(this Type type)
		{
			// http://stackoverflow.com/a/299526

			if (extensionMethodsCache == null)
			{
				extensionMethodsCache = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(potentialType => potentialType.IsSealed && !potentialType.IsGenericType && !potentialType.IsNested)
				.SelectMany(extensionType => extensionType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
				.Where(method => method.IsExtension())
				.ToArray();
			}

			return extensionMethodsCache.Where(method => method.GetParameters()[0].ParameterType.IsAssignableFrom(type));
		}
#endif

		public static bool IsExtension(this MethodInfo methodInfo)
		{
			return methodInfo.IsDefined(typeof(ExtensionAttribute), false);
		}
	}
}