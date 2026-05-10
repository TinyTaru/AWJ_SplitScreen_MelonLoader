using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Office;

public class OfficeDeskLamp : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Rigidbody button;

	[SerializeField]
	private GameObject lightRay;

	[Header("Parameters")]
	[SerializeField]
	private bool lampIsOnAtStart;

	[SerializeField]
	private float onThreshold;

	[SerializeField]
	private float offThreshold;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onLampSwitchedOn;

	[SerializeField]
	private UnityEvent onLampSwitchedOff;

	private bool lampIsOn;

	private void Start()
	{
		lampIsOn = lampIsOnAtStart;
		lightRay.SetActive(lampIsOn);
		float max = button.GetComponent<HingeJoint>().limits.max;
		if (lampIsOn)
		{
			button.transform.localRotation = Quaternion.Euler(max, 0f, 0f);
		}
		else
		{
			button.transform.localRotation = Quaternion.Euler(0f - max, 0f, 0f);
		}
	}

	private void Update()
	{
		float num = button.transform.localRotation.eulerAngles.x;
		if (num > 180f)
		{
			num -= 360f;
		}
		if (lampIsOn)
		{
			if (num < offThreshold)
			{
				SwitchLampOff();
			}
		}
		else if (num > onThreshold)
		{
			SwitchLampOn();
		}
	}

	private void SwitchLampOn()
	{
		lampIsOn = true;
		onLampSwitchedOn?.Invoke();
	}

	private void SwitchLampOff()
	{
		lampIsOn = false;
		onLampSwitchedOff?.Invoke();
	}
}
