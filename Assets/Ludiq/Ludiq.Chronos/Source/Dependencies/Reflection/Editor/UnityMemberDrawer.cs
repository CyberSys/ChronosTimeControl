using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chronos.Controls.Editor;
using Chronos.Reflection.Internal;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Chronos.Reflection.Editor
{
	[CustomPropertyDrawer(typeof(UnityMember))]
	public abstract class UnityMemberDrawer<TMember> : TargetedDrawer where TMember : UnityMember
	{
		public FilterAttribute filterOverride { get; set; }



		#region Fields

		/// <summary>
		/// The filter attribute on the inspected field.
		/// </summary>
		protected FilterAttribute filter;

		/// <summary>
		/// The inspected property, of type UnityMember.
		/// </summary>
		protected SerializedProperty property;

		/// <summary>
		/// The UnityMember.component of the inspected property, of type string.
		/// </summary>
		protected SerializedProperty componentProperty;

		/// <summary>
		/// The UnityMember.name of the inspected property, of type string.
		/// </summary>
		protected SerializedProperty nameProperty;

		/// <summary>
		/// The targeted Unity Objects.
		/// </summary>
		protected UnityObject[] targets;

		/// <summary>
		/// The type of targeted objects.
		/// </summary>
		protected UnityObjectType targetType;

		#endregion

		/// <inheritdoc />
		protected override void Update(SerializedProperty property)
		{
			// Update the targeted drawer
			base.Update(property);

			// Assign the property and sub-properties
			this.property = property;
			componentProperty = property.FindPropertyRelative("_component");
			nameProperty = property.FindPropertyRelative("_name");

			// Fetch the filter
			filter = filterOverride ?? (FilterAttribute)fieldInfo.GetCustomAttributes(typeof(FilterAttribute), true).FirstOrDefault() ?? DefaultFilter();

			// Find the targets
			targets = FindTargets();
			targetType = DetermineTargetType();
		}

		/// <inheritdoc />
		protected override void RenderMemberControl(Rect position)
		{
			// Other Targets
			// Some Unity Objects, like Assets, are not supported by the drawer. 
			// Just display an error message to let the user change their target.

			if (targetType == UnityObjectType.Other)
			{
				EditorGUI.HelpBox(position, "Unsupported Unity Object type.", MessageType.None);
				return;
			}

			// Display a list of all available reflected members in a popup.

			var options = new List<DropdownOption<TMember>>();

			TMember value = GetValue();

			DropdownOption<TMember> selectedOption = null;
			DropdownOption<TMember> noneOption = new DropdownOption<TMember>(null, string.Format("No {0}", memberLabel));

			if (targetType == UnityObjectType.GameObject)
			{
				// Check if all targets have a GameObject (none are empty).
				// If they do, display all members of the GameObject type.

				if (HasSharedGameObject())
				{
					var gameObjectOptions = GetSortedMemberOptions(typeof(GameObject));

					foreach (var gameObjectOption in gameObjectOptions)
					{
						// Prefix label by GameObject for popup clarity.

						gameObjectOption.label = string.Format("GameObject/{0}", gameObjectOption.label);

						options.Add(gameObjectOption);
					}
				}

				// Find all shared component types across targets.
				// Display all members of each one found.

				foreach (Type componentType in GetSharedComponentTypes())
				{
					var componentOptions = GetSortedMemberOptions(componentType, componentType.Name);

					foreach (var componentOption in componentOptions)
					{
						// Prefix label and option by component type for clear distinction.

						componentOption.label = string.Format("{0}/{1}", componentType.Name, componentOption.label);

						options.Add(componentOption);
					}
				}

				// Determine which option is currently selected.

				if (value != null)
				{
					string label;

					if (value.component == null)
					{
						label = string.Format("GameObject.{0}", value.name);
					}
					else
					{
						label = string.Format("{0}.{1}", value.component, value.name);
					}

					UnityMethod method = value as UnityMethod;

					if (method != null)
					{
						string parameterString = string.Join(", ", method.parameterTypes.Select(t => t.PrettyName()).ToArray());

						label += string.Format(" ({0})", parameterString);
					}

					TMember valueInOptions = options.Select(option => option.value).FirstOrDefault(member => member.Corresponds(value));

					if (valueInOptions != null)
					{
						selectedOption = new DropdownOption<TMember>(valueInOptions, label);
					}
					else
					{
						selectedOption = new DropdownOption<TMember>(value, label);
					}
				}
			}
			else if (targetType == UnityObjectType.ScriptableObject)
			{
				// ScriptableObject Target
				// Make sure all targets share the same ScriptableObject Type.
				// If they do, display all members of that type.

				Type scriptableObjectType = GetSharedScriptableObjectType();

				if (scriptableObjectType != null)
				{
					options.AddRange(GetSortedMemberOptions(scriptableObjectType));

					// Determine which option is currently selected.

					if (value != null)
					{
						selectedOption = options.Find(o => o.value.Corresponds(value));

						if (selectedOption == null)
						{
							selectedOption = new DropdownOption<TMember>(value, value.name);
						}
					}
				}
			}

			// Make sure the callback uses the property of this drawer, not at its later value.
			var propertyNow = property;

			bool enabled = targetType != UnityObjectType.None;

			if (!enabled) EditorGUI.BeginDisabledGroup(true);

			DropdownGUI<TMember>.PopupSingle
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
				hasMultipleDifferentValues
			);

			if (!enabled) EditorGUI.EndDisabledGroup();
		}

		#region Value

		/// <summary>
		/// Constructs a new instance of the member from the specified component and name.
		/// </summary>
		protected abstract TMember BuildValue(string component, string name);

		/// <summary>
		/// Returns a member constructed from the current parameter values.
		/// </summary>
		/// <returns></returns>
		protected TMember GetValue()
		{
			if (hasMultipleDifferentValues ||
				string.IsNullOrEmpty(nameProperty.stringValue))
			{
				return null;
			}

			string component = componentProperty.stringValue;
			string name = nameProperty.stringValue;

			if (component == string.Empty) component = null;
			if (name == string.Empty) name = null;

			return BuildValue(component, name);
		}

		/// <summary>
		/// Assigns the property values from a specified member.
		/// </summary>
		protected virtual void SetValue(TMember value)
		{
			if (value != null)
			{
				componentProperty.stringValue = value.component;
				nameProperty.stringValue = value.name;
			}
			else
			{
				componentProperty.stringValue = null;
				nameProperty.stringValue = null;
			}
		}

		/// <summary>
		/// Indicated whether the property has multiple different values.
		/// </summary>
		protected virtual bool hasMultipleDifferentValues
		{
			get
			{
				return componentProperty.hasMultipleDifferentValues || nameProperty.hasMultipleDifferentValues;
			}
		}

		#endregion

		#region Targeting

		/// <summary>
		/// Get the list of targets on the inspected objects.
		/// </summary>
		protected UnityObject[] FindTargets()
		{
			if (isSelfTargeted)
			{
				// In self targeting mode, the targets are the inspected objects themselves.

				return property.serializedObject.targetObjects;
			}
			else
			{
				// In manual targeting mode, the targets the values of each target property.

				return targetProperty.Multiple().Select(p => p.objectReferenceValue).ToArray();
			}
		}

		/// <summary>
		/// Determine the Unity type of the targets.
		/// </summary>
		protected UnityObjectType DetermineTargetType()
		{
			UnityObjectType unityObjectType = UnityObjectType.None;

			foreach (UnityObject targetObject in targets)
			{
				// Null (non-specified) targets don't affect the type
				// If no non-null target is specified, the type will be None
				// as the loop will simply exit.

				if (targetObject == null)
				{
					continue;
				}

				if (targetObject is GameObject || targetObject is Component)
				{
					// For GameObjects and Components, the target is either the
					// GameObject itself, or the one to which the Component belongs.

					// If a ScriptableObject target was previously found,
					// return that the targets are of mixed types.

					if (unityObjectType == UnityObjectType.ScriptableObject)
					{
						return UnityObjectType.Mixed;
					}

					unityObjectType = UnityObjectType.GameObject;
				}
				else if (targetObject is ScriptableObject)
				{
					// For ScriptableObjects, the target is simply the
					// ScriptableObject itself.

					// If a GameObject target was previously found,
					// return that the targets are of mixed types.

					if (unityObjectType == UnityObjectType.GameObject)
					{
						return UnityObjectType.Mixed;
					}

					unityObjectType = UnityObjectType.ScriptableObject;
				}
				else
				{
					// Other target types

					return UnityObjectType.Other;
				}
			}

			return unityObjectType;
		}

		/// <summary>
		/// Determines if the targets all share a GameObject.
		/// </summary>
		public bool HasSharedGameObject()
		{
			return !targets.Contains(null);
		}

		/// <summary>
		/// Determines which types of Components are shared on all GameObject targets.
		/// </summary>
		protected IEnumerable<Type> GetSharedComponentTypes()
		{
			if (targets.Contains(null))
			{
				return Enumerable.Empty<Type>();
			}

			var childrenComponents = targets.OfType<GameObject>().Select(gameObject => gameObject.GetComponents<Component>().Where(c => c != null));
			var siblingComponents = targets.OfType<Component>().Select(component => component.GetComponents<Component>().Where(c => c != null));

			return childrenComponents.Concat(siblingComponents)
				.Select(components => components.Select(component => component.GetType()))
				.IntersectAll()
				.Distinct();
		}

		/// <summary>
		/// Determines which type of ScriptableObject is shared across targets.
		/// Returns null if none are shared.
		/// </summary>
		protected Type GetSharedScriptableObjectType()
		{
			if (targets.Contains(null))
			{
				return null;
			}

			return targets
				.OfType<ScriptableObject>()
				.Select(scriptableObject => scriptableObject.GetType())
				.Distinct()
				.SingleOrDefault(); // Null (default) if multiple or zero
		}

		#endregion

		#region Reflection

		/// <summary>
		/// Gets the list of members available on a type as popup options.
		/// </summary>
		protected virtual List<DropdownOption<TMember>> GetMemberOptions(Type type, string component = null)
		{
			return type
				.GetMembers(validBindingFlags)
				.Where(member => validMemberTypes.HasFlag(member.MemberType))
				.Where(ValidateMember)
				.Select(member => GetMemberOption(member, component, member.DeclaringType != type))
				.ToList();
		}

		/// <summary>
		/// Gets the sorted list of members available on a type as popup options.
		/// </summary>
		protected virtual List<DropdownOption<TMember>> GetSortedMemberOptions(Type type, string component = null)
		{
			var options = GetMemberOptions(type, component);

			var withoutSlashes = options.Where(o => !o.label.Contains("/")).ToList();
			var withSlashes = options.Where(o => o.label.Contains("/")).ToList();

			options.Clear();
			options.AddRange(withSlashes);
			options.AddRange(withoutSlashes);

			return options;
		}

		protected abstract DropdownOption<TMember> GetMemberOption(MemberInfo member, string component, bool inherited);

		#endregion

		#region Filtering

		/// <summary>
		/// The label of a member, displayed in options.
		/// </summary>
		protected virtual string memberLabel
		{
			get
			{
				return "Member";
			}
		}

		/// <summary>
		/// The default applied filter attribute if none is specified.
		/// </summary>
		protected virtual FilterAttribute DefaultFilter()
		{
			return new FilterAttribute();
		}

		/// <summary>
		/// The valid BindingFlags when looking for reflected members.
		/// </summary>
		protected virtual BindingFlags validBindingFlags
		{
			get
			{
				// Build the flags from the filter attribute

				BindingFlags flags = (BindingFlags)0;

				if (filter.Public) flags |= BindingFlags.Public;
				if (filter.NonPublic) flags |= BindingFlags.NonPublic;
				if (filter.Instance) flags |= BindingFlags.Instance;
				if (filter.Static) flags |= BindingFlags.Static;
				if (!filter.Inherited) flags |= BindingFlags.DeclaredOnly;
				if (filter.Static && filter.Inherited) flags |= BindingFlags.FlattenHierarchy;

				return flags;
			}
		}

		/// <summary>
		/// The valid MemberTypes when looking for reflected members.
		/// </summary>
		protected virtual MemberTypes validMemberTypes
		{
			get
			{
				return MemberTypes.All;
			}
		}

		/// <summary>
		/// Determines whether a given MemberInfo should be included in the options.
		/// This check follows the BindingFlags and MemberTypes filtering. 
		/// </summary>
		protected virtual bool ValidateMember(MemberInfo member)
		{
			return true;
		}

		/// <summary>
		/// Determines whether a MemberInfo of the given type should be included in the options.
		/// </summary>
		protected virtual bool ValidateMemberType(Type type)
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

		#endregion
	}
}