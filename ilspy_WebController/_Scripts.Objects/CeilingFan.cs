using System;
using FMODUnity;
using UnityEngine;

namespace _Scripts.Objects;

public class CeilingFan : MonoBehaviour
{
	[SerializeField]
	private HingeJoint joint;

	[SerializeField]
	private Rigidbody ceilingFanRotate;

	[SerializeField]
	private float maxVelocity = 200f;

	[SerializeField]
	private bool playSound = true;

	[SerializeField]
	private StudioEventEmitter ceilingFanLoopSound;

	[SerializeField]
	private float minVelocityLoopSound = 10f;

	[SerializeField]
	private float maxVelocityLoopSound = 50f;

	private void Start()
	{
		if (joint.useMotor && ceilingFanLoopSound != null && playSound)
		{
			ceilingFanLoopSound.Play();
		}
	}

	private void Update()
	{
		if (joint.useMotor && !(ceilingFanLoopSound == null) && playSound)
		{
			float value = Mathf.Abs(ceilingFanRotate.angularVelocity.y) * 180f / MathF.PI;
			float value2 = Mathf.InverseLerp(minVelocityLoopSound, maxVelocityLoopSound, value);
			ceilingFanLoopSound.SetParameter("fan_speed", value2);
		}
	}

	public void SwitchOn()
	{
		joint.useMotor = true;
		if (ceilingFanLoopSound != null && playSound)
		{
			ceilingFanLoopSound.Play();
		}
	}

	public void SwitchOff()
	{
		joint.useMotor = false;
		if (ceilingFanLoopSound != null)
		{
			ceilingFanLoopSound.Stop();
		}
	}

	public void SetSpeed(float value)
	{
		JointMotor motor = joint.motor;
		motor.targetVelocity = maxVelocity * value;
		joint.motor = motor;
	}
}
