using UnityEngine;

public class CharacterMovementNoCamera : MonoBehaviour
{
	public Transform InvisibleCameraOrigin;

	public float StrafeSpeed = 0.1f;

	public float TurnSpeed = 3f;

	public float Damping = 0.2f;

	public float VerticalRotMin = -80f;

	public float VerticalRotMax = 80f;

	public KeyCode sprintJoystick = KeyCode.JoystickButton2;

	public KeyCode sprintKeyboard = KeyCode.Space;

	private bool isSprinting;

	private Animator anim;

	private float currentStrafeSpeed;

	private Vector2 currentVelocity;

	private void OnEnable()
	{
		anim = GetComponent<Animator>();
		currentVelocity = Vector2.zero;
		currentStrafeSpeed = 0f;
		isSprinting = false;
	}

	private void FixedUpdate()
	{
		Vector2 vector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		float y = vector.y;
		y = Mathf.Clamp(y, -1f, 1f);
		y = Mathf.SmoothDamp(anim.GetFloat("Speed"), y, ref currentVelocity.y, Damping);
		anim.SetFloat("Speed", y);
		anim.SetFloat("Direction", y);
		isSprinting = (Input.GetKey(sprintJoystick) || Input.GetKey(sprintKeyboard)) && y > 0f;
		anim.SetBool("isSprinting", isSprinting);
		currentStrafeSpeed = Mathf.SmoothDamp(currentStrafeSpeed, vector.x * StrafeSpeed, ref currentVelocity.x, Damping);
		base.transform.position += base.transform.TransformDirection(Vector3.right) * currentStrafeSpeed;
		Vector2 vector2 = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		Vector3 eulerAngles = base.transform.eulerAngles;
		eulerAngles.y += vector2.x * TurnSpeed;
		base.transform.rotation = Quaternion.Euler(eulerAngles);
		if (InvisibleCameraOrigin != null)
		{
			eulerAngles = InvisibleCameraOrigin.localRotation.eulerAngles;
			eulerAngles.x -= vector2.y * TurnSpeed;
			if (eulerAngles.x > 180f)
			{
				eulerAngles.x -= 360f;
			}
			eulerAngles.x = Mathf.Clamp(eulerAngles.x, VerticalRotMin, VerticalRotMax);
			InvisibleCameraOrigin.localRotation = Quaternion.Euler(eulerAngles);
		}
	}
}
