using System;
using System.Reflection;
using Chronos.Controls.Editor;
using UnityEditor;

namespace Chronos.Reflection.Editor
{
	[CustomPropertyDrawer(typeof(UnityVariable))]
	public class UnityVariableDrawer : UnityMemberDrawer<UnityVariable>
	{
		#region Filtering

		/// <inheritdoc />
		protected override FilterAttribute DefaultFilter()
		{
			FilterAttribute filter = base.DefaultFilter();

			// Override defaults here

			return filter;
		}

		/// <inheritdoc />
		protected override MemberTypes validMemberTypes
		{
			get
			{
				return MemberTypes.Field | MemberTypes.Property;
			}
		}

		/// <inheritdoc />
		protected override bool ValidateMember(MemberInfo member)
		{
			bool valid = base.ValidateMember(member);

			FieldInfo field = member as FieldInfo;
			PropertyInfo property = member as PropertyInfo;

			if (field != null) // Member is a field
			{
				valid &= UnityMemberDrawerHelper.ValidateField(filter, field);
			}
			else if (property != null) // Member is a property
			{
				valid &= UnityMemberDrawerHelper.ValidateProperty(filter, property);
			}

			return valid;
		}

		// Do not edit below

		#endregion

		#region Value

		/// <inheritdoc />
		protected override UnityVariable BuildValue(string component, string name)
		{
			return new UnityVariable(component, name);
		}

		#endregion

		#region Reflection

		protected override DropdownOption<UnityVariable> GetMemberOption(MemberInfo member, string component, bool inherited)
		{
			UnityVariable value;
			string label;

			if (member is FieldInfo)
			{
				FieldInfo field = (FieldInfo)member;

				value = new UnityVariable(component, field.Name);
				label = string.Format("{0} {1}", field.FieldType.PrettyName(), field.Name);
			}
			else if (member is PropertyInfo)
			{
				PropertyInfo property = (PropertyInfo)member;

				value = new UnityVariable(component, property.Name);
				label = string.Format("{0} {1}", property.PropertyType.PrettyName(), property.Name);
			}
			else
			{
				throw new ArgumentException("Invalid member information type.");
			}

			if (inherited)
			{
				label = "Inherited/" + label;
			}

			return new DropdownOption<UnityVariable>(value, label);
		}

		#endregion

		/// <inheritdoc />
		protected override string memberLabel
		{
			get
			{
				return "Variable";
			}
		}
	}
}