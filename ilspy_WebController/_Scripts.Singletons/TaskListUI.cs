using System;
using System.Collections.Generic;
using DG.Tweening;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.Miscellaneous.Christmas;
using _Scripts.UI.MobileMonetization;
using _Scripts.UI.Notifications;
using _Scripts.UI.TaskList;

namespace _Scripts.Singletons;

public class TaskListUI : Singleton<TaskListUI>
{
	public class OnTaskCompletedEventArgs : EventArgs
	{
		public TaskDataSo taskDataSO;
	}

	public class OnAllTaskCompletedEventArgs : EventArgs
	{
		public LevelMusic level;
	}

	[Header("References")]
	[SerializeField]
	private GameObject taskListParent;

	[SerializeField]
	private GameObject taskListGfx;

	[FormerlySerializedAs("toDoEntryPrefab")]
	[SerializeField]
	private TaskEntry taskEntryPrefab;

	[FormerlySerializedAs("toDoEntryContainer")]
	[SerializeField]
	private Transform taskEntryContainer;

	[SerializeField]
	private TaskCompleteNotification messagePrefab;

	[SerializeField]
	private GameObject christmasOverlay;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TaskListFontsSo taskListFontsSo;

	[Header("Task Data")]
	[SerializeField]
	private TaskList taskList;

	[SerializeField]
	private TaskList christmasTaskList;

	[SerializeField]
	private int tasksToMakeNewItemsAvailableInShop = 3;

	[SerializeField]
	private int tasksToMakeExoticItemsAvailableInShop = 5;

	[SerializeField]
	private int tasksToUnlockNextLevel = 7;

	[SerializeField]
	private int tasksToShowMobileReviewPopup = 7;

	[Header("Animation")]
	[SerializeField]
	private float animationDurationIn = 0.5f;

	[SerializeField]
	private float animationDurationOut = 0.5f;

	[SerializeField]
	private bool applyFade = true;

	[SerializeField]
	private float hiddenPositionScale = 1.5f;

	[SerializeField]
	private Ease easingTypeInMove = Ease.OutBack;

	[SerializeField]
	private Ease easingTypeOutMove = Ease.InBack;

	[SerializeField]
	private Ease easingTypeInFade = Ease.OutQuint;

	[SerializeField]
	private Ease easingTypeOutFade = Ease.InQuint;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onTaskFinished;

	[SerializeField]
	private UnityEvent onShowTodoList;

	[SerializeField]
	private UnityEvent onHideTodoList;

	[SerializeField]
	private UnityEvent onMakeNewItemsAvailableInShop;

	[SerializeField]
	private UnityEvent onMakeExoticItemsAvailableInShop;

	[SerializeField]
	private UnityEvent onNextLevelUnlocked;

	[SerializeField]
	private UnityEvent onShowMobileReviewPopup;

	[SerializeField]
	private UnityEvent onAllTasksFinished;

	private CanvasGroup canvasGroup;

	private Vector2 hiddenPosition;

	private Vector2 visiblePosition;

	private bool toDoListActive;

	private List<TaskEntry> taskEntriesList;

	private bool animationRunning;

	private RectTransform todoListRectTransform;

	private Sequence sequence;

	private int completedTasksOld;

	public event EventHandler OnShowTodoList;

	public event EventHandler OnHideTodoList;

	public event EventHandler<OnTaskCompletedEventArgs> OnTaskCompleted;

	public event EventHandler<OnAllTaskCompletedEventArgs> OnAllTaskCompleted;

