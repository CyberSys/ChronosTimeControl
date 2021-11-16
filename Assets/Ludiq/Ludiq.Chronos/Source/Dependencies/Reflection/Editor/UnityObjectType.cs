namespace Chronos.Reflection.Editor
{
	/// <summary>
	/// A simplified representation of the type of a UnityEngine.Object.
	/// </summary>
	public enum UnityObjectType
	{
		/// <summary>
		/// No Object or null Object
		/// </summary>
		None,

		/// <summary>
		/// Multiple different types of objects
		/// </summary>
		Mixed,

		/// <summary>
		/// Object is a GameObject
		/// </summary>
		GameObject,

		/// <summary>
		/// Object is a ScriptableObject
		/// </summary>
		ScriptableObject,

		/// <summary>
		/// Object is not a GameObject or ScriptableObject
		/// </summary>
		Other
	}
}