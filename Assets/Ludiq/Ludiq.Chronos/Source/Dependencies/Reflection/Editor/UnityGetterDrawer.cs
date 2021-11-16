using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chronos.Controls.Editor;
using Chronos.Reflection.Internal;
using UnityEditor;

namespace Chronos.Reflection.Editor
{
	[CustomPropertyDrawer(typeof(UnityGetter))]
	public class UnityGetterDrawer : UnityMemberDrawer<UnityGetter>
	{
		#region Filtering

		/// <inheritdoc />
		protected override FilterAttribute DefaultFilter()
		{
			FilterAttribute filter = base.DefaultFilter();

			// Override defaults here

			return filter;
		}

		// Do not edit below

		/// <inheritdoc />
		protected override MemberTypes validMemberTypes
		{
			get
			{
				return MemberTypes.Field | MemberTypes.Property | MemberTypes.Method;
			}
		}

		/// <inheritdoc />
		protected override bool ValidateMember(MemberInfo member)
		{
			bool valid = base.ValidateMember(member);

			FieldInfo field = member as FieldInfo;
			PropertyInfo property = member as PropertyInfo;
			MethodInfo method = member as MethodInfo;
			
			if (field != null) // Member is a field
			{
				valid &= UnityMemberDrawerHelper.ValidateField(filter, field);
			}
			else if (property != null) // Member is a property
			{
				valid &= UnityMemberDrawerHelper.ValidateProperty(filter, property);
			}
			else if (method != null) // Member is a method
			{
				valid &= UnityMemberDrawerHelper.ValidateMethod(filter, method);
			}

			return valid;
		}

		#endregion

		#region Fields

		/// <summary>
		/// The UnityGetter.parameterTypes of the inspected property, of type Type[].
		/// </summary>
		protected SerializedProperty parameterTypesProperty;

		protected override void Update(SerializedProperty property)
		{
			base.Update(property);

			parameterTypesProperty = property.FindPropertyRelative("_parameterTypes");
		}

		#endregion

		#region Value

		/// <inheritdoc />
		protected override void SetValue(UnityGetter value)
		{
			base.SetValue(value);

			UnityMemberDrawerHelper.SerializeParameterTypes(parameterTypesProperty, value != null ? value.parameterTypes : null);
		}

		/// <inheritdoc />
		protected override UnityGetter BuildValue(string component, string name)
		{
			return new UnityGetter(component, name, UnityMemberDrawerHelper.DeserializeParameterTypes(parameterTypesProperty));
		}

		/// <inheritdoc />
		protected override bool hasMultipleDifferentValues
		{
			get
			{
				if (base.hasMultipleDifferentValues)
				{
					return true;
				}

				return UnityMemberDrawerHelper.ParameterTypesHasMultipleValues(parameterTypesProperty);
			}
		}

		#endregion

		#region Reflection

		protected override List<DropdownOption<UnityGetter>> GetMemberOptions(Type type, string component = null)
		{
			var getters = base.GetMemberOptions(type, component);

			if (filter.Extension)
			{
				var extensionMethods = type.GetExtensionMethods()
					.Where(ValidateMember)
					.Select(method => GetMemberOption(method, component, method.GetParameters()[0].ParameterType != type));

				getters.AddRange(extensionMethods);
			}

			return getters;
		}

		protected override DropdownOption<UnityGetter> GetMemberOption(MemberInfo member, string component, bool inherited)
		{
			UnityGetter value;
			string label;

			if (member is FieldInfo)
			{
				FieldInfo field = (FieldInfo)member;

				value = new UnityGetter(component, field.Name);
				label = string.Format("{0} {1}", field.FieldType.PrettyName(), field.Name);
			}
			else if (member is PropertyInfo)
			{
				PropertyInfo property = (PropertyInfo)member;

				value = new UnityGetter(component, property.Name);
				label = string.Format("{0} {1}", property.PropertyType.PrettyName(), property.Name);
			}
			else if (member is MethodInfo)
			{
				MethodInfo method = (MethodInfo)member;

				ParameterInfo[] parameters = method.GetParameters();

				value = new UnityGetter(component, member.Name, parameters.Select(p => p.ParameterType).ToArray());

				string parameterString = string.Join(", ", parameters.Select(p => p.ParameterType.PrettyName()).ToArray());

				label = string.Format("{0} {1} ({2})", method.ReturnType.PrettyName(), member.Name, parameterString);
			}
			else
			{
				throw new ArgumentException("Invalid member information type.");
			}

			if (inherited)
			{
				label = "Inherited/" + label;
			}

			return new DropdownOption<UnityGetter>(value, label);
		}

		#endregion

		/// <inheritdoc />
		protected override string memberLabel
		{
			get
			{
				return "Getter";
			}
		}
	}
}