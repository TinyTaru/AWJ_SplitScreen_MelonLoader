using System;
using UnityEngine;

public class PlayerMovePhysics : MonoBehaviour
{
	public float speed = 5f;

	public bool worldDirection = true;

	public bool rotatePlayer = true;

	public Action spaceAction;

	public Action enterAction;

	private Rigidbody rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void OnEnable()
	{
		base.transform.position += new Vector3(10f, 0f, 0f);
	}

	private void FixedUpdate()
	{
		Vector3 vector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		if (vector.magnitude > 0f)
		{
			Vector3 vector2 = (worldDirection ? Vector3.forward : (base.transform.position - Camera.main.transform.position));
			vector2.y = 0f;
			vector2 = vector2.normalized;
			if (vector2.magnitude > 0.001f)
			{
				vector = Quaternion.LookRotation(vector2, Vector3.up) * vector;
				if (vector.magnitude > 0.001f)
				{
					rb.AddForce(speed * vector);
					if (rotatePlayer)
					{
						base.transform.rotation = Quaternion.LookRotation(vector.normalized, Vector3.up);
					}
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Space) && spaceAction != null)
		{
			spaceAction();
		}
		if (Input.GetKeyDown(KeyCode.Return) && enterAction != null)
		{
			enterAction();
		}
	}
}
