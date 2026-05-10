using FMODUnity;
using UnityEngine;
using _Scripts.Utils;

namespace _Scripts.KidsRoom;

[RequireComponent(typeof(Rigidbody), typeof(ToyCar))]
public class ToyCarMoveSound : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter moveSound;

	[SerializeField]
	private float minVelocityMoveSound = 20f;

	[SerializeField]
	private float maxVelocityMoveSound = 130f;

	[Space(10f)]
	[SerializeField]
	private bool hasSiren;

	[SerializeField]
	private float velocitySirenOn = 90f;

	[SerializeField]
	private float velocitySirenOff = 70f;

	private ToyCar toyCar;

	private Rigidbody rb;

	private bool isMoving;

	private bool sirenActive;

	private float velocityParameter;

	private void Awake()
	{
		toyCar = GetComponent<ToyCar>();
		rb = GetComponent<Rigidbody>();
		isMoving = false;
		sirenActive = false;
		if (moveSound == null)
		{
			Debug.LogError("Field 'moveSound' not assigned!");
		}
	}

	private void Update()
	{
		float magnitude = rb.linearVelocity.magnitude;
		HandleMoveSound(magnitude);
		HandleSiren(magnitude);
	}

	private void HandleSiren(float velocity)
	{
		if (hasSiren)
		{
			if (!sirenActive && velocity > velocitySirenOn)
			{
				sirenActive = true;
				moveSound.SetParameter("siren_switch", 1f);
				moveSound.SetParameter("silky_siren", toyCar.HasPlayerSpider() ? 1 : 0);
			}
			else if (sirenActive && velocity < velocitySirenOff)
			{
				sirenActive = false;
				moveSound.SetParameter("siren_switch", 0f);
			}
		}
	}

	private void HandleMoveSound(float velocity)
	{
		float b = Mathf.InverseLerp(minVelocityMoveSound, maxVelocityMoveSound, velocity);
		velocityParameter = _Scripts.Utils.Utils.ExponentialDecay(velocityParameter, b, 1f, Time.deltaTime);
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
			moveSound.SetParameter("velocity", velocityParameter);
		}
	}
}
