using UnityEditor;
using UnityEngine;

namespace Chronos
{
	public abstract class AreaClockEditor<TAreaClock> : ClockEditor where TAreaClock : Component, IAreaClock
	{
		protected SerializedProperty mode;
		protected SerializedProperty curve;
		protected SerializedProperty center;
		protected SerializedProperty padding;
		protected SerializedProperty innerBlend;

		public override void OnEnable()
		{
			base.OnEnable();

			mode = serializedObject.FindProperty("_mode");
			curve = serializedObject.FindProperty("_curve");
			center = serializedObject.FindProperty("_center");
			padding = serializedObject.FindProperty("_padding");
			innerBlend = serializedObject.FindProperty("_innerBlend");
		}

		protected abstract void CheckForCollider();

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (!serializedObject.isEditingMultipleObjects)
			{
				TAreaClock clock = (TAreaClock)serializedObject.targetObject;

				Timekeeper timekeeper = clock.GetComponent<Timekeeper>();

				if (timekeeper != null)
				{
					EditorGUILayout.HelpBox("Only global clocks should be attached to the timekeeper.", MessageType.Error);
				}

				CheckForCollider();
			}

			base.OnInspectorGUI();

			EditorGUILayout.PropertyField(mode, new GUIContent("Mode"));

			if (!mode.hasMultipleDifferentValues &&
				mode.enumValueIndex != (int)AreaClockMode.Instant)
			{
				if (!serializedObject.isEditingMultipleObjects) // TODO: Multiple object editing
				{
					// Bug: Doesn't work in Unity 2017.2. 
					// https://issuetracker.unity3d.com/issues/editorguilayout-dot-curvefield-does-not-return-edited-curve-when-dynamically-creating-curve
					// https://issuetracker.unity3d.com/issues/animationcurve-value-cannot-be-changed-in-custom-inspector-when-it-is-accessed-as-serialized-propertys-animationcurvevalue
					// curve.animationCurveValue = EditorGUILayout.CurveField(new GUIContent("Curve"),
					// 													   curve.animationCurveValue,
					// 													   Color.magenta,
					// 													   new Rect(0, -1, 1, 2),
					// 													   GUILayout.Height(30));
					// Moreover, CurveField doesn't seemt trigger GUI.changed, so we can't rely on that for optimization

					// Fallback to fix the bug:
					Undo.RecordObject(serializedObject.targetObject, "Modify Area Clock Curve");
					TAreaClock clock = (TAreaClock)serializedObject.targetObject;
					clock.curve = EditorGUILayout.CurveField(new GUIContent("Curve"),
																		   clock.curve,
																		   Color.magenta,
																		   new Rect(0, -1, 1, 2),
																		   GUILayout.Height(30));
				}

				if (mode.enumValueIndex == (int)AreaClockMode.PointToEdge)
				{
					EditorGUILayout.PropertyField(center, new GUIContent("Center"));
				}
				else if (mode.enumValueIndex == (int)AreaClockMode.DistanceFromEntry)
				{
					EditorGUILayout.PropertyField(padding, new GUIContent("Padding"));
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		protected override void OnBlendsGUI()
		{
			base.OnBlendsGUI();

			EditorGUILayout.PropertyField(innerBlend, new GUIContent("Inner Blend"));
		}
	}
}
