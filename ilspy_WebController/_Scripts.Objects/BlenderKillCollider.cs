using UnityEngine;
using _Scripts.Spider;

namespace _Scripts.Objects;

public class BlenderKillCollider : MonoBehaviour
{
	[SerializeField]
	private Blender blender;

	private void OnTriggerEnter(Collider other)
	{
		MovableObject componentInParent = other.GetComponentInParent<MovableObject>();
		if (componentInParent != null)
		{
			BlendableObject componentInParent2 = componentInParent.GetComponentInParent<BlendableObject>();
			if (componentInParent2 == null)
			{
				blender.EjectObject(componentInParent);
			}
			else
			{
				blender.KillObject(componentInParent2);
			}
		}
		BodyMovement componentInParent3 = other.GetComponentInParent<BodyMovement>();
		if (componentInParent3 != null)
		{
			blender.YeetPlayer(componentInParent3);
		}
	}
}
