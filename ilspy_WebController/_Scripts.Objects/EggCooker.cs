using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace _Scripts.Objects;

public class EggCooker : MonoBehaviour
{
	private enum EggCookerState
	{
		Unplugged,
		Off,
		On,
		Cooldown
	}

	[SerializeField]
	private EggCookerState initialEggCookerState;

	[SerializeField]
	private float offThreshold = 0.1f;

	[SerializeField]
	private float onThreshold = 0.9f;

	[SerializeField]
	private float cookingDuration = 10f;

	[SerializeField]
	private float cooldownDuration = 1f;

	[SerializeField]
	private MagneticLock[] magneticLocks;

	[Header("References")]
	[SerializeField]
	private Rigidbody knob;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onEggCookerUnpluggedEvent;

	[SerializeField]
	private UnityEvent onEggCookerOffEvent;

	[SerializeField]
	private UnityEvent onEggCookerOnEvent;

	[FormerlySerializedAs("onEggCookerFinshedEvent")]
	[SerializeField]
	private UnityEvent onEggCookerFinishedEvent;

	[SerializeField]
	private UnityEvent onEggCookerCooldownFinishedEvent;

	private bool isPluggedIn;

	private EggCookerState eggCookerState;

	private float maxKnobAngle;

	private float cookingTimer;

	private float cooldownTimer;

	private void Awake()
	{
		HingeJoint component = knob.GetComponent<HingeJoint>();
		maxKnobAngle = component.limits.max;
	}

	private void Start()
	{
		eggCookerState = initialEggCookerState;
		isPluggedIn = eggCookerState != EggCookerState.Unplugged;
		SetMagneticLocksActive(isPluggedIn);
		knob.transform.localRotation = Quaternion.Euler(0f, 0f - maxKnobAngle, 0f);
	}

	private void Update()
	{
		float knobValue = GetKnobValue(knob, maxKnobAngle);
		switch (eggCookerState)
		{
		case EggCookerState.Off:
			if (knobValue > onThreshold)
			{
				Debug.Log("Egg Cooker turned on");
				cookingTimer = cookingDuration;
				eggCookerState = EggCookerState.On;
				onEggCookerOnEvent?.Invoke();
			}
			break;
		case EggCookerState.On:
			if (knobValue < offThreshold)
			{
				Debug.Log("Egg Cooker turned off");
				eggCookerState = EggCookerState.Off;
				onEggCookerOffEvent?.Invoke();
			}
			cookingTimer -= Time.deltaTime;
			if (cookingTimer <= 0f)
			{
				Debug.Log("Egg Cooker finished cooking");
				cooldownTimer = cooldownDuration;
				knob.isKinematic = true;
				knob.transform.DOLocalRotate(new Vector3(0f, 0f - maxKnobAngle, 0f), cooldownDuration * 0.5f);
				eggCookerState = EggCookerState.Cooldown;
				onEggCookerFinishedEvent?.Invoke();
			}
			break;
		case EggCookerState.Cooldown:
			cooldownTimer -= Time.deltaTime;
			if (cooldownTimer <= 0f)
			{
				Debug.Log("Egg Cooker cooldown finished");
				knob.isKinematic = false;
				eggCookerState = (isPluggedIn ? EggCookerState.Off : EggCookerState.Unplugged);
				onEggCookerCooldownFinishedEvent?.Invoke();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case EggCookerState.Unplugged:
			break;
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

	public void PlugIn()
	{
		Debug.Log("Egg cooker plugged in");
		isPluggedIn = true;
		eggCookerState = EggCookerState.Off;
		onEggCookerOffEvent?.Invoke();
	}

	public void Unplug()
	{
		Debug.Log("Egg cooker unplugged");
		isPluggedIn = false;
		switch (eggCookerState)
		{
		case EggCookerState.Off:
			eggCookerState = EggCookerState.Unplugged;
			break;
		case EggCookerState.On:
			eggCookerState = EggCookerState.Unplugged;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case EggCookerState.Unplugged:
		case EggCookerState.Cooldown:
			break;
		}
		onEggCookerUnpluggedEvent?.Invoke();
	}

	public void SetMagneticLocksActive(bool value)
	{
		MagneticLock[] array = magneticLocks;
		foreach (MagneticLock magneticLock in array)
		{
			if (value)
			{
				magneticLock.EnableMagneticLock();
			}
			else
			{
				magneticLock.DisableMagneticLock();
			}
		}
	}
}
