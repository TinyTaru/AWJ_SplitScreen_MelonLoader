using UnityEngine;

namespace _Scripts.KidsRoom;

public class Balloon : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private float floatingForce;

	private Rigidbody rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		rb.AddForce(-Physics.gravity.normalized * (floatingForce * Time.fixedDeltaTime), ForceMode.Force);
	}
}
