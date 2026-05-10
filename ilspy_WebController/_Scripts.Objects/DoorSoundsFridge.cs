using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;

namespace _Scripts.Objects;

[RequireComponent(typeof(MovableObject))]
public class DoorSoundsFridge : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private Axis axis = Axis.Y;

	[SerializeField]
	private float maxShutVelocity = 1f;

	[SerializeField]
	private string shutParameterName = "volume";

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter doorOpenSound;

	[SerializeField]
	private StudioEventEmitter doorShutSound;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onDoorOpenEvent;

	[SerializeField]
	private UnityEvent onDoorShutEvent;

	private float shutAngle;

	private bool isShut;

	private Quaternion oldRotation;

	private Vector3 angularVelocity;

	private void Awake()
	{
		Quaternion localRotation = base.transform.localRotation;
		switch (axis)
		{
		case Axis.X:
			shutAngle = localRotation.eulerAngles.x;
			break;
		case Axis.Y:
			shutAngle = localRotation.eulerAngles.y;
			break;
		case Axis.Z:
			shutAngle = localRotation.eulerAngles.z;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case Axis.XYZ:
			break;
		}
	}

	private void Start()
	{
		isShut = true;
	}

	private void Update()
	{
		Quaternion localRotation = base.transform.localRotation;
		angularVelocity = (localRotation.eulerAngles - oldRotation.eulerAngles) / Time.deltaTime;
		float num = 0f;
		float num2 = 0f;
		switch (axis)
		{
		case Axis.X:
			num = Mathf.Abs(localRotation.eulerAngles.x - shutAngle);
			num2 = Mathf.Abs(angularVelocity.x);
			break;
		case Axis.Y:
			num = Mathf.Abs(localRotation.eulerAngles.y - shutAngle);
			num2 = Mathf.Abs(angularVelocity.y);
			break;
		case Axis.Z:
			num = Mathf.Abs(localRotation.eulerAngles.z - shutAngle);
			num2 = Mathf.Abs(angularVelocity.z);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case Axis.XYZ:
			break;
		}
		if (isShut && num > 2f)
		{
			isShut = false;
			if (doorOpenSound != null)
			{
				doorOpenSound.Play();
			}
			onDoorOpenEvent?.Invoke();
		}
		else if (!isShut && num < 1f)
		{
			isShut = true;
			if (doorShutSound != null)
			{
				doorShutSound.Play();
				doorShutSound.SetParameter(shutParameterName, num2 / maxShutVelocity);
			}
			onDoorShutEvent?.Invoke();
		}
		oldRotation = localRotation;
	}
}
