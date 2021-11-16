using UnityEngine;

namespace Chronos
{
	/// <summary>
	/// A Clock that affects every Timeline within its 2D collider by multiplying its time scale with that of their observed clock. 
	/// </summary>
	[AddComponentMenu("Time/Area Clock 2D")]
	[DisallowMultipleComponent]
	[HelpURL("http://ludiq.io/chronos/documentation#AreaClock")]
	public class AreaClock2D : AreaClock<Collider2D, Vector2>
	{
		protected virtual void OnTriggerEnter2D(Collider2D other)
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

		protected virtual void OnTriggerExit2D(Collider2D collider)
		{
			Timeline timeline = collider.GetComponent<Timeline>();

			if (timeline != null)
			{
				Release(timeline);
			}
		}

		protected override float PointToEdgeTimeScale(Vector3 position)
		{
			Vector2 center = transform.TransformPoint(this.center);
			Vector2 delta = (Vector2) position - center;
			Vector2 direction = delta.normalized;
			float distance = delta.magnitude;
			float noEffect = innerBlend == ClockBlend.Multiplicative ? 1 : 0;

			// For exact center...

			if (direction == Vector2.zero)
			{
				return Mathf.Lerp(noEffect, timeScale, curve.Evaluate(0));
			}

			// For circle colliders centered at origin...

			CircleCollider2D circleCollider = collider as CircleCollider2D;

			if (circleCollider != null && circleCollider.offset == this.center)
			{
				Vector3 scale = transform.lossyScale;
				float maxScale = Mathf.Max(scale.x, scale.y);
				float radius = circleCollider.radius * maxScale;
				Vector2 edge = center + (radius / 2 * direction);

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
			Vector2 origin = center + (rayLength * direction);
			direction = -direction;

			// Unfortunately, we have to use RaycastAll() because there is no Collider2D.Raycast()
			RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, rayLength, 1 << gameObject.layer);

			foreach (RaycastHit2D hit in hits)
			{
				if (hit.collider == collider)
				{
					Vector2 edge = hit.point;
					float edgeDistance = (edge - center).magnitude;

					if (Timekeeper.instance.debug)
					{
						Debug.DrawLine(center, edge, Color.cyan);
						Debug.DrawLine(center, position, Color.magenta);
					}

					return Mathf.Lerp(noEffect, timeScale, curve.Evaluate(distance / edgeDistance));
				}
			}

			Debug.LogWarning("Area clock cannot find its collider's edge. Make sure the center is inside and the collider is convex.");

			return timeScale;
		}

		public override bool ContainsPoint(Vector3 point)
		{
			// Manually increase Z size in case the colliders aren't perfectly aligned.

			Bounds bounds = collider.bounds;
			Vector3 size = bounds.size;
			bounds.size = new Vector3(size.x, size.y, 999);

			return bounds.Contains(point);
		}

		/// <summary>
		/// The components used by the area clock are cached for performance optimization. If you add or remove the Collider2D on the GameObject, you need to call this method to update the area clock accordingly. 
		/// </summary>
		public override void CacheComponents()
		{
			collider = GetComponent<Collider2D>();

			if (collider == null)
			{
				throw new ChronosException(string.Format("Missing collider for area clock."));
			}
		}
	}
}
