using UnityEngine;

public class PlayerLookAt : MonoBehaviour
{
	public float speed = 5f;

	private void Update()
	{
		float yAngle = Input.GetAxis("Mouse X") * speed;
		float num = Input.GetAxis("Mouse Y") * speed;
		base.transform.Rotate(0f, yAngle, 0f, Space.World);
		base.transform.Rotate(0f - num, 0f, 0f, Space.Self);
	}
}
