using System.Collections.Generic;
using System.Linq;
using Chronos.Controls.Editor;
using Chronos.Reflection.Internal;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Chronos.Reflection.Editor
{
	[CustomPropertyDrawer(typeof(AnimatorParameter))]
	public class AnimatorParameterDrawer : TargetedDrawer
	{
		#region Fields

		/// <summary>
		/// The inspected property, of type AnimatorParameter.
		/// </summary>
		protected SerializedProperty property;

		/// <summary>
		/// The UnityMember.name of the inspected property, of type string.
		/// </summary>
		protected SerializedProperty nameProperty;

		/// <summary>
		/// The targeted animators.
		/// </summary>
		protected Animator[] targets;

		#endregion

		/// <inheritdoc />
		protected override void Update(SerializedProperty property)
		{
			// Update the targeted drawer
			base.Update(property);

			// Assign the property and sub-properties
			this.property = property;
			nameProperty = property.FindPropertyRelative("_name");

			// Find the targets
			targets = FindTargets();
		}

		/// <inheritdoc />
		protected override void RenderMemberControl(Rect position)
		{
			var options = GetNameOptions();

			DropdownOption<AnimatorParameter> selectedOption = null;
			DropdownOption<AnimatorParameter> noneOption = new DropdownOption<AnimatorParameter>(null, "No Parameter");

			AnimatorParameter value = GetValue();

			if (value != null)
			{
				string label = value.name;

				AnimatorParameter valueInOptions = options.Select(option => option.value).FirstOrDefault(ap => ap.Corresponds(value));

				if (valueInOptions != null)
				{
					selectedOption = new DropdownOption<AnimatorParameter>(valueInOptions, label);
				}
				else
				{
					selectedOption = new DropdownOption<AnimatorParameter>(value, label);
				}

			}

			// Make sure the callback uses the property of this drawer, not at its later value.
			var propertyNow = property;

			bool enabled = targets.Any(target => target != null);

			if (!enabled) EditorGUI.BeginDisabledGroup(true);

			DropdownGUI<AnimatorParameter>.PopupSingle
			(
				position,
				newValue =>
				{
					Update(propertyNow);
					SetValue(newValue);
					propertyNow.serializedObject.ApplyModifiedProperties();
				},
				options,
				selectedOption,
				noneOption,
				nameProperty.hasMultipleDifferentValues
			);

			if (!enabled) EditorGUI.EndDisabledGroup();
		}

		#region Value

		/// <summary>
		/// Returns an animator parameter constructed from the current property values.
		/// </summary>
		protected AnimatorParameter GetValue()
		{
			if (nameProperty.hasMultipleDifferentValues || string.IsNullOrEmpty(nameProperty.stringValue))
			{
				return null;
			}

			string name = nameProperty.stringValue;
			if (name == string.Empty) name = null;
			return new AnimatorParameter(name);
		}

		/// <summary>
		/// Assigns the property values from a specified animator parameter.
		/// </summary>
		protected void SetValue(AnimatorParameter value)
		{
			if (value != null)
			{
				nameProperty.stringValue = value.name;
			}
			else
			{
				nameProperty.stringValue = null;
			}
		}

		#endregion

		#region Targetting

		/// <inheritdoc />
		protected override Object GetSelfTarget(Object obj)
		{
			if (obj is GameObject)
			{
				return ((GameObject)obj).GetComponent<Animator>();
			}
			else if (obj is Component)
			{
				return ((Component)obj).GetComponent<Animator>();
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the list of targets on the inspected objects.
		/// </summary>
		protected Animator[] FindTargets()
		{
			IEnumerable<Object> objects = targetProperty.Multiple().Select(p => p.objectReferenceValue);

			var childrenAnimators = objects.OfType<GameObject>().SelectMany(gameObject => gameObject.GetComponents<Animator>());
			var siblingAnimators = objects.OfType<Component>().SelectMany(component => component.GetComponents<Animator>());

			return childrenAnimators.Concat(siblingAnimators).ToArray();
		}

		#endregion

		#region Reflection

		/// <summary>
		/// Gets the list of shared parameter names as popup options.
		/// </summary>
		protected List<DropdownOption<AnimatorParameter>> GetNameOptions()
		{
			var options = new List<DropdownOption<AnimatorParameter>>();

			List<string> names = targets
				.Select(animator => ((AnimatorController)animator.runtimeAnimatorController))
				.Where(animatorController => animatorController != null)
				.Select(animatorController => animatorController.parameters)
				.Select(parameters => parameters.Select(parameter => parameter.name))
				.IntersectAll()
				.Distinct()
				.ToList();

			foreach (string name in names)
			{
				options.Add(new DropdownOption<AnimatorParameter>(new AnimatorParameter(name), name));
			}

			return options;
		}

		#endregion
	}
}