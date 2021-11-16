namespace Chronos.Controls.Editor
{
	/// <summary>
	/// An option in an editor popup field.
	/// </summary>
	/// <typeparam name="T">The type of the backing value.</typeparam>
	public class DropdownOption<T>
	{
		/// <summary>
		/// The backing value of the option.
		/// </summary>
		public T value;

		/// <summary>
		/// The visible label of the option.
		/// </summary>
		public string label;

		/// <summary>
		/// Initializes a new instance of the PopupOption class with the specified value.
		/// </summary>
		public DropdownOption(T value)
		{
			this.value = value;
			this.label = value.ToString();
		}

		/// <summary>
		/// Initializes a new instance of the PopupOption class with the specified value and label.
		/// </summary>
		public DropdownOption(T value, string label)
		{
			this.value = value;
			this.label = label;
		}

		public static implicit operator T(DropdownOption<T> option)
		{
			return option.value;
		}
	}
}
