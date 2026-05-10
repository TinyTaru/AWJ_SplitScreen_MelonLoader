using FMODUnity;
using UnityEngine;

namespace _Scripts.Objects;

[RequireComponent(typeof(MovableObject))]
public class DoorSounds : MonoBehaviour
{
	[Header("Move")]
	[SerializeField]
	private StudioEventEmitter doorMoveSound;

	[SerializeField]
	private string moveParameterName = "velocity";

	[SerializeField]
	private float maxMoveVelocity = 1f;

	[Header("Shut")]
	[SerializeField]
	private StudioEventEmitter doorShutSound;

	[SerializeField]
	private string shutParameterName = "volume";

	[SerializeField]
	private float maxShutVelocity = 1f;

	private Rigidbody rb;

	private Joint joint;

	private bool isMoving;

	private float shutAngle;

	private bool isShut;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		joint = GetComponent<Joint>();
		shutAngle = rb.rotation.eulerAngles.y;
	}

	private void OnDisable()
	{
		if (doorMoveSound != null)
		{
			doorMoveSound.Stop();
		}
		if (doorShutSound != null)
		{
			doorShutSound.Stop();
		}
	}

	private void Start()
	{
		isMoving = false;
		isShut = true;
	}

	private void Update()
	{
		if (!(joint == null))
		{
			float velocity = Mathf.Abs(rb.angularVelocity.y);
			HandleMoveSound(velocity);
			HandleShutSound(velocity);
		}
	}

	private void HandleShutSound(float velocity)
	{
		if (!(doorShutSound == null))
		{
			float num = Mathf.Abs(rb.rotation.eulerAngles.y - shutAngle);
			if (isShut && num > 2f)
			{
				isShut = false;
			}
			else if (!isShut && num < 1f)
			{
				isShut = true;
				doorShutSound.Play();
				doorShutSound.SetParameter(shutParameterName, velocity / maxShutVelocity);
			}
		}
	}

	private void HandleMoveSound(float velocity)
	{
		if (!(doorMoveSound == null))
		{
			doorMoveSound.SetParameter(moveParameterName, velocity / maxMoveVelocity);
			if (!isMoving && velocity > 0.2f)
			{
				isMoving = true;
				doorMoveSound.Play();
			}
			else if (isMoving && velocity < 0.1f)
			{
				isMoving = false;
				doorMoveSound.Stop();
			}
		}
	}
}
