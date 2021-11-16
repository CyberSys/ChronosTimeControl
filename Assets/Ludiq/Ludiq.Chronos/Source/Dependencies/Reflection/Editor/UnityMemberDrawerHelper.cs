using System;
using System.Linq;
using System.Reflection;
using Chronos.Controls.Editor;
using Chronos.Reflection.Internal;
using UnityEditor;

namespace Chronos.Reflection.Editor
{
	public static class UnityMemberDrawerHelper
	{
		public static bool ValidateMemberType(FilterAttribute filter, Type type)
		{
			bool validFamily = false;
			bool validType;

			// Allow type families based on the filter attribute

			TypeFamily families = filter.TypeFamilies;

			if (families.HasFlag(TypeFamily.Array)) validFamily |= type.IsArray;
			if (families.HasFlag(TypeFamily.Class)) validFamily |= type.IsClass;
			if (families.HasFlag(TypeFamily.Enum)) validFamily |= type.IsEnum;
			if (families.HasFlag(TypeFamily.Interface)) validFamily |= type.IsInterface;
			if (families.HasFlag(TypeFamily.Primitive)) validFamily |= type.IsPrimitive;
			if (families.HasFlag(TypeFamily.Reference)) validFamily |= !type.IsValueType;
			if (families.HasFlag(TypeFamily.Value)) validFamily |= (type.IsValueType && type != typeof(void));
			if (families.HasFlag(TypeFamily.Void)) validFamily |= type == typeof(void);

			// Allow types based on the filter attribute
			// If no filter types are specified, all types are allowed.

			if (filter.Types.Count > 0)
			{
				validType = false;

				foreach (Type allowedType in filter.Types)
				{
					if (allowedType.IsAssignableFrom(type))
					{
						validType = true;
						break;
					}
				}
			}
			else
			{
				validType = true;
			}

			return validFamily && validType;
		}

		public static bool ValidateField(FilterAttribute filter, FieldInfo field)
		{
			bool valid = true;

			// Validate type based on field type
			valid &= ValidateMemberType(filter, field.FieldType);

			// Exclude constants (literal) and readonly (init) fields if
			// the filter rejects read-only fields.
			if (!filter.ReadOnly) valid &= !field.IsLiteral || !field.IsInitOnly;

			return valid;
		}

		public static bool ValidateProperty(FilterAttribute filter, PropertyInfo property)
		{
			bool valid = true;

			// Validate type based on property type
			valid &= ValidateMemberType(filter, property.PropertyType);

			// Exclude read-only and write-only properties
			if (!filter.ReadOnly) valid &= property.CanWrite;
			if (!filter.WriteOnly) valid &= property.CanRead;

			return valid;
		}

		public static bool ValidateMethod(FilterAttribute filter, MethodInfo method)
		{
			bool valid = true;

			// Validate type based on return type
			valid &= ValidateMemberType(filter, method.ReturnType);

			// Exclude special compiler methods
			valid &= !method.IsSpecialName;

			// Exclude generic methods
			// TODO: Generic type (de)serialization
			valid &= !method.ContainsGenericParameters;

			// Exclude methods with parameters
			if (!filter.Parameters)
			{
				valid &= method.GetParameters().Length == 0;
			}

			return valid;
		}

		public static void SerializeParameterTypes(SerializedProperty parameterTypesProperty, Type[] parameterTypes)
		{
			// Assign the parameter types to their underlying properties

			if (parameterTypes == null)
			{
				parameterTypesProperty.arraySize = 0;
			}
			else
			{
				parameterTypesProperty.arraySize = parameterTypes.Length;

				for (int i = 0; i < parameterTypesProperty.arraySize; i++)
				{
					SerializedProperty parameterTypeProperty = parameterTypesProperty.GetArrayElementAtIndex(i);

					parameterTypeProperty.stringValue = TypeSerializer.Serialize(parameterTypes[i]);
				}
			}
		}

		public static Type[] DeserializeParameterTypes(SerializedProperty parameterTypesProperty)
		{
			// Fetch the parameter types from their underlying properties

			Type[] parameterTypes = new Type[parameterTypesProperty.arraySize];

			for (int i = 0; i < parameterTypesProperty.arraySize; i++)
			{
				SerializedProperty parameterTypeProperty = parameterTypesProperty.GetArrayElementAtIndex(i);

				parameterTypes[i] = TypeSerializer.Deserialize(parameterTypeProperty.stringValue);
			}

			return parameterTypes;
		}

		public static bool ParameterTypesHasMultipleValues(SerializedProperty parameterTypesProperty)
		{
			string[] last = null;

			foreach (SerializedProperty arrayProperty in parameterTypesProperty.Multiple())
			{
				string[] current = new string[arrayProperty.arraySize];

				for (int i = 0; i < arrayProperty.arraySize; i++)
				{
					SerializedProperty parameterTypeProperty = arrayProperty.GetArrayElementAtIndex(i);
					current[i] = parameterTypeProperty.stringValue;
				}

				if (last != null && !current.SequenceEqual(last))
				{
					return true;
				}

				last = current;
			}

			return false;
		}
	}
}