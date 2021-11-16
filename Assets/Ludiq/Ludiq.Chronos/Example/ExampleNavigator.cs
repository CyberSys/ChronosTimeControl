using System.Collections;
using UnityEngine;

namespace Chronos.Example
{
	// Example for Chronos with NavMeshAgents.
	// Will choose a random destination within a preset area at
	// a regular interval.
	[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
	public class ExampleNavigator : ExampleBaseBehaviour
	{
		// The delay between destination changes
		public float delay = 5;

		// The center of the potential destinations area
		public Vector3 areaCenter;

		// The size of the potential destinations area
		public Vector3 areaSize;

		private void Start()
		{
			// Start the coroutine
			StartCoroutine(ChangeDestinationCoroutine());
		}

		private IEnumerator ChangeDestinationCoroutine()
		{
			// Loop forever
			while (true)
			{
				// Choose a new destination
				ChangeDestination();

				// Wait for the delay, taking in consideration
				// the time scales. Notice "time.WaitForSeconds"
				// instead of "new WaitForSeconds"
				yield return time.WaitForSeconds(delay);
			}
		}

		private void ChangeDestination()
		{
			// Choose a new destination within the potential area
			Vector3 randomDestination = new Vector3()
			{
				x = areaCenter.x + Random.Range(-areaSize.x, +areaSize.x) / 2,
				y = areaCenter.y + Random.Range(-areaSize.y, +areaSize.y) / 2,
				z = areaCenter.z + Random.Range(-areaSize.z, +areaSize.z) / 2,
			};

			// Set it to the nav mesh agent
			GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(randomDestination);
		}

		// Draw the area gizmos for easy manipulation
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.magenta;

			Gizmos.DrawWireCube(areaCenter, areaSize);
		}
	}
}
