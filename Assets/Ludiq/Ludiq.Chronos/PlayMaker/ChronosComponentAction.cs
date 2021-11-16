#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;
using UnityEngine;

namespace Chronos.PlayMaker
{
	public abstract class ChronosComponentAction<T> : FsmStateAction where T : Component
	{
		private GameObject cachedGameObject;
		protected T component;

		protected Timeline timeline
		{
			get { return component as Timeline; }
		}

		protected Clock clock
		{
			get { return component as Clock; }
		}

		protected GlobalClock globalClock
		{
			get { return component as GlobalClock; }
		}

		protected LocalClock localClock
		{
			get { return component as LocalClock; }
		}

		protected IAreaClock areaClock
		{
			get { return component as IAreaClock; }
		}

		protected AreaClock3D areaClock3D
		{
			get { return component as AreaClock3D; }
		}

		protected AreaClock2D areaClock2D
		{
			get { return component as AreaClock2D; }
		}

		protected bool UpdateCache(GameObject go)
		{
			if (go == null)
			{
				return false;
			}

			if (component == null || cachedGameObject != go)
			{
				component = go.GetComponent<T>();
				cachedGameObject = go;

				if (component == null)
				{
					LogWarning("Missing component: " + typeof(T).FullName + " on: " + go.name);
				}
			}

			return component != null;
		}
	}
}

#endif
