using UnityEngine;

namespace _Scripts.Puzzles;

[SelectionBase]
public class WeightButton : Activator
{
	[SerializeField]
	private Transform button;

	[SerializeField]
	private float activationThreshold = -0.15f;

	[SerializeField]
	private float deactivationThreshold = -0.1f;

	private void Update()
	{
		if (!base.Activated && button.localPosition.y < activationThreshold)
		{
			SetActivated(value: true);
		}
		else if (base.Activated && button.localPosition.y > deactivationThreshold)
		{
			SetActivated(value: false);
		}
	}
}
