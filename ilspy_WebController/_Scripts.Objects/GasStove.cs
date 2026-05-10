using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Objects;

public class GasStove : MonoBehaviour
{
	[SerializeField]
	private float areaOffThreshold = 0.1f;

	[SerializeField]
	private float areaOnThreshold = 0.9f;

	[Header("Initial Values")]
	[SerializeField]
	private float initialValue;

	[Header("References")]
	[SerializeField]
	private Rigidbody[] knobs;

	[Header("Events Area 0")]
	[SerializeField]
	private UnityEvent onArea0TurnedOnEvent;

	[SerializeField]
	private UnityEvent onArea0TurnedOffEvent;

	[Header("Events Area 1")]
	[SerializeField]
	private UnityEvent onArea1TurnedOnEvent;

	[SerializeField]
	private UnityEvent onArea1TurnedOffEvent;

	[Header("Events Area 2")]
	[SerializeField]
	private UnityEvent onArea2TurnedOnEvent;

	[SerializeField]
	private UnityEvent onArea2TurnedOffEvent;

	[Header("Events Area 3")]
	[SerializeField]
	private UnityEvent onArea3TurnedOnEvent;

	[SerializeField]
	private UnityEvent onArea3TurnedOffEvent;

	private bool[] areaIsOn;

	private float maxKnobAngle;

	private UnityEvent[] onAreaTurnedOnEvents;

	private UnityEvent[] onAreaTurnedOffEvents;

	private void Awake()
	{
		HingeJoint component = knobs[0].GetComponent<HingeJoint>();
		maxKnobAngle = component.limits.max;
		onAreaTurnedOnEvents = new UnityEvent[4] { onArea0TurnedOnEvent, onArea1TurnedOnEvent, onArea2TurnedOnEvent, onArea3TurnedOnEvent };
		onAreaTurnedOffEvents = new UnityEvent[4] { onArea0TurnedOffEvent, onArea1TurnedOffEvent, onArea2TurnedOffEvent, onArea3TurnedOffEvent };
	}

	private void Start()
	{
		areaIsOn = new bool[4];
		float y = initialValue * 2f - 1f * maxKnobAngle;
		for (int i = 0; i < knobs.Length; i++)
		{
			knobs[i].transform.localRotation = Quaternion.Euler(0f, y, 0f);
		}
	}

	private void Update()
	{
		for (int i = 0; i < knobs.Length; i++)
		{
			float knobValue = GetKnobValue(knobs[i], maxKnobAngle);
			if (knobValue > areaOnThreshold && !areaIsOn[i])
			{
				areaIsOn[i] = true;
				onAreaTurnedOnEvents[i]?.Invoke();
			}
			else if (knobValue < areaOffThreshold && areaIsOn[i])
			{
				areaIsOn[i] = false;
				onAreaTurnedOffEvents[i]?.Invoke();
			}
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
