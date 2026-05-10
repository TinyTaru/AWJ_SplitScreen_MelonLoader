using FMODUnity;
using UnityEngine;

namespace _Scripts.Objects;

[RequireComponent(typeof(Rigidbody))]
public class ClockHandSound : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter moveSound;

	[SerializeField]
	private float minVelocity = 0.5f;

	[SerializeField]
	private float maxVelocity = 2f;

	private Rigidbody rb;

	private bool isMoving;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		isMoving = false;
	}

	private void Update()
	{
		float magnitude = rb.angularVelocity.magnitude;
		float num = Mathf.InverseLerp(minVelocity, maxVelocity, magnitude);
		if (!isMoving && num > 0.01f)
		{
			isMoving = true;
			moveSound.Play();
		}
		else if (isMoving && num == 0f)
		{
			isMoving = false;
			moveSound.Stop();
		}
		if (isMoving)
		{
			moveSound.SetParameter("clockhand_speed", num);
		}
	}
}
