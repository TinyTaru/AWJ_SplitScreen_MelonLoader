using FMODUnity;
using UnityEngine;
using _Scripts.Utils;

namespace _Scripts.Objects;

[RequireComponent(typeof(Rigidbody))]
public class MoveSound : MonoBehaviour
{
	[SerializeField]
	private bool debug;

	[SerializeField]
	private StudioEventEmitter moveSound;

	[SerializeField]
	private string velocityParameterName = "velocity";

	[SerializeField]
	private float minVelocity;

	[SerializeField]
	private float maxVelocity = 10f;

	[Tooltip("1 = slow, 25 = fast)")]
	[Range(1f, 25f)]
	[SerializeField]
	private float exponentialDecay = 1f;

	private Rigidbody rb;

	private bool isMoving;

	private float velocityParameter;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		isMoving = false;
	}

	private void Update()
	{
		float magnitude = rb.linearVelocity.magnitude;
		if (debug)
		{
			Debug.Log(magnitude);
		}
		float b = Mathf.InverseLerp(minVelocity, maxVelocity, magnitude);
		velocityParameter = _Scripts.Utils.Utils.ExponentialDecay(velocityParameter, b, exponentialDecay, Time.deltaTime);
		if (!isMoving && velocityParameter > 0.01f)
		{
			isMoving = true;
			moveSound.Play();
		}
		else if (isMoving && velocityParameter == 0f)
		{
			isMoving = false;
			moveSound.Stop();
		}
		if (isMoving)
		{
			moveSound.SetParameter(velocityParameterName, velocityParameter);
		}
	}
}
