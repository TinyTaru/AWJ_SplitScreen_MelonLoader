using UnityEngine;
using _Scripts.Spider;

namespace _Scripts.Game;

public class Checkpoint : MonoBehaviour
{
	[SerializeField]
	private Transform resetPosition;

	private void OnTriggerEnter(Collider other)
	{
		BodyMovement component = other.GetComponent<BodyMovement>();
		if (component != null && component.IsPlayer)
		{
			CheckpointController.Instance.SetActiveCheckpoint(this);
		}
	}
}
