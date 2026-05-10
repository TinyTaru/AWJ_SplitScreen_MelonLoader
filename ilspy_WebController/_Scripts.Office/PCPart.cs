using System;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Office;

public class PCPart : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Transform wiggleTransform;

	[Header("Parameters")]
	[SerializeField]
	private float wiggleAmplitude = 0.1f;

	[SerializeField]
	private float wiggleFrequency = 5f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onStartWiggleEvent;

	[SerializeField]
	private UnityEvent onStopWiggleEvent;

	private bool isWiggling;

	private void Start()
	{
		isWiggling = false;
	}

	private void FixedUpdate()
	{
		if (isWiggling)
		{
			float z = wiggleAmplitude * Mathf.Sin(MathF.PI * 2f * wiggleFrequency * Time.fixedTime);
			wiggleTransform.localPosition = new Vector3(0f, 0f, z);
		}
	}

	public void StartWiggle()
	{
		if (!isWiggling)
		{
			isWiggling = true;
			onStartWiggleEvent?.Invoke();
		}
	}

	public void StopWiggle()
	{
		if (isWiggling)
		{
			isWiggling = false;
			onStopWiggleEvent?.Invoke();
		}
	}
}
