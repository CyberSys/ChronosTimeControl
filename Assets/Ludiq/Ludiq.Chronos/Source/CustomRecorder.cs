using System;
using System.Collections.Generic;
using Chronos.Reflection;
using UnityEngine;

namespace Chronos
{
	public class CustomRecorder : Recorder<CustomRecorder.Snapshot>
	{
		public class Snapshot
		{
			public object[] values { get; private set; }

			public Snapshot(int size)
			{
				values = new object[size];
			}
		}

		[SerializeField, SelfTargeted, Filter(TypeFamilies = TypeFamily.Value, Inherited = true, ReadOnly = false)]
		private List<UnityVariable> variables;

		public CustomRecorder()
		{
			variables = new List<UnityVariable>();
		}

		protected override void ApplySnapshot(Snapshot snapshot)
		{
			int i = 0;

			foreach (UnityVariable variable in variables)
			{
				variable.Set(snapshot.values[i++]);
			}
		}

		protected override Snapshot CopySnapshot()
		{
			Snapshot snapshot = new Snapshot(variables.Count);

			int i = 0;

			foreach (UnityVariable variable in variables)
			{
				snapshot.values[i++] = variable.Get();
			}

			return snapshot;
		}

		protected override Snapshot LerpSnapshots(Snapshot from, Snapshot to, float t)
		{
			Snapshot snapshot = new Snapshot(from.values.Length);

			for (int i = 0; i < snapshot.values.Length; i++)
			{
				snapshot.values[i] = LerpValue(from.values[i], to.values[i], t);
			}

			return snapshot;
		}

		private static object LerpValue(object from, object to, float t)
		{
			object inter;

			Type type = from.GetType();

			if (type == typeof(float) || type == typeof(double)) inter = Mathf.Lerp((float)from, (float)to, t);
			else if (type == typeof(Vector3)) inter = Vector3.Lerp((Vector3)from, (Vector3)to, t);
			else if (type == typeof(Vector2)) inter = Vector2.Lerp((Vector2)from, (Vector2)to, t);
			else if (type == typeof(Quaternion)) inter = Quaternion.Lerp((Quaternion)from, (Quaternion)to, t);
			else if (type == typeof(Color)) inter = Color.Lerp((Color)from, (Color)to, t);
			else inter = from;

			return inter;
		}
	}
}
