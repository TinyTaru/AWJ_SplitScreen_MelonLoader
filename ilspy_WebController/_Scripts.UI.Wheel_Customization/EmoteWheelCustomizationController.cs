using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.CosmeticItems;
using _Scripts.Singletons;
using _Scripts.UI.Menus;

namespace _Scripts.UI.Wheel_Customization;

public class EmoteWheelCustomizationController : MonoBehaviour
{
	[Header("Wheel Slots")]
	[SerializeField]
	private EmoteWheelSlot[] emoteWheelSlots;

	[SerializeField]
	private UnlockedColorSelector unlockedColorSelector;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference goBackInput;

	[Header("Pause Menu")]
	[SerializeField]
	private PauseMenu pauseMenu;

	private CosmeticItemWebSo selectedCosmeticItemWebSo;

	private int selectedSlotIndex;

	private bool isEditing;

	private CosmeticItemSo cosmeticItemSo;

	private UnlockedColorButton lastSelectedColorButton;

	public static event Action<Color> OnColorSelected;

	public static event Action OnEditModeEntered;

	public static event Action<int> OnSlotSelected;

	public static event Action OnEditModeExited;

	private void Update()
	{
		if (goBackInput.action.WasPerformedThisFrame() && isEditing)
		{
			ExitEditMode();
		}
	}

	private void OnEnable()
	{
		SetWheelSlotsInteractable(interactable: false);
		List<CosmeticItemWebSo> unlockedItems = Singleton<CosmeticItemsController>.Instance.GetUnlockedItems<CosmeticItemWebSo>();
		unlockedColorSelector.InitializeUnlockedColors(unlockedItems);
		for (int i = 0; i < emoteWheelSlots.Length; i++)
		{
			emoteWheelSlots[i].SetSlotIndex(i);
		}
		UnlockedColorButton.OnColorButtonPressed += EnterEditMode;
		EmoteWheelSlot.OnSlotClicked += AssignColor;
		isEditing = false;
		SetUnlockedButtonsInteractable(interactable: true);
		AssignCurrentColorsToWheelSlots();
		SetWheelSlotsInteractable(interactable: false);
	}

	private void OnDisable()
	{
		UnlockedColorButton.OnColorButtonPressed -= EnterEditMode;
		EmoteWheelSlot.OnSlotClicked -= AssignColor;
		if (isEditing)
		{
			isEditing = false;
			EmoteWheelCustomizationController.OnEditModeExited?.Invoke();
		}
		SetUnlockedButtonsInteractable(interactable: true);
		SetWheelSlotsInteractable(interactable: false);
		if (selectedSlotIndex >= 0 && selectedSlotIndex < emoteWheelSlots.Length)
		{
			emoteWheelSlots[selectedSlotIndex].Deselect();
		}
		selectedSlotIndex = 0;
	}

	private void SetUnlockedButtonsInteractable(bool interactable)
	{
		foreach (UnlockedColorButton allButton in unlockedColorSelector.GetAllButtons())
		{
			allButton.SetInteractable(interactable);
		}
	}

	private void SetWheelSlotsInteractable(bool interactable)
	{
		EmoteWheelSlot[] array = emoteWheelSlots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetInteractable(interactable);
		}
	}

	private void EnterEditMode(UnlockedColorButton selectedButton, CosmeticItemWebSo cosmeticItemWebSo)
	{
		if (!isEditing)
		{
			selectedCosmeticItemWebSo = cosmeticItemWebSo;
			isEditing = true;
			if ((bool)pauseMenu)
			{
				pauseMenu.SetEmoteEditMode(val: true);
			}
			lastSelectedColorButton = selectedButton;
			SetUnlockedButtonsInteractable(interactable: false);
			lastSelectedColorButton.SetInteractable(interactable: true);
			SetWheelSlotsInteractable(interactable: true);
			EmoteWheelCustomizationController.OnEditModeEntered?.Invoke();
			SelectSlot(2);
		}
	}

	private void SelectSlot(int index)
	{
		selectedSlotIndex = index;
		EmoteWheelCustomizationController.OnSlotSelected?.Invoke(selectedSlotIndex);
		emoteWheelSlots[selectedSlotIndex].Select();
	}

	private void AssignCurrentColorsToWheelSlots()
	{
		CosmeticItemWebSo[] currentWebColors = Singleton<EmoteController>.Instance.GetCurrentWebColors();
		for (int i = 0; i < currentWebColors.Length; i++)
		{
			emoteWheelSlots[i].SetColor(currentWebColors[i]);
		}
	}

	private void ExitEditMode()
	{
		isEditing = false;
		if ((bool)pauseMenu)
		{
			pauseMenu.SetEmoteEditMode(val: false);
		}
		EmoteWheelCustomizationController.OnEditModeExited?.Invoke();
		SetUnlockedButtonsInteractable(interactable: true);
		SetWheelSlotsInteractable(interactable: false);
		if (lastSelectedColorButton != null)
		{
			lastSelectedColorButton.Select();
		}
	}

	private void AssignColor(int slotIndex)
	{
		if (isEditing)
		{
			emoteWheelSlots[slotIndex].SetColor(selectedCosmeticItemWebSo);
			Singleton<EmoteController>.Instance.SetWebColorToSlot(selectedCosmeticItemWebSo, slotIndex);
			ExitEditMode();
		}
	}
}
