using UnityEditor;
using UnityEngine;

namespace Chronos
{
	[CustomEditor(typeof(GlobalClock)), CanEditMultipleObjects]
	public class GlobalClockEditor : ClockEditor
	{
		protected SerializedProperty key;

		public override void OnEnable()
		{
			base.OnEnable();

			key = serializedObject.FindProperty("_key");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (!serializedObject.isEditingMultipleObjects)
			{
				Timekeeper timekeeper = ((Clock)serializedObject.targetObject).GetComponent<Timekeeper>();

				if (timekeeper == null || !timekeeper.enabled)
				{
					EditorGUILayout.HelpBox("Global clocks only function when attached to the timekeeper.", MessageType.Error);
				}
			}

			EditorGUILayout.PropertyField(key, new GUIContent("Key"));

			base.OnInspectorGUI();

			if (!key.hasMultipleDifferentValues &&
				string.IsNullOrEmpty(key.stringValue))
			{
				EditorGUILayout.HelpBox("The key cannot be empty.", MessageType.Error);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
