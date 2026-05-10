using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using _Scripts.LevelSaving;
using _Scripts.Singletons;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.UI.Save_Slots;

public class SaveSlotController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private SaveFileCanvas saveFileCanvas;

	[SerializeField]
	private SaveSlot slotPrefab;

	[SerializeField]
	private Transform slotListContainer;

	[SerializeField]
	private RectTransform plusButtonTransform;

	[SerializeField]
	private Button plusButton;

	[SerializeField]
	private Button exitButton;

	[SerializeField]
	private GameObject mobileScrollAreaIndicator;

	[Header("Scroll View")]
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private RectTransform viewport;

	[SerializeField]
	[Min(0f)]
	private float stepPixels;

	[SerializeField]
	private bool invertWheel;

	[SerializeField]
	[Min(0f)]
	private float heldScrollMinInterval = 0.08f;

	[Header("Viewport Logic")]
	[SerializeField]
	[Min(1f)]
	private int visibleSlots = 4;

	[SerializeField]
	[Min(0f)]
	private int bottomBuffer;

	[SerializeField]
	[Min(0f)]
	private int topBuffer = 1;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference navigateAction;

	[SerializeField]
	private InputActionReference scrollWheelAction;

	[Header("Level Data")]
	[SerializeField]
	private LevelData officeLevelData;

	[SerializeField]
	private LevelData kidsRoomLevelData;

	private List<SaveSlot> saveSlots = new List<SaveSlot>();

	private LevelData levelData;

	private float lastHeldScrollAt = -999f;

	private bool navigateHeld;

	private float maxScrollable;

	private void OnEnable()
	{
		SaveSlot.OnSaveSlotClicked -= SaveSlot_OnSaveSlotClicked;
		SaveSlot.OnSaveSlotSelected -= SaveSlot_OnSaveSlotSelected;
		SaveSlot.OnSaveSlotRename -= SaveSlot_OnSaveSlotRename;
		SaveSlot.OnSaveSlotDelete -= SaveSlot_OnSaveSlotDelete;
		SaveSlot.OnSaveSlotClicked += SaveSlot_OnSaveSlotClicked;
		SaveSlot.OnSaveSlotSelected += SaveSlot_OnSaveSlotSelected;
		SaveSlot.OnSaveSlotRename += SaveSlot_OnSaveSlotRename;
		SaveSlot.OnSaveSlotDelete += SaveSlot_OnSaveSlotDelete;
		ScrollToTop();
		navigateAction.action.performed += OnNavigatePerformed;
		scrollWheelAction.action.performed += OnScrollWheel;
		UpdateExitButtonNavigation();
		StartCoroutine(CoNextFrameRecalc());
	}

	private void OnDisable()
	{
		SaveSlot.OnSaveSlotClicked -= SaveSlot_OnSaveSlotClicked;
		SaveSlot.OnSaveSlotSelected -= SaveSlot_OnSaveSlotSelected;
		SaveSlot.OnSaveSlotRename -= SaveSlot_OnSaveSlotRename;
		SaveSlot.OnSaveSlotDelete -= SaveSlot_OnSaveSlotDelete;
		navigateAction.action.performed -= OnNavigatePerformed;
		scrollWheelAction.action.performed -= OnScrollWheel;
		navigateHeld = false;
	}

	private IEnumerator CoNextFrameRecalc()
	{
		yield return null;
		RecalculateScrollable();
		AutoInitStepIfNeeded();
		if (saveSlots.Count > 0)
		{
			saveSlots[0].Select();
		}
		else
		{
			plusButton.Select();
		}
	}

	private void MoveSteps(int direction, int steps)
	{
		if (steps > 0)
		{
			RecalculateScrollable();
			float num = Mathf.Max(1f, stepPixels);
			float y = Mathf.Clamp(content.anchoredPosition.y + (float)direction * num * (float)steps, 0f, maxScrollable);
			content.anchoredPosition = new Vector2(content.anchoredPosition.x, y);
		}
	}

	private void EnsureIndexVisible(int index)
	{
		if (saveSlots.Count == 0)
		{
			return;
		}
		int topIndex = GetTopIndex();
		int num = topIndex + visibleSlots - 1;
		int num2 = topIndex + topBuffer;
		int num3 = num - bottomBuffer;
		if (index < num2 || index > num3)
		{
			if (index < num2)
			{
				int num4 = Mathf.Clamp(index - topBuffer, 0, Mathf.Max(0, saveSlots.Count - visibleSlots));
				int steps = Mathf.Max(0, topIndex - num4);
				MoveSteps(-1, steps);
			}
			else if (index > num3)
			{
				int num5 = Mathf.Clamp(index - (visibleSlots - bottomBuffer - 1), 0, Mathf.Max(0, saveSlots.Count - visibleSlots));
				int steps2 = Mathf.Max(0, num5 - topIndex);
				MoveSteps(1, steps2);
			}
		}
	}

	private bool TryGetSelectedSaveSlotIndex(out int selectedIndex)
	{
		selectedIndex = -1;
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (currentSelectedGameObject == null)
		{
			return false;
		}
		SaveSlot componentInParent = currentSelectedGameObject.GetComponentInParent<SaveSlot>();
		if (componentInParent == null)
		{
			return false;
		}
		int num = saveSlots.IndexOf(componentInParent);
		if (num < 0)
		{
			return false;
		}
		selectedIndex = num;
		return true;
	}

	private int GetTopIndex()
	{
		float num = Mathf.Max(1f, stepPixels);
		int value = Mathf.FloorToInt(content.anchoredPosition.y / num);
		int max = Mathf.Max(0, saveSlots.Count - visibleSlots);
		return Mathf.Clamp(value, 0, max);
	}

	private bool CanScrollDown(int topIdx)
	{
		int num = Mathf.Max(0, saveSlots.Count - visibleSlots);
		return topIdx < num;
	}

	private bool CanScrollUp(int topIdx)
	{
		return topIdx > 0;
	}

	private void MoveOneStepIfThreshold(int direction)
	{
		if (saveSlots.Count <= visibleSlots || !TryGetSelectedSaveSlotIndex(out var selectedIndex))
		{
			return;
		}
		int topIndex = GetTopIndex();
		if (direction > 0)
		{
			int num = topIndex + (visibleSlots - bottomBuffer - 1);
			if (selectedIndex >= num && CanScrollDown(topIndex))
			{
				MoveOneStep(1);
			}
		}
		else if (direction < 0)
		{
			int num2 = topIndex + topBuffer;
			if (selectedIndex <= num2 && CanScrollUp(topIndex))
			{
				MoveOneStep(-1);
			}
		}
	}

	private void MoveOneStep(int direction)
	{
		RecalculateScrollable();
		float num = Mathf.Max(1f, stepPixels);
		float y = Mathf.Clamp(content.anchoredPosition.y + (float)direction * num, 0f, maxScrollable);
		content.anchoredPosition = new Vector2(content.anchoredPosition.x, y);
	}

	private void ScrollToTop()
	{
		scrollRect.verticalNormalizedPosition = 1f;
		StartCoroutine(CoNextFrameAlignTop());
	}

	private IEnumerator CoNextFrameAlignTop()
	{
		yield return null;
		content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);
	}

	private void RecalculateScrollable()
	{
		float num = Mathf.Max(0f, content.rect.height);
		float num2 = Mathf.Max(0f, viewport.rect.height);
		maxScrollable = Mathf.Max(0f, num - num2);
	}

	private void AutoInitStepIfNeeded()
	{
		if (stepPixels > 0f)
		{
			return;
		}
		float num = 0f;
		if (slotListContainer != null && slotListContainer.childCount > 0)
		{
			RectTransform rectTransform = slotListContainer.GetChild(0) as RectTransform;
			if (rectTransform != null)
			{
				num = Mathf.Abs(rectTransform.rect.height);
			}
		}
		if (num <= 0f && slotPrefab != null)
		{
			RectTransform component = slotPrefab.GetComponent<RectTransform>();
			if (component != null)
			{
				num = Mathf.Abs(component.rect.height);
			}
		}
		stepPixels = ((num > 0f) ? num : 100f);
	}

	private void PrintContents()
	{
		Debug.Log("Save Slots:");
		foreach (SaveSlot saveSlot in saveSlots)
		{
			Debug.Log(saveSlot);
		}
	}

	private void UpdateExitButtonNavigation()
	{
		Navigation navigation = exitButton.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnDown = ((saveSlots.Count > 0) ? saveSlots[0].SaveSlotBtn.Btn : plusButton);
		exitButton.navigation = navigation;
	}

	private void TryLoadLevel()
	{
		Singleton<SceneController>.Instance.LoadCreativeModeLevel(levelData.sceneName);
	}

	public void GenerateSaveSlotList(LevelData newLevelData)
	{
		mobileScrollAreaIndicator.SetActive(value: false);
		levelData = newLevelData;
		foreach (Transform item in slotListContainer)
		{
			if (!(item == plusButtonTransform))
			{
				Object.Destroy(item.gameObject);
			}
		}
		saveSlots.Clear();
		LevelSaveData[] allLevelSaveDataForLevel = SaveController.GetAllLevelSaveDataForLevel(levelData.sceneName);
		if (allLevelSaveDataForLevel == null)
		{
			Debug.Log("No save files found for " + levelData.sceneName);
			return;
		}
		allLevelSaveDataForLevel = allLevelSaveDataForLevel.OrderByDescending((LevelSaveData x) => x.date).ToArray();
		LevelSaveData[] array = allLevelSaveDataForLevel;
		foreach (LevelSaveData levelSaveData in array)
		{
			SaveSlot saveSlot = Object.Instantiate(slotPrefab, slotListContainer, worldPositionStays: false);
			saveSlot.name = levelSaveData.fileName;
			saveSlot.transform.SetSiblingIndex(plusButtonTransform.GetSiblingIndex());
			saveSlot.Setup(levelData, levelSaveData);
			saveSlots.Add(saveSlot);
		}
		for (int j = 0; j < saveSlots.Count; j++)
		{
			SaveSlot saveSlot2 = saveSlots[j];
			SaveSlot saveSlotAbove = ((j - 1 < 0) ? null : saveSlots[j - 1]);
			SaveSlot saveSlotBelow = ((j + 1 >= saveSlots.Count) ? null : saveSlots[j + 1]);
			saveSlot2.SetupNavigation(saveSlotAbove, saveSlotBelow, exitButton, plusButton);
		}
	}

	public void AddNewSaveSlot()
	{
		LevelSavingController.GenerateNewLevelSaveData(levelData);
		TryLoadLevel();
	}

	public void DeleteSaveSlot(SaveSlot saveSlot)
	{
		if (saveSlots.Count > 0)
		{
			int num = saveSlots.IndexOf(saveSlot);
			SaveController.DeleteSaveFile(saveSlot.SaveData.sceneName, saveSlot.SaveData.fileName);
			saveSlots.RemoveAt(num);
			UpdateExitButtonNavigation();
			PrintContents();
			if (saveSlots.Count > 0)
			{
				int index = Mathf.Clamp(num, 0, saveSlots.Count - 1);
				saveSlots[index].Select();
			}
			else
			{
				plusButton.Select();
			}
		}
	}

	private void SaveSlot_OnSaveSlotClicked(SaveSlot saveSlot)
	{
		LevelSavingController.LoadLevelSaveData(saveSlot.SaveData);
		TryLoadLevel();
	}

	private void SaveSlot_OnSaveSlotSelected(SaveSlot saveSlot)
	{
		int num = saveSlots.IndexOf(saveSlot);
		if (num >= 0 && navigateHeld)
		{
			float unscaledTime = Time.unscaledTime;
			if (!(unscaledTime - lastHeldScrollAt < heldScrollMinInterval))
			{
				lastHeldScrollAt = unscaledTime;
				EnsureIndexVisible(num);
			}
		}
	}

	private void SaveSlot_OnSaveSlotRename(SaveSlot saveSlot)
	{
		saveFileCanvas.ShowSaveSlotRenamingPanel(saveSlot);
	}

	private void SaveSlot_OnSaveSlotDelete(SaveSlot saveSlot)
	{
		saveFileCanvas.ShowSaveSlotDeletionPanel(saveSlot);
	}

	private void OnNavigatePerformed(InputAction.CallbackContext ctx)
	{
		Vector2 vector = ctx.ReadValue<Vector2>();
		if (Mathf.Abs(vector.y) < 0.5f)
		{
			navigateHeld = false;
			return;
		}
		int direction = ((!(vector.y > 0f)) ? 1 : (-1));
		MoveOneStepIfThreshold(direction);
		navigateHeld = true;
	}

	private void OnScrollWheel(InputAction.CallbackContext ctx)
	{
		Vector2 vector = ctx.ReadValue<Vector2>();
		if (!(Mathf.Abs(vector.y) < 0.01f))
		{
			int direction = ((!(vector.y > 0f)) ? ((!invertWheel) ? 1 : (-1)) : (invertWheel ? 1 : (-1)));
			MoveOneStep(direction);
		}
	}
}
