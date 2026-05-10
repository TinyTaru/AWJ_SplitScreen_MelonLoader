using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.LevelSaving;
using _Scripts.Singletons;
using _Scripts.UI.Panels;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.UI.Save_Slots;

public class SaveFileCanvas : Singleton<SaveFileCanvas>
{
	[Header("References")]
	[SerializeField]
	private RectTransform saveSlotSelectionPanel;

	[SerializeField]
	private RectTransform saveSlotDeletionPanel;

	[SerializeField]
	private RectTransform saveSlotRenamingPanel;

	[SerializeField]
	private SaveSlotController saveSlotController;

	[SerializeField]
	private TextMeshProUGUI renamingSaveSlotName;

	[SerializeField]
	private TMP_InputField renamingInputField;

	[SerializeField]
	private TextMeshProUGUI deletionSaveSlotName;

	private static PanelManager panelManager;

	private static LevelData levelData;

	private static SaveSlot selectedSaveSlot;

	protected override void Awake()
	{
		base.Awake();
		panelManager = GetComponentInParent<PanelManager>();
		saveSlotSelectionPanel.gameObject.SetActive(value: false);
		saveSlotDeletionPanel.gameObject.SetActive(value: false);
		saveSlotRenamingPanel.gameObject.SetActive(value: false);
	}

	private IEnumerator InvalidFileNameCoroutine()
	{
		Image image = renamingInputField.GetComponent<Image>();
		image.color = Color.crimson;
		yield return new WaitForSeconds(0.25f);
		image.color = Color.white;
	}

	public static void Open(LevelData newLevelData)
	{
		levelData = newLevelData;
		panelManager.OpenPanel(Singleton<SaveFileCanvas>.Instance.saveSlotSelectionPanel);
		Singleton<SaveFileCanvas>.Instance.saveSlotController.GenerateSaveSlotList(levelData);
	}

	public void ShowSaveSlotDeletionPanel(SaveSlot saveSlot)
	{
		selectedSaveSlot = saveSlot;
		panelManager.OpenPanel(saveSlotDeletionPanel);
		deletionSaveSlotName.text = saveSlot.SaveData.fileName;
	}

	public void DeleteSaveSlot()
	{
		Singleton<SaveFileCanvas>.Instance.saveSlotController.DeleteSaveSlot(selectedSaveSlot);
		Singleton<SaveFileCanvas>.Instance.saveSlotController.GenerateSaveSlotList(levelData);
		panelManager.GoBack();
	}

	public void GoBack()
	{
		Singleton<SaveFileCanvas>.Instance.saveSlotController.GenerateSaveSlotList(levelData);
		panelManager.GoBack();
	}

	public void ShowSaveSlotRenamingPanel(SaveSlot saveSlot)
	{
		selectedSaveSlot = saveSlot;
		panelManager.OpenPanel(saveSlotRenamingPanel);
		renamingSaveSlotName.text = saveSlot.SaveData.fileName;
		renamingInputField.text = "";
		renamingInputField.onSubmit.AddListener(RenameSaveFile);
	}

	public void RenameSaveFile(string text)
	{
		LevelSaveData saveData = selectedSaveSlot.SaveData;
		string text2 = renamingInputField.text;
		if (!SaveController.RenameSaveFile(saveData.sceneName, saveData.fileName, text2))
		{
			StartCoroutine(InvalidFileNameCoroutine());
			return;
		}
		renamingInputField.onSubmit.RemoveListener(RenameSaveFile);
		Singleton<SaveFileCanvas>.Instance.saveSlotController.GenerateSaveSlotList(levelData);
		panelManager.GoBack();
	}
}
