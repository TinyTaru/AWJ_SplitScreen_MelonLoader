using UnityEngine;

namespace _Scripts.Debugging;

public class GooglyBody : MonoBehaviour
{
	[SerializeField]
	private Transform body;

	[Range(0.5f, 10f)]
	[SerializeField]
	private float speed = 1f;

	[Range(0f, 5f)]
	[SerializeField]
	private float gravityMultiplier = 1f;

	[Range(0.01f, 0.98f)]
	[SerializeField]
	private float bounciness = 0.4f;

	private Vector3 _origin;

	private Vector3 _velocity;

	private Vector3 _lastPosition;

	private void Start()
	{
		_origin = body.localPosition;
		_lastPosition = base.transform.position;
	}

	private void Update()
	{
		Vector3 position = base.transform.position;
		Vector3 vector = base.transform.InverseTransformDirection(Physics.gravity);
		_velocity += vector * gravityMultiplier * Time.deltaTime;
		_velocity += base.transform.InverseTransformVector(_origin - position) / Time.deltaTime;
		_velocity.x = 0f;
		_velocity.z = 0f;
		Vector3 localPosition = body.localPosition;
		localPosition += _velocity * speed * Time.deltaTime;
		if (localPosition.y > 0.25f)
		{
			Vector3 up = base.transform.up;
			_velocity = Vector2.Reflect(_velocity, up) * bounciness;
			localPosition = new Vector3(0f, 0.25f, 0f);
		}
		localPosition.x = body.localPosition.x;
		localPosition.z = body.localPosition.z;
		body.localPosition = localPosition;
		_lastPosition = base.transform.position;
	}
}
