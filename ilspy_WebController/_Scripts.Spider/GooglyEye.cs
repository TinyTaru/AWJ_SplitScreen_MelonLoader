using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Spider;

public class GooglyEye : MonoBehaviour
{
	[FormerlySerializedAs("Eye")]
	[SerializeField]
	private Transform eye;

	[Range(0f, 1f)]
	[SerializeField]
	private float maxDistance = 0.25f;

	[FormerlySerializedAs("Speed")]
	[Range(0.5f, 10f)]
	[SerializeField]
	private float speed = 1f;

	[FormerlySerializedAs("GravityMultiplier")]
	[Range(0f, 5f)]
	[SerializeField]
	private float gravityMultiplier = 0.5f;

	[FormerlySerializedAs("Bounciness")]
	[Range(0.01f, 0.98f)]
	[SerializeField]
	private float bounciness = 0.5f;

	private Vector3 origin;

	private Vector3 velocity;

	private Vector3 lastPosition;

	private void Start()
	{
		origin = eye.localPosition;
		lastPosition = base.transform.position;
	}

	private void Update()
	{
		if (Time.timeScale < 0.1f)
		{
			UpdateEyePosition(Time.unscaledDeltaTime);
		}
	}

	private void FixedUpdate()
	{
		UpdateEyePosition(Time.fixedDeltaTime);
	}

	private void UpdateEyePosition(float deltaTime)
	{
		Vector3 position = base.transform.position;
		Vector3 vector = base.transform.InverseTransformDirection(Physics.gravity);
		velocity += vector * gravityMultiplier * deltaTime;
		velocity += base.transform.InverseTransformVector(lastPosition - position) * 500f * deltaTime;
		velocity.z = 0f;
		Vector3 localPosition = eye.localPosition;
		localPosition += velocity * speed * deltaTime;
		Vector2 vector2 = new Vector2(localPosition.x, localPosition.y);
		float f = Mathf.Atan2(vector2.y, vector2.x);
		if (vector2.magnitude > maxDistance)
		{
			Vector2 inNormal = -vector2.normalized;
			velocity = Vector2.Reflect(new Vector2(velocity.x, velocity.y), inNormal) * bounciness;
			localPosition = new Vector3(Mathf.Cos(f), Mathf.Sin(f), 0f) * maxDistance;
		}
		localPosition.z = origin.z;
		eye.localPosition = localPosition;
		lastPosition = base.transform.position;
	}
}
