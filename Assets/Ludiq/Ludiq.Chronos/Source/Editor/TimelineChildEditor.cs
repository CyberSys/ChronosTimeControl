using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Chronos
{
	[CustomEditor(typeof(TimelineChild)), CanEditMultipleObjects]
	public class TimelineChildEditor : Editor
	{
		protected SerializedProperty recordingInterval;

		public void OnEnable()
		{
			recordingInterval = serializedObject.FindProperty("_recordingInterval");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			bool anyRewindable = serializedObject.targetObjects.OfType<TimelineChild>().Any(tc =>
			{
				var t = tc.GetComponentInParent<Timeline>();
				return t != null && t.rewindable;
            });

			EditorGUI.BeginDisabledGroup(Application.isPlaying);
			{
				if (anyRewindable)
				{
					EditorGUILayout.PropertyField(recordingInterval, new GUIContent("Recording Interval"));
				}
			}
			EditorGUI.EndDisabledGroup();

			if (anyRewindable)
			{
				float estimate = serializedObject.targetObjects.OfType<TimelineChild>().Select(tc => tc.EstimateMemoryUsage()).Sum() / 1024;
				
				EditorGUILayout.HelpBox(string.Format("Estimated memory: {0} KiB.", estimate), MessageType.Info);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
