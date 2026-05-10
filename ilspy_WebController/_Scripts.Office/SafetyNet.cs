using System.Linq;
using UnityEngine;
using _Scripts.Objects;

namespace _Scripts.Office;

public class SafetyNet : MonoBehaviour
{
	[SerializeField]
	private float resetHeight = 20f;

	[SerializeField]
	private MovableObject[] importantObjects;

	private void OnTriggerEnter(Collider other)
	{
		MovableObject componentInParent = other.GetComponentInParent<MovableObject>();
		if (!(componentInParent == null) && importantObjects.Contains(componentInParent))
		{
			Rigidbody rigidbody = componentInParent.GetRigidbody();
			rigidbody.linearVelocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.one;
			Vector3 position = rigidbody.position;
			position.y = resetHeight;
			rigidbody.position = position;
		}
	}
}
