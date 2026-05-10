using UnityEngine;

namespace _Scripts.Objects;

public class KillColliderBlendableObject : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		MovableObject componentInParent = other.GetComponentInParent<MovableObject>();
		if (!(componentInParent == null))
		{
			BlendableObject componentInParent2 = componentInParent.GetComponentInParent<BlendableObject>();
			if (!(componentInParent2 == null))
			{
				componentInParent2.GetMovableObject().DestroySafely();
			}
		}
	}
}
