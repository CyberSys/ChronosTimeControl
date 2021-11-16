using Chronos.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace Chronos
{
	[CustomEditor(typeof(CustomRecorder)), CanEditMultipleObjects]
	public class CustomRecorderEditor : Editor
	{
		private SerializedProperty variables;

		public virtual void OnEnable()
		{
			variables = serializedObject.FindProperty("variables");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			EditorGUI.BeginDisabledGroup(Application.isPlaying);
			{
				ReorderableListGUI.Title("Variables");
				ReorderableListGUI.ListField(variables, ReorderableListFlags.DisableDuplicateCommand);
			}
			EditorGUI.EndDisabledGroup();

			serializedObject.ApplyModifiedProperties();
		}
	}
}
