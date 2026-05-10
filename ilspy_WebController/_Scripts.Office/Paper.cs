using UnityEngine;
using _Scripts.Objects;

namespace _Scripts.Office;

[RequireComponent(typeof(MovableObject))]
public class Paper : MonoBehaviour
{
	[SerializeField]
	private Rigidbody rb;

	public void SetKinematic(bool value)
	{
		rb.isKinematic = value;
	}
}
