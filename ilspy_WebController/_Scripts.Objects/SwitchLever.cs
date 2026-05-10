using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Objects;

public class SwitchLever : MonoBehaviour
{
	[SerializeField]
	private bool initialState = true;

	[SerializeField]
	private Rigidbody lever;

	[SerializeField]
	private float offThreshold = 0.1f;

	[SerializeField]
	private float onThreshold = 0.9f;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onSwitchOnEvent;

	[SerializeField]
	private UnityEvent onSwitchOffEvent;

	private bool isOn;

	private float maxLeverAngle;

	private void Start()
	{
		HingeJoint component = lever.GetComponent<HingeJoint>();
		maxLeverAngle = component.limits.max;
		if (initialState)
		{
			onSwitchOnEvent?.Invoke();
		}
		else
		{
			onSwitchOffEvent?.Invoke();
		}
		float x = (initialState ? 1f : (-1f)) * maxLeverAngle;
		lever.transform.localRotation = Quaternion.Euler(x, 0f, 0f);
	}

	private void Update()
	{
		float leverValue = GetLeverValue(lever, maxLeverAngle);
		if (leverValue > onThreshold && !isOn)
		{
			isOn = true;
			onSwitchOnEvent?.Invoke();
		}
		else if (leverValue < offThreshold && isOn)
		{
			isOn = false;
			onSwitchOffEvent?.Invoke();
		}
	}

	private float GetLeverValue(Rigidbody lever, float maxAngle)
	{
		float num = lever.transform.localRotation.eulerAngles.x;
		if (num > 180f)
		{
			num -= 360f;
		}
		return (num / maxAngle + 1f) / 2f;
	}
}
