using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.Singletons;

namespace _Scripts.Camera;

public class FreeLookCamera : MonoBehaviour
{
	[SerializeField]
	private float startSpeed;

	[SerializeField]
	private float verticalSpeedPercentage;

	[SerializeField]
	private float speedIncrementPercentage;

	[SerializeField]
	private Transform lookTarget;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference moveInputAction;

	[SerializeField]
	private InputActionReference upInputAction;

	[SerializeField]
	private InputActionReference downInputAction;

	[SerializeField]
	private InputActionReference speedInputAction;

	private float speed;

	private Vector2 moveInput;

	private float upGamepadInput;

	private float downGamepadInput;

	private float upKeyboardInput;

	private float downKeyboardInput;

	private void OnEnable()
	{
		moveInputAction.action.performed += OnMove;
		upInputAction.action.performed += OnUp;
		downInputAction.action.performed += OnDown;
		speedInputAction.action.performed += OnSpeed;
	}

	private void OnDisable()
	{
		moveInputAction.action.performed -= OnMove;
		upInputAction.action.performed -= OnUp;
		downInputAction.action.performed -= OnDown;
		speedInputAction.action.performed -= OnSpeed;
	}

	private void Start()
	{
		speed = startSpeed;
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.FreeLook)
		{
			float num = upGamepadInput - downGamepadInput;
			Vector3 vector = lookTarget.right * moveInput.x + lookTarget.up * num * verticalSpeedPercentage + lookTarget.forward * moveInput.y;
			base.transform.position += vector * speed * Time.deltaTime;
		}
	}

	private void OnMove(InputAction.CallbackContext ctx)
	{
		moveInput = ctx.ReadValue<Vector2>();
	}

	private void OnUp(InputAction.CallbackContext ctx)
	{
		upGamepadInput = ctx.ReadValue<float>();
	}

	private void OnDown(InputAction.CallbackContext ctx)
	{
		downGamepadInput = ctx.ReadValue<float>();
	}

	private void OnSpeed(InputAction.CallbackContext ctx)
	{
		if (Singleton<GameController>.Instance.InputIsKeyboardMouse)
		{
			speed *= 1f + Mathf.Sign(ctx.ReadValue<Vector2>().y) * speedIncrementPercentage;
		}
	}
}
