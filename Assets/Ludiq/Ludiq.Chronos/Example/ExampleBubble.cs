using System.Collections;
using UnityEngine;

namespace Chronos.Example
{
	// This class is pretty much identical to ExampleNavigator, except
	// that it doesn't take into consideration Chronos. It's just used
	// as an utility to move the area clock bubbles in the example scene --
	// this script is not an example by itself.
	[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
	public class ExampleBubble : MonoBehaviour
	{
		public float delay = 5;
		public Vector3 areaCenter;
		public Vector3 areaSize;

		private void Start()
		{
			StartCoroutine(ChangeDestinationCoroutine());
		}

		private IEnumerator ChangeDestinationCoroutine()
		{
			while (true)
			{
				ChangeDestination();

				yield return new WaitForSeconds(delay);
			}
		}

		private void ChangeDestination()
		{
			Vector3 randomDestination = new Vector3()
			{
				x = areaCenter.x + Random.Range(-areaSize.x, +areaSize.x) / 2,
				y = areaCenter.y + Random.Range(-areaSize.y, +areaSize.y) / 2,
				z = areaCenter.z + Random.Range(-areaSize.z, +areaSize.z) / 2,
			};

			GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(randomDestination);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.magenta;

			Gizmos.DrawWireCube(areaCenter, areaSize);
		}
	}
}
