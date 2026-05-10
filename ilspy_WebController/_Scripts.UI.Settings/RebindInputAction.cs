using System;
using System.Collections.Generic;
using InputIcons;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace _Scripts.UI.Settings;

public class RebindInputAction : MonoBehaviour
{
	private delegate void OnRebindOperationCompleted(RebindInputAction rebindBehaviour);

	[Header("Parameters")]
	[SerializeField]
	private string bindingName;

	[SerializeField]
	private InputActionReference inputAction;

	[Header("References")]
	[SerializeField]
	private TextMeshProUGUI bindingNameText;

	[SerializeField]
	private TextMeshProUGUI bindingIconText;

	[Header("Keybinding Input Action")]
	[SerializeField]
	private InputActionReference editInputAction;

	[SerializeField]
	private InputActionReference resetInputAction;

	[SerializeField]
	private InputIconsUtility.BindingType bindingType;

	private static InputActionRebindingExtensions.RebindingOperation rebindOperation;

	private static OnRebindOperationCompleted onRebindOperationCompleted;

	private static readonly bool AllowKeysBoundToMultipleActions = false;

	private readonly bool canBeRebound = true;

	private static readonly bool SaveInputRebinds = true;

	private TextMeshProUGUI originalText;

	public InputActionReference actionReference
	{
		get
		{
			return inputAction;
		}
		set
		{
			inputAction = value;
			UpdateBindingDisplay();
		}
	}

	public InputIconsUtility.BindingType BindingType
	{
		get
		{
			return bindingType;
		}
		set
		{
			bindingType = value;
			UpdateBindingDisplay();
		}
	}

	private void Awake()
	{
		onRebindOperationCompleted = (OnRebindOperationCompleted)Delegate.Combine(onRebindOperationCompleted, new OnRebindOperationCompleted(HandleAnyRebindOperationCompleted));
		UpdateBehaviour();
	}

	private void OnEnable()
	{
		UpdateBehaviour();
		InputIconsManagerSO.onControlsChanged = (InputIconsManagerSO.OnControlsChanged)Delegate.Combine(InputIconsManagerSO.onControlsChanged, new InputIconsManagerSO.OnControlsChanged(HandleControlsChanged));
		InputIconsManagerSO.onBindingsChanged = (InputIconsManagerSO.OnBindingsChanged)Delegate.Combine(InputIconsManagerSO.onBindingsChanged, new InputIconsManagerSO.OnBindingsChanged(UpdateBehaviour));
	}

	private void OnDisable()
	{
		InputIconsManagerSO.onControlsChanged = (InputIconsManagerSO.OnControlsChanged)Delegate.Remove(InputIconsManagerSO.onControlsChanged, new InputIconsManagerSO.OnControlsChanged(HandleControlsChanged));
		InputIconsManagerSO.onBindingsChanged = (InputIconsManagerSO.OnBindingsChanged)Delegate.Remove(InputIconsManagerSO.onBindingsChanged, new InputIconsManagerSO.OnBindingsChanged(UpdateBehaviour));
	}

	private void OnDestroy()
	{
		onRebindOperationCompleted = (OnRebindOperationCompleted)Delegate.Remove(onRebindOperationCompleted, new OnRebindOperationCompleted(HandleAnyRebindOperationCompleted));
	}

	private void Update()
	{
		if (canBeRebound)
		{
			if (editInputAction.action.WasPerformedThisFrame() && EventSystem.current.currentSelectedGameObject == base.gameObject)
			{
				StartRebindProcess();
			}
			if (resetInputAction.action.WasPerformedThisFrame() && EventSystem.current.currentSelectedGameObject == base.gameObject)
			{
				ResetBinding();
			}
		}
	}

	private void UpdateTexts()
	{
		bindingNameText.text = bindingName;
		bindingIconText.text = "<style=" + inputAction.action.actionMap.name + "/" + inputAction.action.name + ">";
	}

