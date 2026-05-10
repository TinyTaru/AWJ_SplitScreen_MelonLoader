using UnityEngine;

namespace Cinemachine.Examples;

[AddComponentMenu("")]
public class CharacterMovement : MonoBehaviour
{
	public bool useCharacterForward;

	public bool lockToCameraForward;

	public float turnSpeed = 10f;

	public KeyCode sprintJoystick = KeyCode.JoystickButton2;

	public KeyCode sprintKeyboard = KeyCode.Space;

	private float turnSpeedMultiplier;

	private float speed;

	private float direction;

	private bool isSprinting;

	private Animator anim;

	private Vector3 targetDirection;

	private Vector2 input;

	private Quaternion freeRotation;

	private Camera mainCamera;

	private float velocity;

	private void Start()
	{
		anim = GetComponent<Animator>();
		mainCamera = Camera.main;
	}

	private void FixedUpdate()
	{
		input.x = Input.GetAxis("Horizontal");
		input.y = Input.GetAxis("Vertical");
		if (useCharacterForward)
		{
			speed = Mathf.Abs(input.x) + input.y;
		}
		else
		{
			speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
		}
		speed = Mathf.Clamp(speed, 0f, 1f);
		speed = Mathf.SmoothDamp(anim.GetFloat("Speed"), speed, ref velocity, 0.1f);
		anim.SetFloat("Speed", speed);
		if (input.y < 0f && useCharacterForward)
		{
			direction = input.y;
		}
		else
		{
			direction = 0f;
		}
		anim.SetFloat("Direction", direction);
		isSprinting = (Input.GetKey(sprintJoystick) || Input.GetKey(sprintKeyboard)) && input != Vector2.zero && direction >= 0f;
		anim.SetBool("isSprinting", isSprinting);
		UpdateTargetDirection();
		if (input != Vector2.zero && targetDirection.magnitude > 0.1f)
		{
			Vector3 normalized = targetDirection.normalized;
			freeRotation = Quaternion.LookRotation(normalized, base.transform.up);
			float num = freeRotation.eulerAngles.y - base.transform.eulerAngles.y;
			float y = base.transform.eulerAngles.y;
			if (num < 0f || num > 0f)
			{
				y = freeRotation.eulerAngles.y;
			}
			Vector3 euler = new Vector3(0f, y, 0f);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.Euler(euler), turnSpeed * turnSpeedMultiplier * Time.deltaTime);
		}
	}

	public virtual void UpdateTargetDirection()
	{
		if (!useCharacterForward)
		{
			turnSpeedMultiplier = 1f;
			Vector3 vector = mainCamera.transform.TransformDirection(Vector3.forward);
			vector.y = 0f;
			Vector3 vector2 = mainCamera.transform.TransformDirection(Vector3.right);
			targetDirection = input.x * vector2 + input.y * vector;
		}
		else
		{
			turnSpeedMultiplier = 0.2f;
			Vector3 vector3 = base.transform.TransformDirection(Vector3.forward);
			vector3.y = 0f;
			Vector3 vector4 = base.transform.TransformDirection(Vector3.right);
			targetDirection = input.x * vector4 + Mathf.Abs(input.y) * vector3;
		}
	}
}
