using System;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;

namespace _Scripts.Objects;

public class Radio : MonoBehaviour, IAffectedByWater
{
	private enum RadioState
	{
		Unplugged,
		Off,
		On,
		Disabled
	}

	[SerializeField]
	private RadioState initialRadioState;

	[SerializeField]
	private float radioOnThreshold = 0.1f;

	[SerializeField]
	private float[] gameMusicFrequencies;

	[SerializeField]
	private float toleranceGameMusic = 0.01f;

	[SerializeField]
	private float gameMusicPlayDuration = 1f;

	[SerializeField]
	private float disableDuration = 10f;

	[Header("Initial Values")]
	[Range(0f, 1f)]
	[SerializeField]
	private float initialVolume;

	[Range(0f, 1f)]
	[SerializeField]
	private float initialFrequency;

	[Header("References")]
	[SerializeField]
	private Rigidbody volumeKnob;

	[SerializeField]
	private Rigidbody frequencyKnob;

	[SerializeField]
	private ParticleSystem disabledParticles;

	[Header("Events")]
	[SerializeField]
	private UnityEvent OnRadioSwitchedOnEvent;

	[SerializeField]
	private UnityEvent OnRadioSwitchedOffEvent;

	[SerializeField]
	private UnityEvent OnGameMusicPlayedEvent;

	[SerializeField]
	private UnityEvent onPluginEvent;

	[SerializeField]
	private UnityEvent onUnplugEvent;

	[SerializeField]
	private UnityEvent onDisabledEvent;

	private RadioState radioState;

	private bool isPluggedIn;

	private float maxVolumeKnobAngle;

	private float maxFrequencyKnobAngle;

	private bool gameMusicPlayed;

	private float gameMusicTimer;

	private float disableTimer;

	private void Awake()
	{
		HingeJoint component = volumeKnob.GetComponent<HingeJoint>();
		maxVolumeKnobAngle = component.limits.max;
		HingeJoint component2 = frequencyKnob.GetComponent<HingeJoint>();
		maxFrequencyKnobAngle = component2.limits.max;
		gameMusicTimer = gameMusicPlayDuration;
	}

	private void Start()
	{
		radioState = initialRadioState;
		isPluggedIn = initialRadioState != RadioState.Unplugged;
		disabledParticles.Stop();
		gameMusicPlayed = false;
		Singleton<MusicController>.Instance.SetRadioVolume(initialVolume);
		Singleton<MusicController>.Instance.SetRadioFrequency(initialFrequency);
		Singleton<MusicController>.Instance.StartRadioMusic();
		float y = (initialVolume * 2f - 1f) * maxVolumeKnobAngle;
		volumeKnob.transform.localRotation = Quaternion.Euler(0f, y, 0f);
		float y2 = (initialFrequency * 2f - 1f) * maxFrequencyKnobAngle;
		frequencyKnob.transform.localRotation = Quaternion.Euler(0f, y2, 0f);
	}

	private void OnDestroy()
	{
		Singleton<MusicController>.Instance.StopRadioMusic();
	}

	private void Update()
	{
		float num = GetKnobValue(volumeKnob, maxVolumeKnobAngle);
		float knobValue = GetKnobValue(frequencyKnob, maxFrequencyKnobAngle);
		Singleton<MusicController>.Instance.SetRadioFrequency(knobValue);
		switch (radioState)
		{
		case RadioState.Off:
			if (num > radioOnThreshold)
			{
				radioState = RadioState.On;
				Singleton<MusicController>.Instance.StopMusicTrack(Singleton<GameController>.Instance.Music);
				OnRadioSwitchedOnEvent?.Invoke();
			}
			break;
		case RadioState.On:
			if (!gameMusicPlayed)
			{
				float[] array = gameMusicFrequencies;
				foreach (float num2 in array)
				{
					if (Mathf.Abs(knobValue - num2) < toleranceGameMusic)
					{
						gameMusicTimer -= Time.deltaTime;
						if (gameMusicTimer <= 0f)
						{
							gameMusicPlayed = true;
							OnGameMusicPlayedEvent?.Invoke();
						}
					}
				}
			}
			if (num < radioOnThreshold)
			{
				radioState = RadioState.Off;
				Singleton<MusicController>.Instance.StartMusic(Singleton<GameController>.Instance.Music);
				OnRadioSwitchedOffEvent?.Invoke();
			}
			break;
		case RadioState.Disabled:
			disableTimer -= Time.deltaTime;
			if (disableTimer <= 0f)
			{
				if (!isPluggedIn)
				{
					radioState = RadioState.Unplugged;
				}
				else if (num > radioOnThreshold)
				{
					Singleton<MusicController>.Instance.StopMusicTrack(Singleton<GameController>.Instance.Music);
					radioState = RadioState.On;
				}
				else
				{
					radioState = RadioState.Off;
				}
				disabledParticles.Stop();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case RadioState.Unplugged:
			break;
		}
		if (radioState == RadioState.Disabled || radioState == RadioState.Unplugged)
		{
			num = 0f;
		}
		Singleton<MusicController>.Instance.SetRadioVolume(num);
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

	public void TouchedByWater()
	{
		if (isPluggedIn)
		{
			Singleton<MusicController>.Instance.StartMusic(Singleton<GameController>.Instance.Music);
			radioState = RadioState.Disabled;
			disableTimer = disableDuration;
			disabledParticles.Play();
			onDisabledEvent?.Invoke();
		}
	}

	public void PlugIn()
	{
		isPluggedIn = true;
		float knobValue = GetKnobValue(volumeKnob, maxVolumeKnobAngle);
		onPluginEvent?.Invoke();
		if (knobValue > radioOnThreshold)
		{
			radioState = RadioState.On;
			Singleton<MusicController>.Instance.StopMusicTrack(Singleton<GameController>.Instance.Music);
			OnRadioSwitchedOnEvent?.Invoke();
		}
		else
		{
			radioState = RadioState.Off;
		}
	}

	public void UnPlug()
	{
		isPluggedIn = false;
		Singleton<MusicController>.Instance.StartMusic(Singleton<GameController>.Instance.Music);
		onUnplugEvent?.Invoke();
		switch (radioState)
		{
		case RadioState.Off:
			radioState = RadioState.Unplugged;
			break;
		case RadioState.On:
			radioState = RadioState.Unplugged;
			break;
		case RadioState.Disabled:
			disabledParticles.Stop();
			radioState = RadioState.Unplugged;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case RadioState.Unplugged:
			break;
		}
	}
}
