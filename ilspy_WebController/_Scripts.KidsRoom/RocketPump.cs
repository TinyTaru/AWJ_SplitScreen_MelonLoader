using UnityEngine;
using UnityEngine.Events;
using _Scripts.Objects;

namespace _Scripts.KidsRoom;

[RequireComponent(typeof(MovableObject))]
public class RocketPump : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private float cooldownDuration = 2f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent<float> onCollisionsRegistersEvent;

	private bool canRegisterCollision;

	private float cooldownTimer;

	private void Start()
	{
		canRegisterCollision = true;
	}

	private void Update()
	{
		if (!canRegisterCollision)
		{
			cooldownTimer -= Time.deltaTime;
			if (cooldownTimer <= 0f)
			{
				canRegisterCollision = true;
			}
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (canRegisterCollision)
		{
			float magnitude = other.relativeVelocity.magnitude;
			float magnitude2 = other.impulse.magnitude;
			float num = magnitude * magnitude2;
			Debug.Log(num);
			onCollisionsRegistersEvent?.Invoke(num);
			canRegisterCollision = false;
			cooldownTimer = cooldownDuration;
		}
	}
}
