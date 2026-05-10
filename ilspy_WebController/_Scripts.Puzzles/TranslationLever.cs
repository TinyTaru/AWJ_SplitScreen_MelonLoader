using FMODUnity;
using UnityEngine;

namespace _Scripts.Puzzles;

public class TranslationLever : Activator
{
	[SerializeField]
	private Transform lever;

	[SerializeField]
	private float activationThreshold = 5.8f;

	[SerializeField]
	private float deactivationThreshold = 5.5f;

	[SerializeField]
	private float moveSoundStartThreshold = 1f;

	[SerializeField]
	private float moveSoundStopThreshold = 0.1f;

	[SerializeField]
	private float minVelocityMoveSound;

	[SerializeField]
	private float maxVelocityMoveSound = 5f;

	[SerializeField]
	private float moveSoundStopDelay = 0.1f;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter moveSound;

	private bool isMoving;

	private float moveTimer;

	private void Start()
	{
		SetActivated(value: false);
	}

	private void Update()
	{
		float z = lever.localPosition.z;
		if (!base.Activated && z > activationThreshold)
		{
			SetActivated(value: true);
		}
		else if (base.Activated && z < deactivationThreshold)
		{
			SetActivated(value: false);
		}
		Rigidbody component = lever.GetComponent<Rigidbody>();
		if (!(component != null))
		{
			return;
		}
		float magnitude = component.linearVelocity.magnitude;
		moveSound.Params[0].Value = Mathf.Clamp01((magnitude - minVelocityMoveSound) / (maxVelocityMoveSound - minVelocityMoveSound));
		if (magnitude > moveSoundStartThreshold)
		{
			if (!isMoving)
			{
				moveSound.Play();
			}
			isMoving = true;
			moveTimer = moveSoundStopDelay;
		}
		else if (magnitude < moveSoundStopThreshold)
		{
			moveTimer -= Time.deltaTime;
		}
		else if (magnitude == 0f)
		{
			moveTimer = 0f;
		}
		if (moveTimer <= 0f)
		{
			isMoving = false;
			moveSound.Stop();
		}
	}
}
