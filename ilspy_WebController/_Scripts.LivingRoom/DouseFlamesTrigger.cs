using UnityEngine;
using UnityEngine.Events;
using _Scripts.Objects;

namespace _Scripts.LivingRoom;

public class DouseFlamesTrigger : MonoBehaviour
{
	[SerializeField]
	private UnityEvent onDouseFlamesEvent;

	private void OnTriggerEnter(Collider other)
	{
		MovableObject componentInParent = other.GetComponentInParent<MovableObject>();
		if (componentInParent != null && componentInParent.gameObject.layer == LayerMask.NameToLayer("Water"))
		{
			onDouseFlamesEvent?.Invoke();
			componentInParent.DestroySafely();
		}
	}
}
