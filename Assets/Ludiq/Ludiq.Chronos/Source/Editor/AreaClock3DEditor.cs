using UnityEditor;
using UnityEngine;

namespace Chronos
{
	[CustomEditor(typeof(AreaClock3D)), CanEditMultipleObjects]
	public class AreaClock3DEditor : AreaClockEditor<AreaClock3D>
	{
		protected override void CheckForCollider()
		{
			AreaClock3D clock = (AreaClock3D)serializedObject.targetObject;

			Collider collider = clock.GetComponent<Collider>();

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
		private static void DrawGizmos(AreaClock3D clock, GizmoType gizmoType)
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
				Gizmos.color = clock.padding > 0 ? Color.cyan : Color.red;

				Vector3 inset = -(2 * clock.padding * Vector3.one);

				SphereCollider sphereCollider = clock.GetComponent<SphereCollider>();

				if (sphereCollider != null)
				{
					Vector3 scale = clock.transform.lossyScale;
					float maxScale = Mathf.Max(scale.x, scale.y, scale.z);
					Vector3 insetScale = (maxScale * (2 * sphereCollider.radius) * Vector3.one) + inset;

					if (IsVectorNegative(insetScale)) Gizmos.color = Color.red;
					Gizmos.matrix = Matrix4x4.TRS(clock.transform.position, clock.transform.rotation, insetScale);
					Gizmos.DrawWireSphere(Vector3.zero, 0.5f);

					return;
				}

				BoxCollider boxCollider = clock.GetComponent<BoxCollider>();

				if (boxCollider != null)
				{
					Vector3 insetScale = Vector3.Scale(boxCollider.size, clock.transform.lossyScale) + inset;

					if (IsVectorNegative(insetScale)) Gizmos.color = Color.red;
					Gizmos.matrix = Matrix4x4.TRS(clock.transform.position, clock.transform.rotation, insetScale);
					Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

					return;
				}

				MeshCollider meshCollider = clock.GetComponent<MeshCollider>();

				if (meshCollider != null)
				{
					Vector3 bounds = meshCollider.sharedMesh.bounds.size;
					float smallestBound = Mathf.Min(bounds.x, bounds.y, bounds.z);
					Vector3 normalizedBounds = smallestBound * new Vector3(1 / bounds.x, 1 / bounds.y, 1 / bounds.z);
					Vector3 insetScale = clock.transform.lossyScale + Vector3.Scale(normalizedBounds, inset);

					if (IsVectorNegative(insetScale)) Gizmos.color = Color.red;
					Gizmos.matrix = Matrix4x4.TRS(clock.transform.position, clock.transform.rotation, insetScale);
					Gizmos.DrawWireMesh(meshCollider.sharedMesh);

					return;
				}

				// Capsule colliders are not currently supported.
			}
		}
	}
}
