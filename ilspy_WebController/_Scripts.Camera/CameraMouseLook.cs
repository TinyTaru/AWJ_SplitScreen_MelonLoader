using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.Singletons;
using _Scripts.UI.MobileMonetization;

namespace _Scripts.Camera;

public class CameraMouseLook : MonoBehaviour
{
	[SerializeField]
	private Transform yawTransform;

	[SerializeField]
	private bool clampX;

	[SerializeField]
	private float minX;

	[SerializeField]
	private float maxX;

	[SerializeField]
	private bool clampY;

	[SerializeField]
	private float minY;

	[SerializeField]
	private float maxY;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference lookInputAction;

	[SerializeField]
	private InputActionReference mobileLookInputAction;

	private const float touchSensitivityFactor = 1.5f;

	private const float mobileJoystickSensitivityFactor = 3f;

	private Vector2 lookInput;

	private Vector2 lookInputMobile;

	private Vector2 mouseLook;

	private Vector2 oldLookInput;

	private Vector2 oldLookInputMobile;

	private void OnEnable()
	{
		lookInputAction.action.performed += OnLook;
	}

	private void OnDisable()
	{
		lookInputAction.action.performed -= OnLook;
	}

	private void Start()
	{
		if (yawTransform == null)
		{
			yawTransform = base.transform.parent;
		}
	}

	private void Update()
	{
		if (Singleton<GameController>.Instance == null)
		{
			return;
		}
		if (Singleton<GameController>.Instance.State == GameController.GameState.Paused || Singleton<GameController>.Instance.State == GameController.GameState.Cutscene || Singleton<GameController>.Instance.State == GameController.GameState.SelectEmote || Singleton<GameController>.Instance.State == GameController.GameState.LevelFinished || Singleton<GameController>.Instance.State == GameController.GameState.Debugging || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			lookInput = Vector2.zero;
			lookInputMobile = Vector2.zero;
		}
		else if (Singleton<CameraController>.Instance.CanMoveCamera)
		{
			mouseLook += lookInput * (Singleton<GameController>.Instance.InputIsKeyboardMouse ? 1f : Time.deltaTime);
			if (clampX)
			{
				mouseLook.x = Mathf.Clamp(mouseLook.x, minX, maxX);
			}
			if (clampY)
			{
				mouseLook.y = Mathf.Clamp(mouseLook.y, minY, maxY);
			}
		}
	}

	private void FixedUpdate()
	{
		base.transform.localRotation = Quaternion.AngleAxis(0f - mouseLook.y, Vector3.right);
		yawTransform.localRotation = Quaternion.AngleAxis(mouseLook.x, Vector3.up);
	}

	public void MobileLook(Vector2 value)
	{
	}

	public void MobileJoystickLook(Vector2 value)
	{
	}

	private void OnLook(InputAction.CallbackContext ctx)
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Debugging || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			lookInput = Vector2.zero;
			return;
		}
		lookInput = ctx.ReadValue<Vector2>();
		if (Mathf.Abs(lookInput.sqrMagnitude - oldLookInput.sqrMagnitude) > float.Epsilon)
		{
			Singleton<MobileControls>.Instance.ShowButtons(value: false);
			oldLookInput = lookInput;
		}
		if (Singleton<GameController>.Instance.InputIsKeyboardMouse)
		{
			Vector2 b = new Vector2(SettingsController.MouseSensitivityX, SettingsController.MouseSensitivityY) * 2f + Vector2.one * 0.1f;
			lookInput = Vector2.Scale(lookInput, b);
			if (SettingsController.MouseInvertXAxis)
			{
				lookInput.x *= -1f;
			}
			if (SettingsController.MouseInvertYAxis)
			{
				lookInput.y *= -1f;
			}
		}
		else
		{
			Vector2 b2 = new Vector2(SettingsController.GamepadSensitivityX, SettingsController.GamepadSensitivityY) * 2f + Vector2.one * 0.1f;
			lookInput = Vector2.Scale(lookInput, b2);
			if (SettingsController.GamepadInvertXAxis)
			{
				lookInput.x *= -1f;
			}
			if (SettingsController.GamepadInvertYAxis)
			{
				lookInput.y *= -1f;
			}
		}
	}
}
