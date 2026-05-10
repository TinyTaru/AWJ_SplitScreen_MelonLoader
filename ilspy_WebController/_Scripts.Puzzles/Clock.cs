using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace _Scripts.Puzzles;

public class Clock : MonoBehaviour
{
	[Range(0f, 11f)]
	[SerializeField]
	private int startHour;

	[Range(0f, 59f)]
	[SerializeField]
	private int startMinute;

	[SerializeField]
	private bool debug;

	[SerializeField]
	private Transform hourHand;

	[SerializeField]
	private Transform minuteHand;

	[SerializeField]
	private List<ClockTime> clockTimeList;

	[SerializeField]
	private StudioEventEmitter ambientLoopSound;

	private void Start()
	{
		hourHand.localRotation = Quaternion.Euler(0f, (float)startHour * 30f, 0f);
		minuteHand.localRotation = Quaternion.Euler(0f, (float)startMinute * 6f, 0f);
	}

	private void Update()
	{
		float num = hourHand.localRotation.eulerAngles.y / 30f;
		float num2 = minuteHand.localRotation.eulerAngles.y / 6f;
		if (debug)
		{
			Debug.Log($"{num} : {num2}");
		}
		foreach (ClockTime clockTime in clockTimeList)
		{
			clockTime.CheckTime(num, num2);
		}
		if (ambientLoopSound != null)
		{
			ambientLoopSound.SetParameter("ambience_time", num / 10f);
		}
	}
}
