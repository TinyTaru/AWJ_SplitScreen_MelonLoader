using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Objects;

public class Microwave : MonoBehaviour, IAffectedByWater
{
	private enum MicrowaveState
	{
		Unplugged,
		Off,
		On,
		Cooldown,
		Disabled
	}

	[SerializeField]
	private MicrowaveState initialMicrowaveState;

	[SerializeField]
	private float cookingDuration = 10f;

	[SerializeField]
	private float cooldownDuration = 1f;

	[SerializeField]
	private float doorClosedThreshold;

	[SerializeField]
	private float doorOpenThreshold;

	[SerializeField]
	private float loopVolumeDoorOpen;

	[SerializeField]
	private float loopVolumeDoorClosed;

	[SerializeField]
	private float disableDuration = 10f;

	[Header("References")]
	[SerializeField]
	private Transform door;

	[SerializeField]
	private ParticleSystem disabledParticles;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter microwaveLoopSound;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onMicrowaveUnpluggedEvent;

	[SerializeField]
	private UnityEvent onMicrowaveOffEvent;

	[SerializeField]
	private UnityEvent onMicrowaveOnEvent;

	[SerializeField]
	private UnityEvent onMicrowaveFinshedEvent;

	[SerializeField]
	private UnityEvent onMicrowaveCooldownFinishedEvent;

	[SerializeField]
	private UnityEvent onDisabledEvent;

	private bool isPluggedIn;

	private MicrowaveState microwaveState;

	private float cookingTimer;

	private float cooldownTimer;

	private bool doorClosed;

	private float disableTimer;

	private bool buttonIsPressed;

	private void Start()
	{
		microwaveState = initialMicrowaveState;
		isPluggedIn = microwaveState != MicrowaveState.Unplugged;
		doorClosed = true;
		microwaveLoopSound.SetParameter("volume", loopVolumeDoorClosed);
		disabledParticles.Stop();
	}

	private void Update()
	{
		float y = door.localRotation.eulerAngles.y;
		if (doorClosed && y > doorOpenThreshold)
		{
			doorClosed = false;
			microwaveLoopSound.SetParameter("volume", loopVolumeDoorOpen);
		}
		else if (!doorClosed && y < doorClosedThreshold)
		{
			doorClosed = true;
			microwaveLoopSound.SetParameter("volume", loopVolumeDoorClosed);
		}
		switch (microwaveState)
		{
		case MicrowaveState.Off:
			if (buttonIsPressed)
			{
				microwaveState = MicrowaveState.On;
				cookingTimer = cookingDuration;
				microwaveLoopSound.Play();
				microwaveLoopSound.SetParameter("volume", doorClosed ? loopVolumeDoorClosed : loopVolumeDoorOpen);
				onMicrowaveOnEvent?.Invoke();
			}
			break;
		case MicrowaveState.On:
			cookingTimer -= Time.deltaTime;
			if (cookingTimer <= 0f)
			{
				microwaveState = MicrowaveState.Cooldown;
				cooldownTimer = cooldownDuration;
				microwaveLoopSound.Stop();
				onMicrowaveFinshedEvent?.Invoke();
			}
			break;
		case MicrowaveState.Cooldown:
			cooldownTimer -= Time.deltaTime;
			if (cooldownTimer <= 0f)
			{
				microwaveState = MicrowaveState.Off;
				microwaveLoopSound.Stop();
				onMicrowaveCooldownFinishedEvent?.Invoke();
			}
			break;
		case MicrowaveState.Disabled:
			disableTimer -= Time.deltaTime;
			if (disableTimer <= 0f)
			{
				microwaveState = (isPluggedIn ? MicrowaveState.Off : MicrowaveState.Unplugged);
				disabledParticles.Stop();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MicrowaveState.Unplugged:
			break;
		}
	}

	public void PressButton()
	{
		buttonIsPressed = true;
	}

	public void ReleaseButton()
	{
		buttonIsPressed = false;
	}

	public void TouchedByWater()
	{
		if (isPluggedIn)
		{
			microwaveLoopSound.Stop();
			microwaveState = MicrowaveState.Disabled;
			disableTimer = disableDuration;
			onDisabledEvent?.Invoke();
			disabledParticles.Play();
		}
	}

	public void PlugIn()
	{
		isPluggedIn = true;
		microwaveState = MicrowaveState.Off;
	}

	public void Unplug()
	{
		isPluggedIn = false;
		switch (microwaveState)
		{
		case MicrowaveState.Off:
			microwaveState = MicrowaveState.Unplugged;
			break;
		case MicrowaveState.On:
			microwaveState = MicrowaveState.Unplugged;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MicrowaveState.Unplugged:
		case MicrowaveState.Cooldown:
		case MicrowaveState.Disabled:
			break;
		}
		microwaveLoopSound.Stop();
		onMicrowaveUnpluggedEvent?.Invoke();
	}
}
