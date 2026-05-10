using UnityEngine;
using _Scripts.Utils;

namespace _Scripts.LivingRoom;

public class RobotVacuumCleanerBumper : MonoBehaviour
{
	[SerializeField]
	private LayerMask collisionLayerMask;

	private RobotVacuumCleaner robotVacuumCleaner;

	private void Awake()
	{
		robotVacuumCleaner = GetComponentInParent<RobotVacuumCleaner>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_Scripts.Utils.Utils.IsLayerInLayerMask(other.gameObject.layer, collisionLayerMask))
		{
			robotVacuumCleaner.Collision();
		}
	}
}
