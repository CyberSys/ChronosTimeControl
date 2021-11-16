using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chronos.Controls.Editor;
using Chronos.Reflection.Internal;
using UnityEditor;

namespace Chronos.Reflection.Editor
{
	[CustomPropertyDrawer(typeof(UnityMethod))]
	public class UnityMethodDrawer : UnityMemberDrawer<UnityMethod>
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
				return MemberTypes.Method;
			}
		}

		/// <inheritdoc />
		protected override bool ValidateMember(MemberInfo member)
		{
			bool valid = base.ValidateMember(member);

			valid &= UnityMemberDrawerHelper.ValidateMethod(filter, (MethodInfo)member);

			return valid;
		}

		#endregion

		#region Fields

		/// <summary>
		/// The UnityMethod.parameterTypes of the inspected property, of type Type[].
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
		protected override void SetValue(UnityMethod value)
		{
			base.SetValue(value);

			UnityMemberDrawerHelper.SerializeParameterTypes(parameterTypesProperty, value != null ? value.parameterTypes : null);
		}

		/// <inheritdoc />
		protected override UnityMethod BuildValue(string component, string name)
		{
			return new UnityMethod(component, name, UnityMemberDrawerHelper.DeserializeParameterTypes(parameterTypesProperty));
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

		protected override List<DropdownOption<UnityMethod>> GetMemberOptions(Type type, string component = null)
		{
			var methods = base.GetMemberOptions(type, component);

			if (filter.Extension)
			{
				var extensionMethods = type.GetExtensionMethods()
					.Where(ValidateMember)
					.Select(method => GetMemberOption(method, component, method.GetParameters()[0].ParameterType != type));

				methods.AddRange(extensionMethods);
			}

			return methods;
		}

		protected override DropdownOption<UnityMethod> GetMemberOption(MemberInfo member, string component, bool inherited)
		{
			UnityMethod value;
			string label;

			if (member is MethodInfo)
			{
				MethodInfo method = (MethodInfo)member;

				ParameterInfo[] parameters = method.GetParameters();

				value = new UnityMethod(component, member.Name, parameters.Select(p => p.ParameterType).ToArray());

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

			return new DropdownOption<UnityMethod>(value, label);
		}

		#endregion

		/// <inheritdoc />
		protected override string memberLabel
		{
			get
			{
				return "Method";
			}
		}
	}
}