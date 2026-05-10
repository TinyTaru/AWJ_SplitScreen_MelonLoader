using FMODUnity;
using UnityEngine;

namespace _Scripts.LivingRoom;

public class RobotVacuumCleanerSuctionArea : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter suctionSound;

	private void OnTriggerEnter(Collider other)
	{
		DustBunny componentInParent = other.gameObject.GetComponentInParent<DustBunny>();
		if (componentInParent != null)
		{
			if (!componentInParent.IsCleaned)
			{
				suctionSound.Play();
			}
			componentInParent.Clean(base.transform);
		}
	}
}
