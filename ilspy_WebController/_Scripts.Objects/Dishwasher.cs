using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Objects;

public class Dishwasher : MonoBehaviour
{
	private enum DishwasherState
	{
		Off,
		SpeedingUp,
		Cleaning,
		SlowingDown,
		Cooldown
	}

	[SerializeField]
	private float offThreshold = 0.1f;

	[SerializeField]
	private float onThreshold = 0.9f;

	[SerializeField]
	private float speedUpDuration = 1f;

	[SerializeField]
	private float cleaningDuration = 10f;

	[SerializeField]
	private float slowDownDuration = 1f;

	[SerializeField]
	private float cooldownDuration = 2f;

	[Header("References")]
	[SerializeField]
	private Rigidbody knob;

	[SerializeField]
	private HingeJoint[] sprayArmHingeJoints;

	[SerializeField]
	private ParticleSystem[] sprayArmParticles;

	[SerializeField]
	private Collider cleaningArea;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onDishwasherOffEvent;

	[SerializeField]
	private UnityEvent onDishwasherOnEvent;

	[SerializeField]
	private UnityEvent onSpeedingUpStartedEvent;

	[SerializeField]
	private UnityEvent onCleaningStartedEvent;

	[SerializeField]
	private UnityEvent onCleaningFinishedEvent;

	[SerializeField]
	private UnityEvent onCooldownFinishedEvent;

	private bool isOn;

	private DishwasherState dishwasherState;

	private float maxKnobAngle;

	private float speedUpTimer;

	private float cleaningTimer;

	private float slowDownTimer;

	private float cooldownTimer;

	private void Awake()
	{
		HingeJoint component = knob.GetComponent<HingeJoint>();
		maxKnobAngle = component.limits.max;
	}

	private void Start()
	{
		dishwasherState = DishwasherState.Off;
		isOn = false;
		cleaningArea.enabled = false;
		SetSprayArmsActive(value: false);
		knob.transform.localRotation = Quaternion.Euler(0f, 0f - maxKnobAngle, 0f);
	}

	private void Update()
	{
		float knobValue = GetKnobValue(knob, maxKnobAngle);
		switch (dishwasherState)
		{
		case DishwasherState.Off:
			if (knobValue > onThreshold)
			{
				isOn = true;
				SetSprayArmsActive(value: true);
				speedUpTimer = speedUpDuration;
				dishwasherState = DishwasherState.SpeedingUp;
				onDishwasherOnEvent?.Invoke();
			}
			break;
		case DishwasherState.SpeedingUp:
			if (knobValue <= offThreshold)
			{
				isOn = false;
				SetSprayArmsActive(value: false);
				slowDownTimer = slowDownDuration;
				dishwasherState = DishwasherState.SlowingDown;
				onDishwasherOffEvent?.Invoke();
			}
			speedUpTimer -= Time.deltaTime;
			if (speedUpTimer <= 0f)
			{
				SetSprayArmParticlesActive(value: true);
				cleaningTimer = cleaningDuration;
				cleaningArea.enabled = true;
				dishwasherState = DishwasherState.Cleaning;
				onCleaningStartedEvent?.Invoke();
			}
			break;
		case DishwasherState.Cleaning:
			if (knobValue <= offThreshold)
			{
				isOn = false;
				SetSprayArmsActive(value: false);
				SetSprayArmParticlesActive(value: false);
				slowDownTimer = slowDownDuration;
				dishwasherState = DishwasherState.SlowingDown;
				onDishwasherOffEvent?.Invoke();
			}
			cleaningTimer -= Time.deltaTime;
			if (cleaningTimer <= 0f)
			{
				SetSprayArmParticlesActive(value: false);
				SetSprayArmsActive(value: false);
				slowDownTimer = slowDownDuration;
				cleaningArea.enabled = false;
				dishwasherState = DishwasherState.SlowingDown;
				onCleaningFinishedEvent?.Invoke();
			}
			break;
		case DishwasherState.SlowingDown:
			slowDownTimer -= Time.deltaTime;
			if (slowDownTimer <= 0f)
			{
				knob.isKinematic = true;
				knob.transform.DOLocalRotate(new Vector3(0f, 0f - maxKnobAngle, 0f), 0.5f);
				cooldownTimer = cooldownDuration;
				dishwasherState = DishwasherState.Cooldown;
			}
			break;
		case DishwasherState.Cooldown:
			if (isOn && knobValue <= offThreshold)
			{
				isOn = false;
				onDishwasherOffEvent?.Invoke();
			}
			cooldownTimer -= Time.deltaTime;
			if (cooldownTimer <= 0f)
			{
				knob.isKinematic = false;
				dishwasherState = DishwasherState.Off;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private float GetKnobValue(Rigidbody knob, float maxAngle)
	{
		float num = knob.transform.localRotation.eulerAngles.y;
		if (num > 180f)
		{
			num -= 360f;
		}
		return (num / maxAngle + 1f) / 2f;
	}

	private void SetSprayArmsActive(bool value)
	{
		HingeJoint[] array = sprayArmHingeJoints;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].useMotor = value;
		}
	}

	private void SetSprayArmParticlesActive(bool value)
	{
		ParticleSystem[] array = sprayArmParticles;
		foreach (ParticleSystem particleSystem in array)
		{
			if (value)
			{
				particleSystem.Play();
			}
			else
			{
				particleSystem.Stop();
			}
		}
	}
}
