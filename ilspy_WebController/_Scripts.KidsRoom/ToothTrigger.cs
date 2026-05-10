using UnityEngine;
using UnityEngine.Events;
using _Scripts.Objects;

namespace _Scripts.KidsRoom;

public class ToothTrigger : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private MovableObject coinPrefab;

	[SerializeField]
	private GameObject particleEffect;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onToothExchangedEvent;

	private void OnTriggerEnter(Collider other)
	{
		MovableObject componentInParent = other.GetComponentInParent<MovableObject>();
		if (!(componentInParent == null) && componentInParent.name.Contains("Tooth"))
		{
			componentInParent.DestroySafely();
			Vector3 linearVelocity = componentInParent.GetRigidbody().linearVelocity;
			MovableObject movableObject = Object.Instantiate(coinPrefab, componentInParent.transform.position, Quaternion.identity);
			movableObject.GetRigidbody().linearVelocity = linearVelocity;
			Object.Instantiate(particleEffect, movableObject.transform.position, Quaternion.identity, movableObject.transform);
			onToothExchangedEvent?.Invoke();
		}
	}
}
