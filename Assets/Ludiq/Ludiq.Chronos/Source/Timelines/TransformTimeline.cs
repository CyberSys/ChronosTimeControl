using UnityEngine;

namespace Chronos
{
	public class TransformTimeline : RecorderTimeline<Transform, TransformTimeline.Snapshot>
	{
		// Scale is disabled by default because it usually doesn't change and 
		// would otherwise take more memory. Feel free to uncomment the lines
		// below if you need to record it.

		public struct Snapshot
		{
			public Vector3 position;
			public Quaternion rotation;
			//public Vector3 scale;

			public static Snapshot Lerp(Snapshot from, Snapshot to, float t)
			{
				return new Snapshot()
				{
					position = Vector3.Lerp(from.position, to.position, t),
					rotation = Quaternion.Lerp(from.rotation, to.rotation, t),
					//scale = Vector3.Lerp(from.scale, to.scale, t)
				};
			}
		}

		public TransformTimeline(Timeline timeline, Transform component) : base(timeline, component) { }

		protected override Snapshot LerpSnapshots(Snapshot from, Snapshot to, float t)
		{
			return Snapshot.Lerp(from, to, t);
		}

		protected override Snapshot CopySnapshot()
		{
			return new Snapshot()
			{
				position = component.position,
				rotation = component.rotation,
				//scale = component.localScale
			};
		}

		protected override void ApplySnapshot(Snapshot snapshot)
		{
			component.position = snapshot.position;
			component.rotation = snapshot.rotation;
			//component.localScale = snapshot.scale;
		}
	}
}
