using System;
using UnityEngine;

public class Oscillate : MonoBehaviour
{
	[SerializeField]
	private float amplitude = 1f;

	[SerializeField]
	private float period = 1f;

	private Vector3 startPosition;

	private void Start()
	{
		startPosition = base.transform.position;
	}

	private void FixedUpdate()
	{
		float num = amplitude * Mathf.Sin(MathF.PI * 2f / period * Time.fixedTime);
		base.transform.position = startPosition + Vector3.up * num;
	}
}
