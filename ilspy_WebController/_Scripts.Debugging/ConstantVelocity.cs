using UnityEngine;

namespace _Scripts.Debugging;

public class ConstantVelocity : MonoBehaviour
{
	[SerializeField]
	private Vector3 constantVelocity;

	private Rigidbody rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		rb.linearVelocity = constantVelocity;
	}
}
