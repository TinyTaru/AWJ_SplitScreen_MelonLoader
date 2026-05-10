using UnityEngine;

namespace _Scripts.Objects;

public class WaterOrbTutorial : WaterOrb
{
	[SerializeField]
	private GameObject buttonPrompt;

	protected new void OnCollisionEnter(Collision collision)
	{
		base.OnCollisionEnter(collision);
		FanControlUnit componentInParent = collision.gameObject.GetComponentInParent<FanControlUnit>();
		if (componentInParent != null)
		{
			componentInParent.TurnOff();
			DestroyWaterOrb();
		}
	}

	public void EnableButtonPrompt(bool value)
	{
		buttonPrompt.SetActive(value);
	}
}
