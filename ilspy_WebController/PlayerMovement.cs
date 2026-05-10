using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float movementSpeed = 10f;

	public float lookatspeed = 5f;

	private void Update()
	{
		if (Input.GetKey("w"))
		{
			base.transform.position += base.transform.TransformDirection(Vector3.forward) * Time.deltaTime * movementSpeed;
		}
		else if (Input.GetKey("s"))
		{
			base.transform.position -= base.transform.TransformDirection(Vector3.forward) * Time.deltaTime * movementSpeed;
		}
		if (Input.GetKey("a") && !Input.GetKey("d"))
		{
			base.transform.position += base.transform.TransformDirection(Vector3.left) * Time.deltaTime * movementSpeed;
		}
		else if (Input.GetKey("d") && !Input.GetKey("a"))
		{
			base.transform.position -= base.transform.TransformDirection(Vector3.left) * Time.deltaTime * movementSpeed;
		}
		float yAngle = Input.GetAxis("Mouse X") * lookatspeed;
		Input.GetAxis("Mouse Y");
		_ = lookatspeed;
		base.transform.Rotate(0f, yAngle, 0f, Space.World);
	}
}
