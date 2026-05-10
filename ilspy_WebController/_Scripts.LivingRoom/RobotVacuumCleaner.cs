using System;
using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.LivingRoom;

public class RobotVacuumCleaner : MonoBehaviour
{
	private enum AutomaticModeState
	{
		MoveForward,
		ResolveCollision
	}

	[Header("References")]
	[SerializeField]
	private Rigidbody rigidbodyRobot;

	[Header("Parameters")]
	[SerializeField]
	private float maxSpeed;

	[SerializeField]
	private float maxRotationSpeed;

	[SerializeField]
	private float moveForwardForce;

	[FormerlySerializedAs("maxTurnSpeed")]
	[SerializeField]
	private float maxTurnTorque;

	[SerializeField]
	private float collisionBackwardDuration = 1f;

	[SerializeField]
	private float collisionMinTurnDuration = 2f;

	[SerializeField]
	private float collisionMaxTurnDuration = 4f;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter moveLoopSound;

	[SerializeField]
	private StudioEventEmitter turnLoopSound;

	private bool moveForward;

	private bool turnLeft;

	private bool turnRight;

	private bool automaticModeActive;

	private AutomaticModeState automaticModeState;

	private bool moveForwardAutomatic;

	private bool moveBackwardAutomatic;

	private bool turnLeftAutomatic;

	private bool turnRightAutomatic;

	private bool wasMoving;

	private bool wasTurning;

	private void Start()
	{
		automaticModeState = AutomaticModeState.MoveForward;
		moveLoopSound.Play();
	}

	private void Update()
	{
		if (automaticModeActive)
		{
			AutomaticModeState automaticModeState = this.automaticModeState;
			if (automaticModeState == AutomaticModeState.MoveForward || automaticModeState != AutomaticModeState.ResolveCollision)
			{
				moveForwardAutomatic = true;
			}
		}
	}

	private void FixedUpdate()
	{
		HandleForwardMovement();
		HandleTurning();
		HandleMoveSound();
		HandleTurnSound();
	}

	private IEnumerator AutomaticModeCollisionCoroutine()
	{
		automaticModeState = AutomaticModeState.ResolveCollision;
		moveForwardAutomatic = false;
		moveBackwardAutomatic = true;
		yield return new WaitForSeconds(collisionBackwardDuration);
		moveBackwardAutomatic = false;
		float num = UnityEngine.Random.Range(-1f, 1f);
		float seconds = UnityEngine.Random.Range(collisionMinTurnDuration, collisionMaxTurnDuration);
		if (num > 0f)
		{
			turnRightAutomatic = true;
		}
		else
		{
			turnLeftAutomatic = true;
		}
		yield return new WaitForSeconds(seconds);
		turnRightAutomatic = false;
		turnLeftAutomatic = false;
		automaticModeState = AutomaticModeState.MoveForward;
	}

	private void HandleForwardMovement()
	{
		if ((moveForward || moveForwardAutomatic) && rigidbodyRobot.linearVelocity.sqrMagnitude < maxSpeed * maxSpeed)
		{
			rigidbodyRobot.AddForce(rigidbodyRobot.transform.forward * moveForwardForce, ForceMode.Force);
		}
		if (moveBackwardAutomatic && rigidbodyRobot.linearVelocity.sqrMagnitude < maxSpeed * maxSpeed)
		{
			rigidbodyRobot.AddForce(-rigidbodyRobot.transform.forward * moveForwardForce, ForceMode.Force);
		}
	}

	private void HandleMoveSound()
	{
		float value = rigidbodyRobot.linearVelocity.magnitude / maxSpeed;
		moveLoopSound.SetParameter("roomba_velocity", value);
	}

	private void HandleTurning()
	{
		if (!(rigidbodyRobot.angularVelocity.sqrMagnitude > maxRotationSpeed * maxRotationSpeed * (MathF.PI / 180f) * (MathF.PI / 180f)))
		{
			float num = 0f;
			if (turnLeft || turnLeftAutomatic)
			{
				num -= maxTurnTorque;
			}
			if (turnRight || turnRightAutomatic)
			{
				num += maxTurnTorque;
			}
			Vector3 torque = rigidbodyRobot.transform.up * num;
			rigidbodyRobot.AddTorque(torque, ForceMode.Force);
		}
	}

	private void HandleTurnSound()
	{
		bool flag = turnLeft || turnLeftAutomatic || turnRight || turnRightAutomatic;
		if (flag && !wasTurning)
		{
			turnLoopSound.Play();
		}
		else if (!flag && wasTurning)
		{
			turnLoopSound.Stop();
		}
		wasTurning = flag;
	}

	public void PressMoveForwardButton()
	{
		if (!automaticModeActive)
		{
			moveForward = true;
		}
	}

	public void ReleaseMoveForwardButton()
	{
		moveForward = false;
	}

	public void PressTurnLeftButton()
	{
		turnLeft = true;
	}

	public void ReleaseTurnLeftButton()
	{
		turnLeft = false;
	}

	public void PressTurnRightButton()
	{
		turnRight = true;
	}

	public void ReleaseTurnRightButton()
	{
		turnRight = false;
	}

	public void PressAutomaticModeButton()
	{
		automaticModeActive = true;
	}

	public void ReleaseAutomaticModeButton()
	{
		automaticModeActive = false;
		automaticModeState = AutomaticModeState.MoveForward;
		moveForwardAutomatic = false;
		moveBackwardAutomatic = false;
		turnLeftAutomatic = false;
		turnRightAutomatic = false;
	}

	public void Collision()
	{
		if (automaticModeActive && automaticModeState != AutomaticModeState.ResolveCollision)
		{
			StartCoroutine(AutomaticModeCollisionCoroutine());
		}
	}
}
