using UnityEngine;

namespace _Scripts.Objects;

public class WaterOrbKitchen : WaterOrb
{
	protected new void OnCollisionEnter(Collision collision)
	{
		base.OnCollisionEnter(collision);
		KitchenPlant componentInParent = collision.gameObject.GetComponentInParent<KitchenPlant>();
		if (componentInParent != null && componentInParent.WaterPlant())
		{
			DestroyWaterOrb();
		}
		CleanableObject componentInParent2 = collision.gameObject.GetComponentInParent<CleanableObject>();
		if (componentInParent2 != null && componentInParent2.RemoveDirtAmountAbsolute(1.1f))
		{
			DestroyWaterOrb();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Splat componentInParent = other.GetComponentInParent<Splat>();
		if (componentInParent != null && componentInParent.RemoveDirtAmountAbsolute(1.1f))
		{
			DestroyWaterOrb();
		}
	}
}
