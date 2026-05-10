using UnityEngine;

namespace Cinemachine.Examples;

[AddComponentMenu("")]
public class CharacterMovement2D : MonoBehaviour
{
	public KeyCode sprintJoystick = KeyCode.JoystickButton2;

	public KeyCode jumpJoystick = KeyCode.JoystickButton0;

	public KeyCode sprintKeyboard = KeyCode.LeftShift;

	public KeyCode jumpKeyboard = KeyCode.Space;

	public float jumpVelocity = 7f;

	public float groundTolerance = 0.2f;

	public bool checkGroundForJump = true;

	private float speed;

	private bool isSprinting;

	private Animator anim;

	private Vector2 input;

	private float velocity;

	private bool headingleft;

	private Quaternion targetrot;

	private Rigidbody rigbody;

	private void Start()
	{
		anim = GetComponent<Animator>();
		rigbody = GetComponent<Rigidbody>();
		targetrot = base.transform.rotation;
	}

	private void FixedUpdate()
	{
		input.x = Input.GetAxis("Horizontal");
		if ((input.x < 0f && !headingleft) || (input.x > 0f && headingleft))
		{
			if (input.x < 0f)
			{
				targetrot = Quaternion.Euler(0f, 270f, 0f);
			}
			if (input.x > 0f)
			{
				targetrot = Quaternion.Euler(0f, 90f, 0f);
			}
			headingleft = !headingleft;
		}
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, targetrot, Time.deltaTime * 20f);
		speed = Mathf.Abs(input.x);
		speed = Mathf.SmoothDamp(anim.GetFloat("Speed"), speed, ref velocity, 0.1f);
		anim.SetFloat("Speed", speed);
		if ((Input.GetKeyDown(sprintJoystick) || Input.GetKeyDown(sprintKeyboard)) && input != Vector2.zero)
		{
			isSprinting = true;
		}
		if (Input.GetKeyUp(sprintJoystick) || Input.GetKeyUp(sprintKeyboard) || input == Vector2.zero)
		{
			isSprinting = false;
		}
		anim.SetBool("isSprinting", isSprinting);
	}

	private void Update()
	{
		if (Input.GetKeyDown(jumpJoystick) || Input.GetKeyDown(jumpKeyboard))
		{
			rigbody.AddForce(new Vector3(0f, jumpVelocity, 0f), ForceMode.Impulse);
		}
	}

	public bool isGrounded()
	{
		if (checkGroundForJump)
		{
			return Physics.Raycast(base.transform.position, Vector3.down, groundTolerance);
		}
		return true;
	}
}
