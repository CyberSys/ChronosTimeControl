using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Chronos
{
	[CustomEditor(typeof(AreaClock2D)), CanEditMultipleObjects]
	public class AreaClock2DEditor : AreaClockEditor<AreaClock2D>
	{
		protected override void CheckForCollider()
		{
			AreaClock2D clock = (AreaClock2D)serializedObject.targetObject;

			Collider2D collider = clock.GetComponent<Collider2D>();

			if (collider == null || !collider.enabled)
			{
				EditorGUILayout.HelpBox("An area clock requires a collider component.", MessageType.Error);
			}
			else if (!collider.isTrigger)
			{
				EditorGUILayout.HelpBox("The collider attached to the area clock should be a trigger.", MessageType.Warning);
			}
		}

		private static bool IsVectorNegative(Vector3 v)
		{
			return v.x < 0 || v.y < 0 || v.z < 0;
		}

		[DrawGizmo(GizmoType.Active)]
		private static void DrawGizmos(AreaClock2D clock, GizmoType gizmoType)
		{
			// Draw icon...

			Gizmos.color = Color.white;

			Vector3 position;

			if (clock.mode == AreaClockMode.PointToEdge)
			{
				position = clock.transform.TransformPoint(clock.center);
			}
			else
			{
				position = clock.transform.position;
			}

			Gizmos.DrawIcon(position, "Chronos/AreaClock");

			// Draw inset approximations for padding...

			if (clock.mode == AreaClockMode.DistanceFromEntry)
			{
				Handles.color = clock.padding > 0 ? Color.cyan : Color.red;

				Vector2 inset = -(2 * clock.padding * Vector2.one);

				CircleCollider2D circleCollider = clock.GetComponent<CircleCollider2D>();

				if (circleCollider != null)
				{
					Vector2 scale = clock.transform.lossyScale;
					float maxScale = Mathf.Max(scale.x, scale.y);
					Vector2 insetScale = (maxScale * (2 * circleCollider.radius) * Vector2.one) + inset;

					if (IsVectorNegative(insetScale)) Handles.color = Color.red;
					Handles.matrix = Matrix4x4.TRS(clock.transform.position, clock.transform.rotation, insetScale);
					Handles.DrawWireDisc(Vector3.zero, Vector3.forward, 0.5f);

					return;
				}

				BoxCollider2D boxCollider = clock.GetComponent<BoxCollider2D>();

				if (boxCollider != null)
				{
					Vector2 insetScale = Vector2.Scale(boxCollider.size, clock.transform.lossyScale) + inset;

					if (IsVectorNegative(insetScale)) Handles.color = Color.red;
					Handles.matrix = Matrix4x4.TRS(clock.transform.position, clock.transform.rotation, insetScale);
					Handles.DrawPolyLine
					(
						new Vector3(-1, -1, 0) / 2,
						new Vector3(-1, +1, 0) / 2,
						new Vector3(+1, +1, 0) / 2,
						new Vector3(+1, -1, 0) / 2,
						new Vector3(-1, -1, 0) / 2
					);

					return;
				}

				PolygonCollider2D polyCollider = clock.GetComponent<PolygonCollider2D>();

				if (polyCollider != null)
				{
					Vector3 bounds = polyCollider.bounds.size;
					float smallestBound = Mathf.Min(bounds.x, bounds.y);
					Vector2 normalizedBounds = smallestBound * new Vector2(1 / bounds.x, 1 / bounds.y);
					Vector2 insetScale = (Vector2)clock.transform.lossyScale + Vector2.Scale(normalizedBounds, inset);

					if (IsVectorNegative(insetScale)) Handles.color = Color.red;
					Handles.matrix = Matrix4x4.TRS(clock.transform.position, clock.transform.rotation, insetScale);
					Vector3[] points = polyCollider.points.Select(v2 => (Vector3)v2).ToArray();
					Handles.DrawPolyLine(points);

					return;
				}
			}
		}
	}
}
