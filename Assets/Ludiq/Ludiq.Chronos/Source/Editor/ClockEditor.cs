using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Chronos
{
	[CustomEditor(typeof(Clock)), CanEditMultipleObjects]
	public class ClockEditor : Editor
	{
		protected SerializedProperty parentKey;
		protected SerializedProperty localTimeScale;
		protected SerializedProperty paused;
		protected SerializedProperty parentBlend;

		public virtual void OnEnable()
		{
			parentKey = serializedObject.FindProperty("_parentKey");
			localTimeScale = serializedObject.FindProperty("_localTimeScale");
			paused = serializedObject.FindProperty("_paused");
			parentBlend = serializedObject.FindProperty("_parentBlend");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginDisabledGroup(Application.isPlaying);
			{
				EditorGUILayout.PropertyField(parentKey, new GUIContent("Parent"));
			}
			EditorGUI.EndDisabledGroup();

			OnBlendsGUI();

			EditorGUILayout.PropertyField(localTimeScale, new GUIContent("Time Scale"));

			EditorGUILayout.PropertyField(paused, new GUIContent("Paused"));

			if (!serializedObject.isEditingMultipleObjects &&
				Application.isPlaying)
			{
				Clock clock = (Clock)serializedObject.targetObject;

				EditorGUILayout.LabelField("Computed Time Scale", clock.timeScale.ToString("0.00"));
			}
		}

		protected virtual void OnBlendsGUI()
		{
			if (parentKey.hasMultipleDifferentValues || !string.IsNullOrEmpty(parentKey.stringValue))
			{
				EditorGUILayout.PropertyField(parentBlend, new GUIContent("Parent Blend"));
			}
		}
	}
}
