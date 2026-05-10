using System;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Spider;

namespace _Scripts.LivingRoom;

public class ScavengerHuntHintPiano : MonoBehaviour
{
	[SerializeField]
	private UnityEvent onHintFoundEvent;

	private bool isHintFound;

	public event Action OnHintFound;

	private void OnTriggerEnter(Collider other)
	{
		if (!isHintFound)
		{
			BodyMovement componentInParent = other.GetComponentInParent<BodyMovement>();
			if (componentInParent != null && componentInParent.IsPlayer)
			{
				isHintFound = true;
				this.OnHintFound?.Invoke();
				onHintFoundEvent?.Invoke();
			}
		}
	}
}
