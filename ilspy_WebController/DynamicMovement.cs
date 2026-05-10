using UnityEngine;
using UnityEngine.InputSystem;

public class DynamicMovement : MonoBehaviour
{
	[SerializeField]
	private float movementSpeed;

	[SerializeField]
	private float rotationSpeed;

	[SerializeField]
	private float jumpForce;

	[SerializeField]
	private float levitatingForce;

	[SerializeField]
	private float suctionForce;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference moveInputAction;

	[SerializeField]
	private InputActionReference jumpInputAction;

	private Rigidbody rb;

	private Vector2 moveInput;

	private bool jumpInput;

	private Vector3 contactNormal;

	private void OnEnable()
	{
		moveInputAction.action.performed += OnMove;
		jumpInputAction.action.performed += OnJump;
	}

	private void OnDisable()
	{
		moveInputAction.action.performed -= OnMove;
		jumpInputAction.action.performed -= OnJump;
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		Vector3 linearVelocity = rb.linearVelocity;
		_ = rb.rotation;
		if (jumpInput)
		{
			jumpInput = false;
			rb.AddForce(contactNormal * jumpForce);
			contactNormal = Vector3.zero;
		}
		Vector3 up;
		Vector3 vector;
		if (contactNormal.sqrMagnitude > 0f)
		{
			rb.AddForce(-Physics.gravity * rb.mass, ForceMode.Force);
			rb.AddForce(-contactNormal * suctionForce, ForceMode.Force);
			up = contactNormal;
			vector = Vector3.Cross(base.transform.right, contactNormal);
			Debug.DrawRay(base.transform.position, vector * 5f, Color.blue);
			linearVelocity = vector * moveInput.y * movementSpeed;
		}
		else
		{
			up = Vector3.up;
			vector = Vector3.Cross(base.transform.right, Vector3.up);
			Debug.DrawRay(base.transform.position, vector * 5f, Color.blue);
			linearVelocity = Vector3.up * linearVelocity.y + vector * moveInput.y * movementSpeed;
		}
		linearVelocity = Vector3.ClampMagnitude(linearVelocity, movementSpeed);
		rb.linearVelocity = linearVelocity;
		Vector3 vector2 = Quaternion.AngleAxis(moveInput.x * rotationSpeed * Time.fixedDeltaTime, up) * vector;
		base.transform.LookAt(base.transform.position + vector2, up);
	}

	private void OnCollisionStay(Collision other)
	{
		contactNormal = other.contacts[0].normal;
		Rigidbody component = other.gameObject.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.AddForceAtPosition(Physics.gravity * rb.mass, rb.position, ForceMode.Force);
		}
	}

	private void OnCollisionEnter(Collision other)
	{
	}

	private void OnCollisionExit(Collision other)
	{
		contactNormal = Vector3.zero;
	}

	private void OnMove(InputAction.CallbackContext ctx)
	{
		moveInput = ctx.ReadValue<Vector2>();
	}

	private void OnJump(InputAction.CallbackContext ctx)
	{
		jumpInput = ctx.ReadValueAsButton();
	}
}