	private new void Awake()
	{
		base.Awake();
		todoListRectTransform = taskListGfx.GetComponent<RectTransform>();
		canvasGroup = taskListGfx.GetComponent<CanvasGroup>();
		toDoListActive = false;
		animationRunning = false;
		taskEntriesList = new List<TaskEntry>();
		foreach (Transform item in taskEntryContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		visiblePosition = todoListRectTransform.anchoredPosition;
		hiddenPosition = visiblePosition + Vector2.down * todoListRectTransform.rect.height * hiddenPositionScale;
		sequence.Kill();
		toDoListActive = false;
		animationRunning = false;
		todoListRectTransform.anchoredPosition = hiddenPosition;
		taskListGfx.SetActive(value: false);
		if (applyFade)
		{
			canvasGroup.alpha = 0f;
		}
	}

	private void Start()
	{
		TaskDataSo[] tasks = taskList.tasks;
		foreach (TaskDataSo taskDataSO in tasks)
		{
			TaskEntry taskEntry = UnityEngine.Object.Instantiate(taskEntryPrefab, taskEntryContainer);
			taskEntry.Setup(taskDataSO);
			taskEntriesList.Add(taskEntry);
		}
		int num = 0;
		tasks = taskList.tasks;
		for (int i = 0; i < tasks.Length; i++)
		{
			if (QuestLog.GetQuestState(tasks[i].questName) == QuestState.Success)
			{
				num++;
			}
		}
		completedTasksOld = num;
		if (DialogueManager.instance != null)
		{
			DialogueManager.instance.receivedUpdateTracker += DialogueManager_ReceivedUpdateTracker;
			DialogueManager.instance.conversationStarted += DialogueManager_OnConversationStarted;
		}
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnToggleTaskList += GameController_OnToggleTaskList;
			Singleton<GameController>.Instance.OnPauseGame += GameController_OnPauseGame;
			Singleton<GameController>.Instance.OnContinueGame += GameController_OnContinueGame;
		}
		if (Singleton<KitchenChristmasController>.Instance != null)
		{
			Singleton<KitchenChristmasController>.Instance.OnActivateChristmasEffects += ChristmasController_OnActivateChristmasEffects;
		}
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnLanguageChanged += SettingsController_OnLanguageChanged;
		}
	}

	private void OnDestroy()
	{
		if (DialogueManager.instance != null)
		{
			DialogueManager.instance.receivedUpdateTracker -= DialogueManager_ReceivedUpdateTracker;
		}
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnToggleTaskList -= GameController_OnToggleTaskList;
			Singleton<GameController>.Instance.OnPauseGame -= GameController_OnPauseGame;
		}
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnLanguageChanged -= SettingsController_OnLanguageChanged;
		}
	}

	private void AnimateIn()
	{
		PerformLocalizationChanges();
		taskListGfx.SetActive(value: true);
		animationRunning = true;
		sequence = DOTween.Sequence();
		sequence.Append(todoListRectTransform.DOAnchorPos(visiblePosition, animationDurationIn).SetEase(easingTypeInMove));
		if (applyFade)
		{
			canvasGroup.alpha = 0f;
			sequence.Join(canvasGroup.DOFade(1f, animationDurationIn).SetEase(easingTypeInFade));
		}
		sequence.OnComplete(delegate
		{
			animationRunning = false;
			toDoListActive = true;
		});
		onShowTodoList?.Invoke();
		this.OnShowTodoList?.Invoke(this, EventArgs.Empty);
	}

	private void PerformLocalizationChanges()
	{
		TMP_FontAsset tMP_FontAsset = taskListFontsSo.dictionary[SystemLanguage.English];
		foreach (KeyValuePair<SystemLanguage, TMP_FontAsset> item in taskListFontsSo.dictionary)
		{
			if (item.Key == SettingsController.Language)
			{
				tMP_FontAsset = item.Value;
				break;
			}
		}
		titleText.font = tMP_FontAsset;
		foreach (TaskEntry taskEntries in taskEntriesList)
		{
			taskEntries.TranslateText();
			taskEntries.ChangeFont(tMP_FontAsset);
		}
	}

	private void AnimateOut()
	{
		animationRunning = true;
		sequence = DOTween.Sequence();
		sequence.Append(todoListRectTransform.DOAnchorPos(hiddenPosition, animationDurationOut).SetEase(easingTypeOutMove));
		if (applyFade)
		{
			sequence.Join(canvasGroup.DOFade(0f, animationDurationOut).SetEase(easingTypeOutFade));
		}
		sequence.OnComplete(delegate
		{
			animationRunning = false;
			toDoListActive = false;
			taskListGfx.SetActive(value: false);
		});
		onHideTodoList?.Invoke();
		this.OnHideTodoList?.Invoke(this, EventArgs.Empty);
	}

	public void EnableChristmasOverlay()
	{
		if (christmasOverlay != null)
		{
			christmasOverlay.SetActive(value: true);
		}
		TaskDataSo[] tasks = christmasTaskList.tasks;
		foreach (TaskDataSo taskDataSO in tasks)
		{
			TaskEntry taskEntry = UnityEngine.Object.Instantiate(taskEntryPrefab, taskEntryContainer);
			taskEntry.Setup(taskDataSO);
			taskEntriesList.Add(taskEntry);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(taskEntryContainer.GetComponent<RectTransform>());
	}

	private void GameController_OnToggleTaskList(object sender, EventArgs e)
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Running && !Singleton<PlayTimeCanvas>.Instance.IsOpen && !animationRunning)
		{
			if (toDoListActive)
			{
				AnimateOut();
				return;
			}
			AnimateIn();
			LayoutRebuilder.ForceRebuildLayoutImmediate(todoListRectTransform);
		}
	}

	private void GameController_OnPauseGame(object sender, EventArgs e)
	{
		sequence.Kill();
		toDoListActive = false;
		animationRunning = false;
		todoListRectTransform.anchoredPosition = hiddenPosition;
		taskListGfx.SetActive(value: false);
		if (applyFade)
		{
			canvasGroup.alpha = 0f;
		}
	}

	private void GameController_OnContinueGame(object sender, EventArgs e)
	{
	}

	private void DialogueManager_ReceivedUpdateTracker()
	{
		foreach (TaskEntry taskEntries in taskEntriesList)
		{
			taskEntries.CheckIfDone(out var justCompleted);
			if (!justCompleted)
			{
				continue;
			}
			this.OnTaskCompleted?.Invoke(this, new OnTaskCompletedEventArgs
			{
				taskDataSO = taskEntries.GetTaskData()
			});
			onTaskFinished?.Invoke();
			if (taskEntries.GetTaskData().CosmeticItemSos != null && Singleton<CosmeticItemsController>.Instance != null)
			{
				CosmeticItemSo[] cosmeticItemSos = taskEntries.GetTaskData().CosmeticItemSos;
				foreach (CosmeticItemSo cosmeticItemSo in cosmeticItemSos)
				{
					Singleton<CosmeticItemsController>.Instance.Unlock(cosmeticItemSo);
				}
			}
		}
		int num = 0;
		TaskDataSo[] tasks = taskList.tasks;
		for (int i = 0; i < tasks.Length; i++)
		{
			if (QuestLog.GetQuestState(tasks[i].questName) == QuestState.Success)
			{
				num++;
			}
		}
		if (num != completedTasksOld)
		{
			if (num >= tasksToMakeNewItemsAvailableInShop)
			{
				onMakeNewItemsAvailableInShop?.Invoke();
			}
			if (num >= tasksToMakeExoticItemsAvailableInShop)
			{
				onMakeExoticItemsAvailableInShop?.Invoke();
			}
			if (num >= tasksToUnlockNextLevel)
			{
				onNextLevelUnlocked?.Invoke();
			}
			_ = tasksToShowMobileReviewPopup;
			if (num == taskList.tasks.Length)
			{
				this.OnAllTaskCompleted?.Invoke(this, new OnAllTaskCompletedEventArgs
				{
					level = Singleton<GameController>.Instance.Music
				});
				onAllTasksFinished?.Invoke();
			}
			completedTasksOld = num;
		}
	}

	private void ChristmasController_OnActivateChristmasEffects(object sender, EventArgs e)
	{
		if (!(this == null))
		{
			EnableChristmasOverlay();
		}
	}

	private void SettingsController_OnLanguageChanged(object sender, EventArgs e)
	{
		PerformLocalizationChanges();
	}

	private void DialogueManager_OnConversationStarted(Transform t)
	{
		if (toDoListActive)
		{
			AnimateOut();
		}
	}
}
