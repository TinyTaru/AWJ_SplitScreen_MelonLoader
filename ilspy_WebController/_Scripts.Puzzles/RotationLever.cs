using FMODUnity;
using UnityEngine;

namespace _Scripts.Puzzles;

[SelectionBase]
public class RotationLever : Activator
{
	[SerializeField]
	private bool initialState;

	[SerializeField]
	private Transform lever;

	[SerializeField]
	private float activationAngleThreshold = 30f;

	[SerializeField]
	private float deactivationAngleThreshold = -30f;

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

	private HingeJoint joint;

	private void Start()
	{
		SetActivated(initialState);
		joint = lever.GetComponent<HingeJoint>();
		float num = joint.limits.max / 2f;
		lever.localRotation = Quaternion.Euler(0f, 0f, initialState ? num : (0f - num));
	}

	private void Update()
	{
		float num = lever.localRotation.eulerAngles.z % 360f;
		if (num > 180f)
		{
			num -= 360f;
		}
		if (!base.Activated && num > activationAngleThreshold)
		{
			SetActivated(value: true);
		}
		else if (base.Activated && num < deactivationAngleThreshold)
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
			if (!isMoving && moveSound != null)
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
			if (moveSound != null)
			{
				moveSound.Stop();
			}
		}
	}
}
