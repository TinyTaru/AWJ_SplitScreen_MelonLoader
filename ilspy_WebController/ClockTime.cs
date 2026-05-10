using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ClockTime
{
	[SerializeField]
	private bool enabled;

	[SerializeField]
	private int hour;

	[Tooltip("Hour deviation in hours. Floating point numbers possible")]
	[SerializeField]
	private float hourDeviation;

	[SerializeField]
	private int minute;

	[Tooltip("Hour deviation in minutes.")]
	[SerializeField]
	private int minuteDeviation;

	[SerializeField]
	private UnityEvent OnCorrectTime;

	private bool currentTimeActive;

	public void CheckTime(float currentHour, float currentMinute)
	{
		if (!enabled)
		{
			return;
		}
		float num = currentHour - (float)hour;
		float b = 12f - num;
		float num2 = Mathf.Abs(Mathf.Min(num, b));
		float b2 = Mathf.Abs(num2 - 12f);
		float num3 = Mathf.Min(num2, b2);
		float num4 = currentMinute - (float)minute;
		float b3 = 60f - num4;
		float num5 = Mathf.Abs(Mathf.Min(num4, b3));
		float b4 = Mathf.Abs(num5 - 60f);
		float num6 = Mathf.Min(num5, b4);
		if (num3 < hourDeviation && num6 < (float)minuteDeviation)
		{
			if (!currentTimeActive)
			{
				currentTimeActive = true;
				OnCorrectTime?.Invoke();
				Debug.Log($"The current time is {hour}:{minute}!");
			}
		}
		else
		{
			currentTimeActive = false;
		}
	}
}
