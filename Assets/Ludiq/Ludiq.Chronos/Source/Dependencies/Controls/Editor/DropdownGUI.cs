using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Chronos.Controls.Editor
{
	/// <summary>
	/// Utility class to display complex editor popups.
	/// </summary>
	public static class DropdownGUI<T>
	{
		public delegate void SingleCallback(T value);
		public delegate void MultipleCallback(IEnumerable<T> value);

		/// <summary>
		/// Render an editor popup and return the newly selected option.
		/// </summary>
		/// <param name="position">The position of the control.</param>
		/// <param name="callback">The function called when a value is selected.</param>
		/// <param name="options">The list of available options.</param>
		/// <param name="selectedOption">The selected option, or null for none.</param>
		/// <param name="noneOption">The option for "no selection", or null for none.</param>
		/// <param name="hasMultipleDifferentValues">Whether the content has multiple different values.</param>
		/// <param name="allowOuterOption">Whether a selected option not in range should be allowed.</param>
		public static void PopupSingle
		(
			Rect position,
			SingleCallback callback,
			IEnumerable<DropdownOption<T>> options,
			DropdownOption<T> selectedOption,
			DropdownOption<T> noneOption,
			bool hasMultipleDifferentValues,
			bool allowOuterOption = true
		)
		{
			string label;

			if (hasMultipleDifferentValues)
			{
				label = "\u2014"; // Em Dash
			}
			else if (selectedOption == null)
			{
				if (noneOption != null)
				{
					label = noneOption.label;
				}
				else
				{
					label = string.Empty;
				}
			}
			else
			{
				label = selectedOption.label;
			}

			if (GUI.Button(position, label, EditorStyles.popup))
			{
				DropdownSingle
				(
					new Vector2(position.xMin, position.yMax),
					callback,
					options,
					selectedOption,
					noneOption,
					hasMultipleDifferentValues
				);
			}
			else if (selectedOption != null && !options.Select(o => o.value).Contains(selectedOption.value) && !allowOuterOption)
			{
				// Selected option isn't in range

				if (hasMultipleDifferentValues)
				{
					// Do nothing
				}
				else if (noneOption != null)
				{
					callback(noneOption.value);
				}
				else
				{
					callback(default(T));
				}
			}
		}

		public static void DropdownSingle
		(
			Vector2 position,
			SingleCallback callback,
			IEnumerable<DropdownOption<T>> options,
			DropdownOption<T> selectedOption,
			DropdownOption<T> noneOption,
			bool hasMultipleDifferentValues
		)
		{
			bool hasOptions = options != null && options.Any();

			GenericMenu menu = new GenericMenu();
			GenericMenu.MenuFunction2 menuCallback = (o) => { GUI.changed = true; callback((T)o); };

			if (noneOption != null)
			{
				bool on = !hasMultipleDifferentValues && (selectedOption == null || EqualityComparer<T>.Default.Equals(selectedOption.value, noneOption.value));

				menu.AddItem(new GUIContent(noneOption.label), on, menuCallback, noneOption.value);
			}

			if (noneOption != null && hasOptions)
			{
				menu.AddSeparator("");
			}

			if (hasOptions)
			{
				foreach (var option in options)
				{
					bool on = !hasMultipleDifferentValues && (selectedOption != null && EqualityComparer<T>.Default.Equals(selectedOption.value, option.value));

					menu.AddItem(new GUIContent(option.label), on, menuCallback, option.value);
				}
			}

			menu.DropDown(new Rect(position, Vector2.zero));
		}

		private static IEnumerable<T> SanitizeMultipleOptions(IEnumerable<DropdownOption<T>> options, IEnumerable<T> selectedOptions)
		{
			// Remove outer options
			return selectedOptions.Where(so => options.Any(o => EqualityComparer<T>.Default.Equals(o.value, so)));
		}

		public static void PopupMultiple
		(
			Rect position,
			MultipleCallback callback,
			IEnumerable<DropdownOption<T>> options,
			IEnumerable<T> selectedOptions,
			bool hasMultipleDifferentValues
		)
		{
			string label;

			selectedOptions = SanitizeMultipleOptions(options, selectedOptions);

			if (hasMultipleDifferentValues)
			{
				label = "\u2014"; // Em Dash
			}
			else
			{
				var selectedOptionsCount = selectedOptions.Count();
				var optionsCount = options.Count();

				if (selectedOptionsCount == 0)
				{
					label = "Nothing";
				}
				else if (selectedOptionsCount == 1)
				{
					label = options.First(o => EqualityComparer<T>.Default.Equals(o.value, selectedOptions.First())).label;
				}
				else if (selectedOptionsCount == optionsCount)
				{
					label = "Everything";
				}
				else
				{
					label = "(Mixed)";
				}
			}

			if (GUI.Button(position, label, EditorStyles.popup))
			{
				DropdownMultiple
				(
					new Vector2(position.xMin, position.yMax),
					callback,
					options,
					selectedOptions,
					hasMultipleDifferentValues
				);
			}
		}

		public static void DropdownMultiple
		(
			Vector2 position,
			MultipleCallback callback,
			IEnumerable<DropdownOption<T>> options,
			IEnumerable<T> selectedOptions,
			bool hasMultipleDifferentValues
		)
		{
			selectedOptions = SanitizeMultipleOptions(options, selectedOptions);

			bool hasOptions = options != null && options.Any();

			GenericMenu menu = new GenericMenu();
			GenericMenu.MenuFunction2 switchCallback = (o) =>
			{
				GUI.changed = true;

				var switchOption = (T)o;

				var newSelectedOptions = selectedOptions.ToList();

				if (newSelectedOptions.Contains(switchOption))
				{
					newSelectedOptions.Remove(switchOption);
				}
				else
				{
					newSelectedOptions.Add(switchOption);
				}

				callback(newSelectedOptions);
			};

			GenericMenu.MenuFunction nothingCallback = () =>
			{
				GUI.changed = true;
				callback(Enumerable.Empty<T>());
			};

			GenericMenu.MenuFunction everythingCallback = () =>
			{
				GUI.changed = true;
				callback(options.Select((o) => o.value));
			};

			menu.AddItem(new GUIContent("Nothing"), !hasMultipleDifferentValues && !selectedOptions.Any(), nothingCallback);
			menu.AddItem(new GUIContent("Everything"), !hasMultipleDifferentValues && selectedOptions.Count() == options.Count() && Enumerable.SequenceEqual(selectedOptions.OrderBy(t => t), options.Select(o => o.value).OrderBy(t => t)), everythingCallback);

			if (hasOptions)
			{
				menu.AddSeparator(""); // Not in Unity default, but pretty

				foreach (var option in options)
				{
					bool on = !hasMultipleDifferentValues && (selectedOptions.Any(selectedOption => EqualityComparer<T>.Default.Equals(selectedOption, option.value)));

					menu.AddItem(new GUIContent(option.label), on, switchCallback, option.value);
				}
			}

			menu.DropDown(new Rect(position, Vector2.zero));
		}
	}
}
