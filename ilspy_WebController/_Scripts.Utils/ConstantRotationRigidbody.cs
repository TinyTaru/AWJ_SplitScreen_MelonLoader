using System;
using UnityEngine;
using _Scripts.General;

namespace _Scripts.Utils;

public class ConstantRotationRigidbody : MonoBehaviour
{
	private enum RotationType
	{
		Global,
		Local
	}

	[SerializeField]
	private float speed;

	[SerializeField]
	private Axis axis;

	[SerializeField]
	private RotationType rotationType;

	private Rigidbody rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		switch (rotationType)
		{
		case RotationType.Global:
			switch (axis)
			{
			case Axis.X:
				rb.angularVelocity = Vector3.right * speed * (MathF.PI / 180f);
				break;
			case Axis.Y:
				rb.angularVelocity = Vector3.up * speed * (MathF.PI / 180f);
				break;
			case Axis.Z:
				rb.angularVelocity = Vector3.forward * speed * (MathF.PI / 180f);
				break;
			}
			break;
		case RotationType.Local:
			switch (axis)
			{
			case Axis.X:
				rb.angularVelocity = base.transform.right * speed * (MathF.PI / 180f);
				break;
			case Axis.Y:
				rb.angularVelocity = base.transform.up * speed * (MathF.PI / 180f);
				break;
			case Axis.Z:
				rb.angularVelocity = base.transform.forward * speed * (MathF.PI / 180f);
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
