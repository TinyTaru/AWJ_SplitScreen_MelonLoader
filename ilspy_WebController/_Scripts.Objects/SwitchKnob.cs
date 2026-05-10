using System;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Objects;

public class SwitchKnob : MonoBehaviour
{
	[SerializeField]
	private bool initialState;

	[SerializeField]
	private Rigidbody knob;

	[SerializeField]
	private float offThreshold = 0.1f;

	[SerializeField]
	private float onThreshold = 0.2f;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onSwitchOnEvent;

	[SerializeField]
	private UnityEvent onSwitchOffEvent;

	[SerializeField]
	private UnityEvent<float> onValueChangedEvent;

	private bool isOn;

	private float maxKnobAngle;

	private float valueOld;

	private void Awake()
	{
		HingeJoint component = knob.GetComponent<HingeJoint>();
		maxKnobAngle = component.limits.max;
	}

	private void Start()
	{
		if (initialState)
		{
			onSwitchOnEvent?.Invoke();
		}
		else
		{
			onSwitchOffEvent?.Invoke();
		}
		float y = (initialState ? 1f : (-1f)) * maxKnobAngle;
		knob.transform.localRotation = Quaternion.Euler(0f, y, 0f);
	}

	private void Update()
	{
		float leverValue = GetLeverValue(knob, maxKnobAngle);
		if (isOn)
		{
			if (leverValue < offThreshold)
			{
				isOn = false;
				onSwitchOffEvent?.Invoke();
			}
			if (Math.Abs(leverValue - valueOld) > 0.0001f)
			{
				onValueChangedEvent?.Invoke(leverValue);
			}
		}
		else if (leverValue > onThreshold)
		{
			isOn = true;
			onSwitchOnEvent?.Invoke();
			onValueChangedEvent?.Invoke(leverValue);
		}
		valueOld = leverValue;
	}

	private float GetLeverValue(Rigidbody lever, float maxAngle)
	{
		float num = lever.transform.localRotation.eulerAngles.y;
		if (num > 180f)
		{
			num -= 360f;
		}
		return (num / maxAngle + 1f) / 2f;
	}
}
