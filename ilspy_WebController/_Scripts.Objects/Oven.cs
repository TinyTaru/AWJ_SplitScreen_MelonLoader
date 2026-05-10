using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace _Scripts.Objects;

public class Oven : MonoBehaviour
{
	private enum OvenState
	{
		Off,
		Heating,
		Max,
		CoolingDown
	}

	[SerializeField]
	private float ovenOffThreshold = 0.1f;

	[SerializeField]
	private float ovenOnThreshold = 0.9f;

	[SerializeField]
	private float heatingSpeed = 0.2f;

	[SerializeField]
	private float coolingSpeed = 0.1f;

	[SerializeField]
	private float maxTemperatureIndicatorAngle = 135f;

	[SerializeField]
	private float maxOvenLoopVolume = 0.5f;

	[SerializeField]
	private int heatingMaterialIndex = 2;

	[Header("References")]
	[SerializeField]
	private Rigidbody knob;

	[FormerlySerializedAs("temperaturIndicator")]
	[SerializeField]
	private Transform temperatureIndicator;

	[SerializeField]
	private MeshRenderer oven;

	[SerializeField]
	private Transform door;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter ovenLoopSound;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onOvenOffEvent;

	[SerializeField]
	private UnityEvent onOvenOnEvent;

	[SerializeField]
	private UnityEvent onOvenMaxEvent;

	[SerializeField]
	private UnityEvent onOvenCoolingDownFinishedEvent;

	[SerializeField]
	private UnityEvent onOvenOpenEvent;

	[SerializeField]
	private UnityEvent onOvenCloseEvent;

	private OvenState ovenState;

	private float maxKnobAngle;

	private float temperature;

	private bool isOpen;

	private static MaterialPropertyBlock mpb;

	private static readonly int temperatureId = Shader.PropertyToID("_Temperature");

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		HingeJoint component = knob.GetComponent<HingeJoint>();
		maxKnobAngle = component.limits.max;
		temperature = 0f;
		SetTemperatureViaMpb();
	}

	private void Start()
	{
		ovenState = OvenState.Off;
		temperature = 0f;
		isOpen = false;
		knob.transform.localRotation = Quaternion.Euler(0f, 0f - maxKnobAngle, 0f);
		temperatureIndicator.localRotation = Quaternion.Euler(0f, 0f - maxTemperatureIndicatorAngle, 0f);
	}

	private void Update()
	{
		float knobValue = GetKnobValue(knob, maxKnobAngle);
		float y = 0f - maxTemperatureIndicatorAngle + 2f * maxTemperatureIndicatorAngle * temperature;
		temperatureIndicator.localRotation = Quaternion.Euler(0f, y, 0f);
		if (!isOpen && door.localRotation.eulerAngles.x > 2f)
		{
			isOpen = true;
			onOvenOpenEvent?.Invoke();
		}
		else if (isOpen && door.localRotation.eulerAngles.x < 1f)
		{
			isOpen = false;
			onOvenCloseEvent?.Invoke();
		}
		switch (ovenState)
		{
		case OvenState.Off:
			if (knobValue > ovenOnThreshold)
			{
				ovenState = OvenState.Heating;
				ovenLoopSound.Play();
				SetOvenLoopSoundVolume(0f);
				onOvenOnEvent?.Invoke();
			}
			break;
		case OvenState.Heating:
			temperature += heatingSpeed * Time.deltaTime;
			SetOvenLoopSoundVolume(temperature * maxOvenLoopVolume);
			SetTemperatureViaMpb();
			if (temperature >= 1f)
			{
				ovenState = OvenState.Max;
				onOvenMaxEvent?.Invoke();
			}
			if (knobValue < ovenOffThreshold)
			{
				ovenState = OvenState.CoolingDown;
				onOvenOffEvent?.Invoke();
			}
			break;
		case OvenState.Max:
			if (knobValue < ovenOffThreshold)
			{
				ovenState = OvenState.CoolingDown;
				onOvenOffEvent?.Invoke();
			}
			break;
		case OvenState.CoolingDown:
			temperature -= coolingSpeed * Time.deltaTime;
			SetOvenLoopSoundVolume(temperature * maxOvenLoopVolume);
			SetTemperatureViaMpb();
			if (temperature <= 0f)
			{
				ovenState = OvenState.Off;
				ovenLoopSound.Stop();
				onOvenCoolingDownFinishedEvent?.Invoke();
			}
			if (knobValue > ovenOnThreshold)
			{
				ovenState = OvenState.Heating;
				onOvenOnEvent?.Invoke();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void SetOvenLoopSoundVolume(float value)
	{
		ovenLoopSound.SetParameter("volume", value);
	}

	private void SetTemperatureViaMpb()
	{
		if (!(oven == null))
		{
			oven.GetPropertyBlock(mpb, heatingMaterialIndex);
			mpb.SetFloat(temperatureId, temperature);
			oven.SetPropertyBlock(mpb, heatingMaterialIndex);
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
}
