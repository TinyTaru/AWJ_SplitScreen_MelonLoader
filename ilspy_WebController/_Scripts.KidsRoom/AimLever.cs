using System;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.KidsRoom;

public class AimLever : MonoBehaviour
{
	private enum AimDirection
	{
		None,
		Up,
		Down
	}

	[Header("References")]
	[SerializeField]
	private Transform lever;

	[Header("Parameters")]
	[SerializeField]
	private float aimUpThreshold = 40f;

	[SerializeField]
	private float aimDownThreshold = -40f;

	[SerializeField]
	private float resetThreshold = 30f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onStartAimingUpEvent;

	[SerializeField]
	private UnityEvent onStartAimingDownEvent;

	[SerializeField]
	private UnityEvent onStopAimingEvent;

	private AimDirection aimDirection;

	private void Start()
	{
		aimDirection = AimDirection.None;
	}

	private void FixedUpdate()
	{
		float num = lever.localRotation.eulerAngles.x;
		if (num > 180f)
		{
			num -= 360f;
		}
		switch (aimDirection)
		{
		case AimDirection.None:
			if (num > aimUpThreshold)
			{
				aimDirection = AimDirection.Up;
				onStartAimingUpEvent?.Invoke();
			}
			else if (num < aimDownThreshold)
			{
				aimDirection = AimDirection.Down;
				onStartAimingDownEvent?.Invoke();
			}
			break;
		case AimDirection.Up:
		case AimDirection.Down:
			if (Mathf.Abs(num) < resetThreshold)
			{
				aimDirection = AimDirection.None;
				onStopAimingEvent?.Invoke();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
