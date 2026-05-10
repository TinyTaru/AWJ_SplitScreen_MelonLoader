using Cinemachine.Utility;
using UnityEngine;

public class PlayerMoveOnSphere : MonoBehaviour
{
	public SphereCollider Sphere;

	public float speed = 5f;

	public bool rotatePlayer = true;

	public float rotationDamping = 0.5f;

	private void Update()
	{
		Vector3 vector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		if (vector.magnitude > 0f)
		{
			vector = Camera.main.transform.rotation * vector;
			if (vector.magnitude > 0.001f)
			{
				base.transform.position += vector * (speed * Time.deltaTime);
				if (rotatePlayer)
				{
					float t = Damper.Damp(1f, rotationDamping, Time.deltaTime);
					Quaternion b = Quaternion.LookRotation(vector.normalized, base.transform.up);
					base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, t);
				}
			}
		}
		if (Sphere != null)
		{
			Vector3 normalized = (base.transform.position - Sphere.transform.position).normalized;
			Vector3 forward = base.transform.forward.ProjectOntoPlane(normalized);
			base.transform.position = Sphere.transform.position + normalized * (Sphere.radius + base.transform.localScale.y / 2f);
			base.transform.rotation = Quaternion.LookRotation(forward, normalized);
		}
	}
}
