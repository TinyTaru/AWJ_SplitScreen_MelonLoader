using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace _Scripts.Objects;

public class Toaster : MonoBehaviour, IAffectedByWater
{
	private enum ToasterState
	{
		Unplugged,
		Idle,
		Toasting,
		Pop,
		Cooldown,
		Disabled
	}

	[SerializeField]
	private ToasterState initialToasterState;

	[SerializeField]
	private float popDuration = 0.2f;

	[FormerlySerializedAs("idleDelay")]
	[SerializeField]
	private float cooldownDuration = 0.5f;

	[SerializeField]
	private float minToastingDuration = 2f;

	[SerializeField]
	private float maxToastingDuration = 10f;

	[SerializeField]
	private float leverStopMoveSoundThreshold = 0.1f;

	[SerializeField]
	private float leverStartMoveSoundThreshold = 1f;

	[SerializeField]
	private float disableDuration = 10f;

	[Header("References")]
	[SerializeField]
	private Rigidbody lever;

	[SerializeField]
	private Rigidbody catapultLeft;

	[SerializeField]
	private Rigidbody catapultRight;

	[SerializeField]
	private Rigidbody knob;

	[SerializeField]
	private ParticleSystem disabledParticles;

	[Header("Events")]
	[SerializeField]
	private UnityEvent OnStartToasting;

	[SerializeField]
	private UnityEvent OnFinishToasting;

	[SerializeField]
	private UnityEvent OnFinishPop;

	[SerializeField]
	private UnityEvent OnFinishCooldown;

	[SerializeField]
	private UnityEvent onLeverStopMovingEvent;

	[SerializeField]
	private UnityEvent onLeverStartMovingEvent;

	[SerializeField]
	private UnityEvent onDisabledEvent;

	private float toastingDuration;

	private ToasterState toasterState;

	private float toastingTimer;

	private float popTimer;

	private float cooldownTimer;

	private bool isPluggedIn;

	private Vector3 catapultOffsetLeft;

	private Vector3 catapultOffsetRight;

	private Vector3 bottomLeverPosition;

	private Vector3 topLeverPosition;

	private float maxKnobAngle;

	private bool leverIsMoving;

	private float disableTimer;

	private void Awake()
	{
		ConfigurableJoint component = lever.GetComponent<ConfigurableJoint>();
		bottomLeverPosition = lever.transform.position - lever.transform.up * (component.linearLimit.limit - 0.01f);
		topLeverPosition = lever.transform.position + lever.transform.up * component.linearLimit.limit;
		lever.position = topLeverPosition;
		HingeJoint component2 = knob.GetComponent<HingeJoint>();
		maxKnobAngle = component2.limits.max;
	}

	private void Start()
	{
		toasterState = initialToasterState;
		isPluggedIn = toasterState != ToasterState.Unplugged;
		catapultOffsetLeft = catapultLeft.position - lever.position;
		catapultOffsetRight = catapultRight.position - lever.position;
		disabledParticles.Stop();
	}

	private void Update()
	{
		if (toasterState == ToasterState.Disabled)
		{
			disableTimer -= Time.deltaTime;
			if (disableTimer <= 0f)
			{
				toasterState = (isPluggedIn ? ToasterState.Idle : ToasterState.Unplugged);
				disabledParticles.Stop();
			}
		}
	}

	private void FixedUpdate()
	{
		catapultLeft.MovePosition(lever.position + catapultOffsetLeft);
		catapultRight.MovePosition(lever.position + catapultOffsetRight);
		if (leverIsMoving && lever.linearVelocity.magnitude < leverStopMoveSoundThreshold)
		{
			leverIsMoving = false;
			onLeverStopMovingEvent?.Invoke();
		}
		else if (!leverIsMoving && lever.linearVelocity.magnitude > leverStartMoveSoundThreshold)
		{
			leverIsMoving = true;
			onLeverStartMovingEvent?.Invoke();
		}
		switch (toasterState)
		{
		case ToasterState.Idle:
			if (lever.transform.position.y <= bottomLeverPosition.y)
			{
				float num = knob.transform.localRotation.eulerAngles.y;
				if (num > 180f)
				{
					num -= 360f;
				}
				float num2 = (num / maxKnobAngle + 1f) / 2f;
				toastingDuration = minToastingDuration + (maxToastingDuration - minToastingDuration) * num2;
				lever.isKinematic = true;
				toastingTimer = toastingDuration;
				OnStartToasting?.Invoke();
				toasterState = ToasterState.Toasting;
			}
			break;
		case ToasterState.Toasting:
			toastingTimer -= Time.fixedDeltaTime;
			if (toastingTimer <= 0f)
			{
				lever.DOMove(topLeverPosition, popDuration);
				popTimer = popDuration;
				OnFinishToasting?.Invoke();
				toasterState = ToasterState.Pop;
			}
			break;
		case ToasterState.Pop:
			popTimer -= Time.fixedDeltaTime;
			if (popTimer <= 0f)
			{
				cooldownTimer = cooldownDuration;
				OnFinishPop?.Invoke();
				toasterState = ToasterState.Cooldown;
			}
			break;
		case ToasterState.Cooldown:
			cooldownTimer -= Time.fixedDeltaTime;
			if (cooldownTimer <= 0f)
			{
				lever.isKinematic = false;
				OnFinishCooldown?.Invoke();
				if (disableTimer > 0f)
				{
					toasterState = ToasterState.Disabled;
				}
				else
				{
					toasterState = (isPluggedIn ? ToasterState.Idle : ToasterState.Unplugged);
				}
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ToasterState.Unplugged:
		case ToasterState.Disabled:
			break;
		}
	}

	public void TouchedByWater()
	{
		if (isPluggedIn)
		{
			if (toasterState == ToasterState.Toasting)
			{
				lever.DOMove(topLeverPosition, popDuration);
				popTimer = popDuration;
				toasterState = ToasterState.Pop;
				OnFinishToasting?.Invoke();
			}
			else
			{
				toasterState = ToasterState.Disabled;
				onDisabledEvent?.Invoke();
			}
			disableTimer = disableDuration;
			disabledParticles.Play();
		}
	}

	public void PlugIn()
	{
		isPluggedIn = true;
		toasterState = ToasterState.Idle;
	}

	public void Unplug()
	{
		isPluggedIn = false;
		switch (toasterState)
		{
		case ToasterState.Idle:
			toasterState = ToasterState.Unplugged;
			break;
		case ToasterState.Toasting:
			lever.DOMove(topLeverPosition, popDuration);
			popTimer = popDuration;
			OnFinishToasting?.Invoke();
			toasterState = ToasterState.Pop;
			break;
		case ToasterState.Disabled:
			disabledParticles.Stop();
			toasterState = ToasterState.Unplugged;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ToasterState.Unplugged:
		case ToasterState.Pop:
		case ToasterState.Cooldown:
			break;
		}
	}
}
