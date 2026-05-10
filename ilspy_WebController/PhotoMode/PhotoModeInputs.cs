using UnityEngine;
using UnityEngine.InputSystem;

namespace PhotoMode;

public class PhotoModeInputs : MonoBehaviour
{
	private InputDevice currentInputDevice;

	[Header("Device Configuration")]
	[SerializeField]
	private InfoBarController infoBarController;

	[Space]
	[Header("Input System Actions - Photo Mode")]
	[SerializeField]
	private InputActionAsset photoModeActionAsset;

	public InputActionReference submitAction;

	public InputActionReference navigationAction;

	public InputActionReference lookAction;

	public InputActionReference pauseAction;

	public InputActionReference resetAction;

	public InputActionReference toggleInterfaceAction;

	public InputActionReference toggleGridAction;

	[Space]
	[Header("Input System Actions - Sticker Mode")]
	public InputActionReference swapStickerAction;

	public InputActionReference deleteStickerAction;

	public InputActionReference flipStickerAction;

	[Space]
	[Header("Input System Actions - Modifiers")]
	public InputActionReference modifier1Action;

	public InputActionReference modifier2Action;

	[HideInInspector]
	public PhotoModeDeviceEvent DeviceChangeEvent;

	[HideInInspector]
	public PhotoModeButtonEvent SubmitEvent;

	[HideInInspector]
	public PhotoModeButtonEvent PauseEvent;

	[HideInInspector]
	public PhotoModeButtonEvent ResetEvent;

	[HideInInspector]
	public PhotoModeButtonEvent ToggleInterfaceEvent;

	[HideInInspector]
	public PhotoModeButtonEvent ToggleGridEvent;

	[HideInInspector]
	public PhotoModeButtonEvent SwapStickerEvent;

	[HideInInspector]
	public PhotoModeButtonEvent DeleteStickerEvent;

	[HideInInspector]
	public PhotoModeButtonEvent FlipStickerEvent;

	[Header("Input Values")]
	public Vector2 moveAxis;

	public Vector2 lookAxis;

	public float modifier1_value;

	public float modifier2_value;

	private void Start()
	{
		photoModeActionAsset.Enable();
		submitAction.action.performed += SubmitAction_performed;
		navigationAction.action.performed += NavigateAction_performed;
		lookAction.action.performed += LookAction_performed;
		resetAction.action.performed += ResetAction_performed;
		pauseAction.action.performed += PauseAction_performed;
		toggleGridAction.action.performed += GridAction_performed;
		toggleInterfaceAction.action.performed += InterfaceAction_performed;
		swapStickerAction.action.performed += SwapStickerAction_performed;
		deleteStickerAction.action.performed += DeleteStickerAction_performed;
		flipStickerAction.action.performed += ExtraAction_performed;
		modifier1Action.action.performed += Modifier1Action_performed;
		modifier2Action.action.performed += Modifier2Action_performed;
		photoModeActionAsset.actionMaps[0].actionTriggered += PhotoModeInputs_actionTriggered;
		photoModeActionAsset.actionMaps[1].actionTriggered += PhotoModeInputs_actionTriggered;
	}

	private void ResetAction_performed(InputAction.CallbackContext callback)
	{
		ResetEvent.Invoke();
	}

	private void DeleteStickerAction_performed(InputAction.CallbackContext callback)
	{
		DeleteStickerEvent.Invoke();
	}

	private void SwapStickerAction_performed(InputAction.CallbackContext callback)
	{
		SwapStickerEvent.Invoke();
	}

	private void InterfaceAction_performed(InputAction.CallbackContext callback)
	{
		ToggleInterfaceEvent.Invoke();
	}

	private void GridAction_performed(InputAction.CallbackContext callback)
	{
		ToggleGridEvent.Invoke();
	}

	private void SubmitAction_performed(InputAction.CallbackContext callback)
	{
		SubmitEvent.Invoke();
	}

	private void NavigateAction_performed(InputAction.CallbackContext callback)
	{
		moveAxis = callback.ReadValue<Vector2>();
	}

	private void LookAction_performed(InputAction.CallbackContext callback)
	{
		lookAxis = callback.ReadValue<Vector2>();
	}

	private void PauseAction_performed(InputAction.CallbackContext callback)
	{
		PauseEvent.Invoke();
	}

	private void Modifier1Action_performed(InputAction.CallbackContext callback)
	{
		modifier1_value = callback.ReadValue<float>();
	}

	private void Modifier2Action_performed(InputAction.CallbackContext callback)
	{
		modifier2_value = callback.ReadValue<float>();
	}

	private void ExtraAction_performed(InputAction.CallbackContext callback)
	{
		FlipStickerEvent.Invoke();
	}

	private void PhotoModeInputs_actionTriggered(InputAction.CallbackContext obj)
	{
		if (currentInputDevice != obj.control.device)
		{
			currentInputDevice = obj.control.device;
			string text = obj.control.device.ToString();
			bool flag = text == "Keyboard:/Keyboard";
			bool flag2 = text == "Mouse:/Mouse";
			infoBarController.SetKeyboardModeActive(flag || flag2);
		}
	}
}
