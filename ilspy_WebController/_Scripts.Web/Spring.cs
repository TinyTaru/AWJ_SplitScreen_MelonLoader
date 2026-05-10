using UnityEngine;

namespace _Scripts.Web;

public class Spring
{
	private float strength;

	private float damper;

	private float target;

	private float velocity;

	private float value;

	public float Value => value;

	public void Reset()
	{
		velocity = 0f;
		value = 0f;
	}

	public void Update(float deltaTime)
	{
		float num = ((target - value >= 0f) ? 1f : (-1f));
		float num2 = Mathf.Abs(target - value) * strength;
		velocity += (num2 * num - velocity * damper) * deltaTime;
		value += velocity * deltaTime;
	}

	public void SetValue(float newValue)
	{
		value = newValue;
	}

	public void SetTarget(float newTarget)
	{
		target = newTarget;
	}

	public void SetDamper(float newDamper)
	{
		damper = newDamper;
	}

	public void SetStrength(float newStrength)
	{
		strength = newStrength;
	}

	public void SetVelocity(float newVelocity)
	{
		velocity = newVelocity;
	}
}
