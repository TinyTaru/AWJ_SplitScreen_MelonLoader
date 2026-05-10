using System;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;

namespace _Scripts.Office;

public class PullButton : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private Axis axis;

	[SerializeField]
	private float pressDistance = 0.09f;

	[SerializeField]
	private float releaseDistance = 0.05f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onButtonPressedEvent;

	[SerializeField]
	private UnityEvent onButtonReleasedEvent;

	private Vector3 startPosition;

	private bool isPressed;

	private bool isMainWebAttached;

	private void Awake()
	{
		startPosition = base.transform.localPosition;
	}

	private void FixedUpdate()
	{
		if (!isMainWebAttached)
		{
			float num = 0f;
			num = axis switch
			{
				Axis.X => base.transform.localPosition.x - startPosition.x, 
				Axis.Y => base.transform.localPosition.y - startPosition.y, 
				Axis.Z => base.transform.localPosition.z - startPosition.z, 
				Axis.XYZ => Vector3.Distance(base.transform.localPosition, startPosition), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			if (!isPressed && num > pressDistance)
			{
				isPressed = true;
				onButtonPressedEvent?.Invoke();
			}
			else if (isPressed && num < releaseDistance)
			{
				isPressed = false;
				onButtonReleasedEvent?.Invoke();
			}
		}
	}

	public void MainWebAttached()
	{
		isPressed = true;
		isMainWebAttached = true;
		onButtonPressedEvent?.Invoke();
	}

	public void MainWebReleased()
	{
		isMainWebAttached = false;
		onButtonReleasedEvent?.Invoke();
	}
}
