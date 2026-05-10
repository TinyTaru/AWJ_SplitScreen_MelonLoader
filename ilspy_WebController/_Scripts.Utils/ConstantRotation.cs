using System;
using UnityEngine;
using _Scripts.General;

namespace _Scripts.Utils;

public class ConstantRotation : MonoBehaviour
{
	private enum RotationType
	{
		Global,
		Local
	}

	[SerializeField]
	private bool activeAtStart = true;

	[SerializeField]
	private bool randomStartingRotation;

	[SerializeField]
	private float speed;

	[SerializeField]
	private Axis axis;

	[SerializeField]
	private RotationType rotationType;

	[SerializeField]
	private AnimatorUpdateMode updateMode = AnimatorUpdateMode.Fixed;

	private bool isActive;

	private void Awake()
	{
		SetActive(activeAtStart);
	}

	private void Start()
	{
		if (!randomStartingRotation)
		{
			return;
		}
		float angle = UnityEngine.Random.Range(0f, 360f);
		switch (rotationType)
		{
		case RotationType.Global:
			switch (axis)
			{
			case Axis.X:
				base.transform.RotateAround(base.transform.position, Vector3.right, angle);
				break;
			case Axis.Y:
				base.transform.RotateAround(base.transform.position, Vector3.up, angle);
				break;
			case Axis.Z:
				base.transform.RotateAround(base.transform.position, Vector3.forward, angle);
				break;
			}
			break;
		case RotationType.Local:
			switch (axis)
			{
			case Axis.X:
				base.transform.RotateAround(base.transform.position, base.transform.right, angle);
				break;
			case Axis.Y:
				base.transform.RotateAround(base.transform.position, base.transform.up, angle);
				break;
			case Axis.Z:
				base.transform.RotateAround(base.transform.position, base.transform.forward, angle);
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void Update()
	{
		if (!isActive || updateMode == AnimatorUpdateMode.Fixed)
		{
			return;
		}
		float num = ((updateMode == AnimatorUpdateMode.Normal) ? Time.deltaTime : Time.unscaledDeltaTime);
		switch (rotationType)
		{
		case RotationType.Global:
			switch (axis)
			{
			case Axis.X:
				base.transform.RotateAround(base.transform.position, Vector3.right, speed * num);
				break;
			case Axis.Y:
				base.transform.RotateAround(base.transform.position, Vector3.up, speed * num);
				break;
			case Axis.Z:
				base.transform.RotateAround(base.transform.position, Vector3.forward, speed * num);
				break;
			}
			break;
		case RotationType.Local:
			switch (axis)
			{
			case Axis.X:
				base.transform.RotateAround(base.transform.position, base.transform.right, speed * num);
				break;
			case Axis.Y:
				base.transform.RotateAround(base.transform.position, base.transform.up, speed * num);
				break;
			case Axis.Z:
				base.transform.RotateAround(base.transform.position, base.transform.forward, speed * num);
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void FixedUpdate()
	{
		if (!isActive || updateMode != AnimatorUpdateMode.Fixed)
		{
			return;
		}
		switch (rotationType)
		{
		case RotationType.Global:
			switch (axis)
			{
			case Axis.X:
				base.transform.RotateAround(base.transform.position, Vector3.right, speed * Time.fixedDeltaTime);
				break;
			case Axis.Y:
				base.transform.RotateAround(base.transform.position, Vector3.up, speed * Time.fixedDeltaTime);
				break;
			case Axis.Z:
				base.transform.RotateAround(base.transform.position, Vector3.forward, speed * Time.fixedDeltaTime);
				break;
			}
			break;
		case RotationType.Local:
			switch (axis)
			{
			case Axis.X:
				base.transform.RotateAround(base.transform.position, base.transform.right, speed * Time.fixedDeltaTime);
				break;
			case Axis.Y:
				base.transform.RotateAround(base.transform.position, base.transform.up, speed * Time.fixedDeltaTime);
				break;
			case Axis.Z:
				base.transform.RotateAround(base.transform.position, base.transform.forward, speed * Time.fixedDeltaTime);
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SetActive(bool value)
	{
		isActive = value;
	}

	public void SetSpeed(float newSpeed)
	{
		speed = newSpeed;
	}
}
