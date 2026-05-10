using System;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.LevelSaving;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.UI.Save_Slots;

public class SaveSlot : MonoBehaviour
{
	[SerializeField]
	private SaveSlotButton saveSlotButton;

	[SerializeField]
	private Button renameButton;

	[SerializeField]
	private Button deleteButton;

	private LevelSaveData levelSaveData;

	public LevelSaveData SaveData => levelSaveData;

	public SaveSlotButton SaveSlotBtn => saveSlotButton;

	public static event Action<SaveSlot> OnSaveSlotClicked;

	public static event Action<SaveSlot> OnSaveSlotDelete;

	public static event Action<SaveSlot> OnSaveSlotRename;

	public static event Action<SaveSlot> OnSaveSlotSelected;

	public void Setup(LevelData levelData, LevelSaveData newLevelSaveData)
	{
		levelSaveData = newLevelSaveData;
		saveSlotButton.Setup(levelData, newLevelSaveData);
	}

	public void SetupNavigation(SaveSlot saveSlotAbove, SaveSlot saveSlotBelow, Button exitButton, Button plusButton)
	{
		Navigation navigation = saveSlotButton.Btn.navigation;
		navigation.selectOnUp = ((saveSlotAbove == null) ? exitButton : saveSlotAbove.saveSlotButton.Btn);
		navigation.selectOnDown = ((saveSlotBelow == null) ? plusButton : saveSlotBelow.saveSlotButton.Btn);
		saveSlotButton.Btn.navigation = navigation;
		Navigation navigation2 = renameButton.navigation;
		if (saveSlotAbove != null)
		{
			navigation2.selectOnUp = saveSlotAbove.renameButton;
		}
		if (saveSlotBelow != null)
		{
			navigation2.selectOnDown = saveSlotBelow.renameButton;
		}
		renameButton.navigation = navigation2;
		Navigation navigation3 = deleteButton.navigation;
		if (saveSlotAbove != null)
		{
			navigation3.selectOnUp = saveSlotAbove.deleteButton;
		}
		if (saveSlotBelow != null)
		{
			navigation3.selectOnDown = saveSlotBelow.deleteButton;
		}
		deleteButton.navigation = navigation3;
	}

	public void Select()
	{
		saveSlotButton.Select();
	}

	public void OnSaveSlotButtonSelected()
	{
		SaveSlot.OnSaveSlotSelected?.Invoke(this);
	}

	public void Click()
	{
		SaveSlot.OnSaveSlotClicked?.Invoke(this);
	}

	public void Rename()
	{
		SaveSlot.OnSaveSlotRename?.Invoke(this);
	}

	public void Delete()
	{
		SaveSlot.OnSaveSlotDelete?.Invoke(this);
	}
}
