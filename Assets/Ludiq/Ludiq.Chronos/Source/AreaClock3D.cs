using UnityEngine;

namespace Chronos
{
	/// <summary>
	/// A Clock that affects every Timeline within its 3D collider by multiplying its time scale with that of their observed clock.
	/// </summary>
	[AddComponentMenu("Time/Area Clock 3D")]
	[DisallowMultipleComponent]
	[HelpURL("http://ludiq.io/chronos/documentation#AreaClock")]
	public class AreaClock3D : AreaClock<Collider, Vector3>
	{
		protected virtual void OnTriggerEnter(Collider other)
		{
			if (!enabled)
			{
				return;
			}

			Timeline timeline = other.GetComponent<Timeline>();

			if (timeline != null)
			{
				// Store local coordinates to account for dynamic changes of the clock's transform
				Vector3 entry = transform.InverseTransformPoint(other.transform.position);

				Capture(timeline, entry);
			}
		}

		protected virtual void OnTriggerExit(Collider collider)
		{
			Timeline timeline = collider.GetComponent<Timeline>();

			if (timeline != null)
			{
				Release(timeline);
			}
		}

		protected override float PointToEdgeTimeScale(Vector3 position)
		{
			Vector3 center = transform.TransformPoint(this.center);
			Vector3 delta = position - center;
			Vector3 direction = delta.normalized;
			float distance = delta.magnitude;
			float noEffect = innerBlend == ClockBlend.Multiplicative ? 1 : 0;

			// For exact center...

			if (direction == Vector3.zero)
			{
				return Mathf.Lerp(noEffect, timeScale, curve.Evaluate(0));
			}

			// For sphere colliders centered at origin...

			SphereCollider sphereCollider = collider as SphereCollider;

			if (sphereCollider != null && sphereCollider.center == this.center)
			{
				Vector3 scale = transform.lossyScale;
				float maxScale = Mathf.Max(scale.x, scale.y, scale.z);
				float radius = sphereCollider.radius * maxScale;
				Vector3 edge = center + direction * radius;

				if (Timekeeper.instance.debug)
				{
					Debug.DrawLine(center, edge, Color.cyan);
					Debug.DrawLine(center, position, Color.magenta);
				}

				return Mathf.Lerp(noEffect, timeScale, curve.Evaluate(distance / radius));
			}

			// For any collider at any origin...

			// This ray length should always be sufficient -- the biggest distance within the AABB
			float rayLength = (collider.bounds.max - collider.bounds.min).magnitude;

			// Reverse the ray because a collider raycast doesn't work from within
			Ray ray = new Ray(center, direction);
			ray.origin = ray.GetPoint(rayLength);
			ray.direction = -ray.direction;
			RaycastHit hit;

			if (collider.Raycast(ray, out hit, rayLength))
			{
				Vector3 edge = hit.point;
				float edgeDistance = (edge - center).magnitude;

				if (Timekeeper.instance.debug)
				{
					Debug.DrawLine(center, edge, Color.cyan);
					Debug.DrawLine(center, position, Color.magenta);
				}

				return Mathf.Lerp(noEffect, timeScale, curve.Evaluate(distance / edgeDistance));
			}
			else
			{
				Debug.LogWarning("Area clock cannot find its collider's edge. Make sure the center is inside and the collider is convex.");

				return timeScale;
			}
		}

		public override bool ContainsPoint(Vector3 point)
		{
			return collider.bounds.Contains(point);
		}

		/// <summary>
		/// The components used by the area clock are cached for performance optimization. If you add or remove the Collider on the GameObject, you need to call this method to update the area clock accordingly. 
		/// </summary>
		public override void CacheComponents()
		{
			collider = GetComponent<Collider>();

			if (collider == null)
			{
				throw new ChronosException(string.Format("Missing collider for area clock."));
			}
		}
	}
}
