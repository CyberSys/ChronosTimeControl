using UnityEngine;

namespace Chronos.Example
{
	// Utility script to change the color of a game object based
	// on the time scale of its timeline.
	public class ExampleTimeColor : MonoBehaviour
	{
		// The state colors
		private Color rewind = Color.magenta;
		private Color pause = Color.red;
		private Color slow = Color.yellow;
		private Color play = Color.green;
		private Color accelerate = Color.blue;

		// The time scales at which to apply colors
		private float slowTimeScale = 0.5f;
		private float rewindTimeScale = -1f;
		private float accelerateTimeScale = 2f;
		
		private Timeline time;
		private new Renderer renderer;
		private new ParticleSystem particleSystem;

		private void Awake()
		{
			time = GetComponentInParent<Timeline>();
			renderer = GetComponent<Renderer>();
			particleSystem = GetComponent<ParticleSystem>();
		}

		private void Update()
		{
			Color color = Color.white;

			// Get the timeline in the ancestors

			if (time != null)
			{
				float timeScale = time.timeScale;

				// Color lerping magic :)

				if (timeScale < 0)
				{
					color = Color.Lerp(pause, rewind, Mathf.Max(rewindTimeScale, timeScale) / rewindTimeScale);
				}
				else if (timeScale < slowTimeScale)
				{
					color = Color.Lerp(pause, slow, timeScale / slowTimeScale);
				}
				else if (timeScale < 1)
				{
					color = Color.Lerp(slow, play, (timeScale - slowTimeScale) / (1 - slowTimeScale));
				}
				else
				{
					color = Color.Lerp(play, accelerate, (timeScale - 1) / (accelerateTimeScale - 1));
				}
			}

			// Apply the color to the renderer (if any)
			if (renderer != null)
			{
				foreach (Material material in GetComponent<Renderer>().materials)
				{
					material.color = color;
				}
			}

			// Apply the color to the particle system (if any)
			if (particleSystem != null)
			{
				var particleSystemMain = particleSystem.main;
				particleSystemMain.startColor = color;
				var colorModule = particleSystem.colorOverLifetime;
				colorModule.color = new ParticleSystem.MinMaxGradient(color);
			}
		}
	}
}
