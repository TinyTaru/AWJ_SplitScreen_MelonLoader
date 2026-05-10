using FMODUnity;
using UnityEngine;

namespace _Scripts.LivingRoom;

public class FishSuctionArea : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter suctionSound;

	private Fish fish;

	private Shroomp shroomp;

	private void Awake()
	{
		fish = GetComponentInParent<Fish>();
		shroomp = GetComponentInParent<Shroomp>();
	}

	private void OnTriggerEnter(Collider other)
	{
		FishFood componentInParent = other.gameObject.GetComponentInParent<FishFood>();
		if (!(componentInParent != null) || componentInParent.IsBeingEaten)
		{
			return;
		}
		if (fish != null && !fish.IsEating)
		{
			fish.StartEating(componentInParent);
			if (suctionSound != null)
			{
				suctionSound.Play();
			}
		}
		if (shroomp != null && !shroomp.IsEating)
		{
			shroomp.StartEating(componentInParent);
			if (suctionSound != null)
			{
				suctionSound.Play();
			}
		}
	}
}
