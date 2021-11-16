using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Chronos
{
	[CustomEditor(typeof(Timekeeper))]
	public class TimekeeperEditor : Editor
	{
		protected SerializedProperty debug;
		protected SerializedProperty maxParticleLoops;

		public virtual void OnEnable()
		{
			debug = serializedObject.FindProperty("_debug");
			maxParticleLoops = serializedObject.FindProperty("_maxParticleLoops");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			Timekeeper timekeeper = (Timekeeper)serializedObject.targetObject;

			EditorGUILayout.PropertyField(debug, new GUIContent("Debug Mode"));
			EditorGUILayout.PropertyField(maxParticleLoops, new GUIContent("Max Particle Loops"));

			EditorGUILayout.HelpBox("Add global clocks to this object to configure each clock individually.", MessageType.Info);

			string[] duplicates = timekeeper.GetComponents<GlobalClock>()
				.Select(gc => gc.key)
				.Where(k => !string.IsNullOrEmpty(k))
				.GroupBy(k => k)
				.Where(g => g.Count() > 1)
				.Select(y => y.Key)
				.ToArray();

			if (duplicates.Length > 0)
			{
				EditorGUILayout.HelpBox("The following global clocks have identical keys:\n" + string.Join("\n", duplicates.Select(d => "    - " + d).ToArray()), MessageType.Error);
			}

			serializedObject.ApplyModifiedProperties();
		}

		[MenuItem("GameObject/Timekeeper", false, 12)]
		private static void MenuCommand(MenuCommand menuCommand)
		{
			if (GameObject.FindObjectOfType<Timekeeper>() != null)
			{
				EditorUtility.DisplayDialog("Chronos", "The scene already contains a timekeeper.", "OK");
				return;
			}

			GameObject go = new GameObject("Timekeeper");
			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			Timekeeper timekeeper = go.AddComponent<Timekeeper>();
			timekeeper.AddClock("Root");
			Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
			Selection.activeObject = go;
		}
	}
}
