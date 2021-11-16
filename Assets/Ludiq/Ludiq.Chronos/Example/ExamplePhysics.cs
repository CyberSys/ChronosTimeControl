using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chronos.Example
{
	// An example for Chronos with physics.
	// Will spawn a certain amount of boxes at random positions
	// in the air and attach the proper components to them for
	// Chronos support.
	public class ExamplePhysics : ExampleBaseBehaviour
	{
		// The delay between respawns
		public float delay = 5;

		// The amount of boxes to spawn
		public float amount = 10;

		// The list of spawned boxes
		private List<GameObject> spawned = new List<GameObject>();

		private void Start()
		{
			// Start spawning
			StartCoroutine(SpawnCoroutine());
		}

		private IEnumerator SpawnCoroutine()
		{
			// Loop forever
			while (true)
			{
				// Spawn the boxes
				Spawn();

				// Wait for the delay, taking in consideration
				// the time scales. Notice "time.WaitForSeconds"
				// instead of "new WaitForSeconds"
				yield return time.WaitForSeconds(delay);
			}
		}

		// Spawn the boxes
		private void Spawn()
		{
			// Clear the previously spawned boxes
			foreach (GameObject go in spawned)
			{
				Destroy(go);
			}

			spawned.Clear();

			for (int i = 0; i < amount; i++)
			{
				// Create a cube
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

				// Place it in the air at a random position
				go.transform.position = transform.position;
				go.transform.position += new Vector3()
				{
					x = Random.Range(-1f, +1f),
					y = 2 * i,
					z = Random.Range(-1f, +1f)
				};

				// Give it a rigidbody
				go.AddComponent<Rigidbody>();

				// Give it a timeline in that watches the same
				// global clock as this component
				Timeline timeline = go.AddComponent<Timeline>();
				timeline.mode = TimelineMode.Global;
				timeline.globalClockKey = time.globalClockKey;
				timeline.rewindable = time.rewindable;

				// Add the time-color script for easy visualisation
				go.AddComponent<ExampleTimeColor>();

				// Store the spawned object so we can destroy it on respawn
				spawned.Add(go);
			}
		}
	}
}
