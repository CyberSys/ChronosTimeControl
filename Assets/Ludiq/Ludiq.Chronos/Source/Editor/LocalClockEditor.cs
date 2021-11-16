using UnityEditor;
using UnityEngine;

namespace Chronos
{
	[CustomEditor(typeof(LocalClock)), CanEditMultipleObjects]
	public class LocalClockEditor : ClockEditor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (!serializedObject.isEditingMultipleObjects)
			{
				LocalClock clock = (LocalClock)serializedObject.targetObject;
				Timekeeper timekeeper = clock.GetComponent<Timekeeper>();

				if (timekeeper != null)
				{
					EditorGUILayout.HelpBox("Only global clocks should be attached to the timekeeper.", MessageType.Error);
				}
			}

			base.OnInspectorGUI();

			serializedObject.ApplyModifiedProperties();
		}
	}
}
