using UnityEngine;

namespace Chronos.Example
{
	// Example for Chronos with Occurrences.
	// Will change color at scheduled time. This code is rewindable.
	// More information:
	// http://ludiq.io/chronos/documentation#Occurrence

	[RequireComponent(typeof(Renderer))]
	public class ExampleOccurrences : ExampleBaseBehaviour
	{
		// Subclass the occurrence class (see documentation)
		private class ChangeColorOccurrence : Occurrence
		{
			private Material material;
			private Color newColor;
			private Color previousColor;

			public ChangeColorOccurrence(Material material, Color newColor)
			{
				this.material = material;
				this.newColor = newColor;
			}

			// The action when time is going forward
			public override void Forward()
			{
				previousColor = material.color;
				material.color = newColor;
			}

			// The action when time is going backward
			public override void Backward()
			{
				material.color = previousColor;
			}
		}

		private void Start()
		{
			// Get the renderer's material
			Material material = GetComponent<Renderer>().material;
			
			// Change the color to yellow now
			time.Do(true, new ChangeColorOccurrence(material, Color.yellow));

			// Change the color to blue in 5 seconds
			time.Plan(5, true, new ChangeColorOccurrence(material, Color.blue));

			// Change the color to green in 7 seconds
			time.Plan(7, true, new ChangeColorOccurrence(material, Color.green));

			// Change the color to red in 10 seconds
			time.Plan(10, true, new ChangeColorOccurrence(material, Color.red));

			// Plan an occurrence, but cancel it.
			Occurrence changeToMagenta = time.Plan(3, true, new ChangeColorOccurrence(material, Color.magenta));
			time.Cancel(changeToMagenta);
		}
	}
}
