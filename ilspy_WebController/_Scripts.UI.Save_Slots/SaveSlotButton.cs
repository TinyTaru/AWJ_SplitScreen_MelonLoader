using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using _Scripts.LevelSaving;
using _Scripts.Singletons;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.UI.Save_Slots;

public class SaveSlotButton : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	[Header("References")]
	[SerializeField]
	private SaveSlot saveSlot;

	[SerializeField]
	private Button button;

	[SerializeField]
	private Image levelImage;

	[SerializeField]
	private TextMeshProUGUI saveSlotText;

	[SerializeField]
	private TextMeshProUGUI dateText;

	[Header("Sounds")]
	[SerializeField]
	private EventReference clearProfileSoundRef;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference clearSlot;

	private LevelData levelData;

	private LevelSaveData levelSaveData;

	public Button Btn => button;

	public void OnSelect(BaseEventData eventData)
	{
		saveSlot.OnSaveSlotButtonSelected();
	}

	public void Setup(LevelData newLevelData, LevelSaveData newLevelSaveData)
	{
		levelData = newLevelData;
		levelSaveData = newLevelSaveData;
		levelImage.sprite = (SettingsController.ArachnophobiaMode ? levelData.levelImageArachnophobia : levelData.levelImageNormal);
		saveSlotText.text = levelSaveData.fileName;
		dateText.text = $"{levelSaveData.date:yyyy-MM-dd}";
	}

	public void Click()
	{
		saveSlot.Click();
	}

	public void Select()
	{
		button.Select();
	}
}