	private void StartRebindProcess()
	{
		if (rebindOperation != null)
		{
			rebindOperation.Cancel();
		}
		bindingIconText.text = DialogueManager.GetLocalizedText("Setting_Waiting for Input");
		inputAction.action.Disable();
		InputDevice device = InputSystem.GetDevice<InputDevice>();
		int index = InputIconsUtility.GetIndexOfBindingType(inputAction.action, BindingType, "Spider");
		rebindOperation = inputAction.action.PerformInteractiveRebinding(index);
		rebindOperation.WithControlsExcluding("<Mouse>/position").WithControlsExcluding("<Mouse>/delta").WithControlsExcluding("<Gamepad>/Start")
			.WithCancelingThrough("<Keyboard>/escape")
			.OnMatchWaitForAnother(0.1f)
			.OnCancel(delegate
			{
				RebindCanceled();
			})
			.OnComplete(delegate
			{
				RebindCompleted(index);
			});
		string activeDeviceString = InputIconsUtility.GetActiveDeviceString();
		if (device is Gamepad)
		{
			rebindOperation.WithControlsExcluding("<Keyboard>");
			rebindOperation.WithControlsExcluding("<Mouse>");
		}
		else
		{
			rebindOperation.WithControlsExcluding("<Gamepad>");
		}
		rebindOperation.WithBindingGroup(activeDeviceString);
		rebindOperation.Start();
	}

	private void RebindCanceled()
	{
		rebindOperation.Dispose();
		rebindOperation = null;
		inputAction.action.Enable();
		bindingIconText.text = "<style=" + inputAction.action.actionMap.name + "/" + inputAction.action.name + ">";
	}

	private void RebindCompleted(int bindingIndex)
	{
		for (int i = 0; i < inputAction.action.bindings.Count; i++)
		{
			inputAction.action.GetBindingDisplayString(i, out var _, out var _);
		}
		if (SaveInputRebinds)
		{
			InputIconsManagerSO.SaveUserBindings();
		}
		rebindOperation.Dispose();
		rebindOperation = null;
		inputAction.action.Enable();
		onRebindOperationCompleted?.Invoke(this);
		InputIconsManagerSO.HandleInputBindingsChanged();
	}

	private void HandleControlsChanged(InputDevice inputDevice)
	{
		UpdateBehaviour();
	}

	private void HandleAnyRebindOperationCompleted(RebindInputAction rebindBehaviour)
	{
		if (AllowKeysBoundToMultipleActions || rebindBehaviour == this || rebindBehaviour.actionReference.action.actionMap != inputAction.action.actionMap || (rebindBehaviour.actionReference.action == inputAction.action && InputIconsUtility.ActionIsComposite(rebindBehaviour.actionReference.action) && rebindBehaviour.BindingType == bindingType))
		{
			return;
		}
		List<InputBinding> bindings = InputIconsUtility.GetBindings(rebindBehaviour.actionReference, rebindBehaviour.BindingType, "Spider");
		List<InputBinding> bindings2 = InputIconsUtility.GetBindings(inputAction, bindingType, "Spider");
		for (int i = 0; i < bindings.Count; i++)
		{
			for (int j = 0; j < bindings2.Count; j++)
			{
				if (bindings[i].effectivePath == bindings2[j].effectivePath && bindings[i].id != bindings2[j].id)
				{
					int indexOfInputBinding = InputIconsUtility.GetIndexOfInputBinding(inputAction.action, bindings2[j]);
					inputAction.action.ApplyBindingOverride(indexOfInputBinding, "");
					InputIconsManagerSO.SaveUserBindings();
				}
			}
		}
	}

	private void ButtonPressedResetBinding()
	{
		ResetBinding();
	}

	private void ResetBinding()
	{
		inputAction.action.RemoveAllBindingOverrides();
		onRebindOperationCompleted?.Invoke(this);
		InputIconsManagerSO.HandleInputBindingsChanged();
		InputIconsManagerSO.SaveUserBindings();
	}

	private void UpdateBindingDisplay()
	{
		string text = inputAction.action.actionMap.name + "/" + inputAction.action.name;
		if (InputIconsUtility.ActionIsComposite(inputAction.action))
		{
			text = text + "/" + BindingType.ToString().ToLower();
		}
		if (!InputIconsManagerSO.GetSpriteStyleTagSingle(text).Contains("name=\"\""))
		{
			bindingIconText.SetText(InputIconsManagerSO.GetSpriteStyleTagSingle(text));
		}
		else
		{
			bindingIconText.text = DialogueManager.GetLocalizedText("Setting_Missing Binding");
		}
	}

	private void UpdateBehaviour()
	{
		UpdateBindingDisplay();
	}

	private void ToggleGameObjectState(GameObject targetGameObject, bool newState)
	{
		targetGameObject.SetActive(newState);
	}
}
