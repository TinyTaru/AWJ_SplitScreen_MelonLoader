using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Serialization;
using _Scripts.CoinDetector;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.UI.Notifications;
using _Scripts.UI.Scene_Loading;
using _Scripts.UI.TaskList;
using _Scripts.Wardrobe;

namespace _Scripts.Singletons;

public class PopUpController : Singleton<PopUpController>
{
	[SerializeField]
	private Transform popUpContainer;

	[SerializeField]
	private WebColorPaletteSO webColorPaletteSO;

	[FormerlySerializedAs("taskNotification")]
	[Header("Notification Prefabs")]
	[SerializeField]
	private TaskCompleteNotification taskNotificationPrefab;

	[SerializeField]
	private TaskCompleteNotification taskNotificationTutorialPrefab;

	[SerializeField]
	private TaskCompleteNotification taskNotificationKitchenPrefab;

	[SerializeField]
	private TaskCompleteNotification taskNotificationOfficePrefab;

	[SerializeField]
	private TaskCompleteNotification taskNotificationKidsRoomPrefab;

	[SerializeField]
	private TaskCompleteNotification taskNotificationLivingRoomPrefab;

	[SerializeField]
	private ItemUnlockedNotification itemNotification;

	[SerializeField]
	private WishlistNotification wishlistNotification;

	[SerializeField]
	private LevelUnlockedNotification levelNotification;

	[SerializeField]
	private LevelCompleteNotification levelCompleteNotification;

	[SerializeField]
	private CoinDetectorUnlockedNotification coinDetectorUnlockedNotification;

	[SerializeField]
	private AllCollectiblesCollectedNotification allCollectiblesCollectedNotification;

	[FormerlySerializedAs("newItemsAvailableInShopItemNotification")]
	[FormerlySerializedAs("shopItemNotification")]
	[SerializeField]
	private NewItemsAvailableInShopNotification newItemsAvailableInShopNotification;

	private Queue<IQueueableNotification> myQueue;

	private bool popUPIsActive;

	private CanvasGroup popUpContainerCanvasGroup;

	protected override void Awake()
	{
		base.Awake();
		popUpContainerCanvasGroup = popUpContainer.GetComponent<CanvasGroup>();
	}

	private void OnEnable()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnPauseGame += GameController_OnPauseGame;
			Singleton<GameController>.Instance.OnContinueGame += GameController_OnContinueGame;
		}
	}

	private void OnDisable()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnPauseGame -= GameController_OnPauseGame;
			Singleton<GameController>.Instance.OnContinueGame -= GameController_OnContinueGame;
		}
	}

	private void Start()
	{
		if (Singleton<TaskListUI>.Instance != null)
		{
			Singleton<TaskListUI>.Instance.OnTaskCompleted += TaskListUI_OnTaskCompleted;
			Singleton<TaskListUI>.Instance.OnAllTaskCompleted += TaskListUI_OnAllTaskCompleted;
		}
		if (Singleton<CosmeticItemsController>.Instance != null)
		{
			Singleton<CosmeticItemsController>.Instance.OnCosmeticItemUnlocked += CosmeticItemsController_OnCosmeticItemUnlocked;
			Singleton<CosmeticItemsController>.Instance.OnNewItemsAvailableInShop += CosmeticItemsController_OnNewItemsAvailableInShop;
		}
		if (Singleton<ProfileController>.Instance != null)
		{
			Singleton<ProfileController>.Instance.OnLevelUnlocked += ProfileController_OnLevelUnlocked;
		}
		if (Singleton<_Scripts.CoinDetector.CoinDetector>.Instance != null)
		{
			_Scripts.CoinDetector.CoinDetector.OnCoinDetectorUnlocked += CoinDetector_OnCoinDetectorUnlocked;
		}
		if (Singleton<CoinController>.Instance != null)
		{
			Singleton<CoinController>.Instance.OnAllCoinsCollectedInCurrentLevel += Instance_OnAllCoinsCollectedInCurrentLevel;
		}
		myQueue = new Queue<IQueueableNotification>();
	}

	private void OnDestroy()
	{
		if (Singleton<CosmeticItemsController>.Instance != null)
		{
			Singleton<CosmeticItemsController>.Instance.OnCosmeticItemUnlocked -= CosmeticItemsController_OnCosmeticItemUnlocked;
			Singleton<CosmeticItemsController>.Instance.OnNewItemsAvailableInShop -= CosmeticItemsController_OnNewItemsAvailableInShop;
		}
		if (Singleton<ProfileController>.Instance != null)
		{
			Singleton<ProfileController>.Instance.OnLevelUnlocked -= ProfileController_OnLevelUnlocked;
		}
	}

	private void ShowNextPopUp()
	{
		if (myQueue.Count != 0 && !popUPIsActive)
		{
			IQueueableNotification queueableNotification = myQueue.Dequeue();
			queueableNotification.OnPopUpCompleted += Notification_OnPopUpCompleted;
			queueableNotification.ShowMessage();
			popUPIsActive = true;
		}
	}

	private void AddTaskCompletedPopUpToQueue(TaskDataSo taskDataSo)
	{
		string text = taskDataSo.text;
		TaskCompleteNotification original = taskNotificationPrefab;
		switch (Singleton<GameController>.Instance.Music)
		{
		case LevelMusic.Tutorial:
			original = taskNotificationTutorialPrefab;
			break;
		case LevelMusic.Kitchen:
			original = taskNotificationKitchenPrefab;
			break;
		case LevelMusic.Office:
			original = taskNotificationOfficePrefab;
			break;
		case LevelMusic.KidsRoom:
			original = taskNotificationKidsRoomPrefab;
			break;
		case LevelMusic.LivingRoom:
			original = taskNotificationLivingRoomPrefab;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case LevelMusic.Menu:
		case LevelMusic.Hub:
		case LevelMusic.IslandForest:
		case LevelMusic.IslandDesert:
			break;
		}
		TaskCompleteNotification taskCompleteNotification = UnityEngine.Object.Instantiate(original, popUpContainer);
		taskCompleteNotification.gameObject.SetActive(value: false);
		taskCompleteNotification.SetMessageText(text);
		myQueue.Enqueue(taskCompleteNotification);
	}

	private void AddAllTaskCompletedPopUpToQueue(LevelMusic level)
	{
		LevelCompleteNotification levelCompleteNotification = UnityEngine.Object.Instantiate(this.levelCompleteNotification, popUpContainer);
		levelCompleteNotification.SetLevel(level);
		myQueue.Enqueue(levelCompleteNotification);
	}

	private void AddWebColorUnlockedPopUpToQueue(CosmeticItemWebSo cosmeticItemWebSo)
	{
		string displayName = cosmeticItemWebSo.displayName;
		Sprite webSprite = cosmeticItemWebSo.webSo.webSprite;
		ItemUnlockedNotification itemUnlockedNotification = UnityEngine.Object.Instantiate(itemNotification, popUpContainer);
		itemUnlockedNotification.SetItemSprite(webSprite);
		itemUnlockedNotification.SetMessageText(DialogueManager.GetLocalizedText("Misc_Popup_New Web Color"));
		itemUnlockedNotification.SetHintText(DialogueManager.GetLocalizedText("Misc_Popup_Change Web Color"));
		itemUnlockedNotification.SetTitleText(DialogueManager.GetLocalizedText(displayName));
		itemUnlockedNotification.SetUnlockType(ItemUnlockedNotification.UnlockType.WebColor);
		myQueue.Enqueue(itemUnlockedNotification);
	}

	private void AddHatUnlockedPopUpToQueue(CosmeticItemHatSo cosmeticItemHatSo)
	{
		string displayName = cosmeticItemHatSo.displayName;
		Sprite hatSprite = cosmeticItemHatSo.hatSo.hatSprite;
		ItemUnlockedNotification itemUnlockedNotification = UnityEngine.Object.Instantiate(itemNotification, popUpContainer);
		itemUnlockedNotification.SetItemSprite(hatSprite);
		itemUnlockedNotification.SetMessageText(DialogueManager.GetLocalizedText("Misc_Popup_New Hat"));
		itemUnlockedNotification.SetTitleText(DialogueManager.GetLocalizedText(displayName));
		myQueue.Enqueue(itemUnlockedNotification);
	}

	private void AddShoeUnlockedPopUpToQueue(CosmeticItemShoeSo cosmeticItemShoeSo)
	{
		string displayName = cosmeticItemShoeSo.displayName;
		Sprite shoeSprite = cosmeticItemShoeSo.shoeSo.shoeSprite;
		ItemUnlockedNotification itemUnlockedNotification = UnityEngine.Object.Instantiate(itemNotification, popUpContainer);
		itemUnlockedNotification.SetItemSprite(shoeSprite);
		itemUnlockedNotification.SetMessageText(DialogueManager.GetLocalizedText("Misc_Popup_New Shoes"));
		itemUnlockedNotification.SetTitleText(DialogueManager.GetLocalizedText(displayName));
		myQueue.Enqueue(itemUnlockedNotification);
	}

	private void AddAccessoryUnlockedPopUpToQueue(CosmeticItemAccessorySo cosmeticItemAccessorySo)
	{
		string displayName = cosmeticItemAccessorySo.displayName;
		Sprite accessorySprite = cosmeticItemAccessorySo.accessorySo.accessorySprite;
		ItemUnlockedNotification itemUnlockedNotification = UnityEngine.Object.Instantiate(itemNotification, popUpContainer);
		itemUnlockedNotification.SetItemSprite(accessorySprite);
		itemUnlockedNotification.SetMessageText(DialogueManager.GetLocalizedText("Misc_Popup_New Accessory"));
		itemUnlockedNotification.SetTitleText(DialogueManager.GetLocalizedText(displayName));
		myQueue.Enqueue(itemUnlockedNotification);
	}

	private void AddNewItemsAvailableInShopPopUpToQueue()
	{
		NewItemsAvailableInShopNotification newItemsAvailableInShopNotification = UnityEngine.Object.Instantiate(this.newItemsAvailableInShopNotification, popUpContainer);
		newItemsAvailableInShopNotification.SetMessageText(DialogueManager.GetLocalizedText("Misc_Popup_New Items"));
		myQueue.Enqueue(newItemsAvailableInShopNotification);
	}

	private void AddLevelUnlockedPopUpToQueue(LevelData levelData)
	{
		Debug.Log("AddLevelUnlockedPoupUPToQueue");
		string levelName = levelData.levelName;
		Sprite sprite = null;
		sprite = ((!SettingsController.ArachnophobiaMode) ? levelData.levelImageNormal : levelData.levelImageArachnophobia);
		LevelUnlockedNotification levelUnlockedNotification = UnityEngine.Object.Instantiate(levelNotification, popUpContainer);
		if ((bool)sprite)
		{
			levelUnlockedNotification.SetLevelSprite(sprite);
		}
		levelUnlockedNotification.SetMessageText(DialogueManager.GetLocalizedText("Misc_Popup_New Level"));
		levelUnlockedNotification.SetTitleText(DialogueManager.GetLocalizedText(levelName));
		myQueue.Enqueue(levelUnlockedNotification);
	}

	private void AddCoinDetectorUnlockedPopUpToQueue()
	{
		CoinDetectorUnlockedNotification item = UnityEngine.Object.Instantiate(coinDetectorUnlockedNotification, popUpContainer);
		myQueue.Enqueue(item);
	}

	private void AddAllCollectiblesCollectedPopUpToQueue()
	{
		AllCollectiblesCollectedNotification item = UnityEngine.Object.Instantiate(allCollectiblesCollectedNotification, popUpContainer);
		myQueue.Enqueue(item);
	}

	private void AddWishlistPopUpToQueue()
	{
		WishlistNotification item = UnityEngine.Object.Instantiate(wishlistNotification, popUpContainer);
		myQueue.Enqueue(item);
	}

	private void TaskListUI_OnTaskCompleted(object sender, TaskListUI.OnTaskCompletedEventArgs e)
	{
		AddTaskCompletedPopUpToQueue(e.taskDataSO);
		ShowNextPopUp();
	}

	private void TaskListUI_OnAllTaskCompleted(object sender, TaskListUI.OnAllTaskCompletedEventArgs e)
	{
		if (e.level != LevelMusic.Tutorial)
		{
			AddAllTaskCompletedPopUpToQueue(e.level);
			ShowNextPopUp();
		}
	}

	private void ProfileController_OnLevelUnlocked(object sender, ProfileController.OnLevelUnlockedEventArgs e)
	{
		AddLevelUnlockedPopUpToQueue(e.levelData);
		ShowNextPopUp();
	}

	private void CosmeticItemsController_OnCosmeticItemUnlocked(object sender, CosmeticItemsController.OnCosmeticItemUnlockedEventArgs e)
	{
		CosmeticItemSo cosmeticItemSo = e.cosmeticItemSo;
		Debug.Log("cosmeticItemSo " + cosmeticItemSo.name);
		if (cosmeticItemSo is CosmeticItemHatSo cosmeticItemHatSo)
		{
			AddHatUnlockedPopUpToQueue(cosmeticItemHatSo);
		}
		else if (cosmeticItemSo is CosmeticItemShoeSo cosmeticItemShoeSo)
		{
			AddShoeUnlockedPopUpToQueue(cosmeticItemShoeSo);
		}
		else if (cosmeticItemSo is CosmeticItemAccessorySo cosmeticItemAccessorySo)
		{
			AddAccessoryUnlockedPopUpToQueue(cosmeticItemAccessorySo);
		}
		else if (cosmeticItemSo is CosmeticItemWebSo cosmeticItemWebSo)
		{
			AddWebColorUnlockedPopUpToQueue(cosmeticItemWebSo);
		}
		ShowNextPopUp();
	}

	private void CosmeticItemsController_OnNewItemsAvailableInShop(object sender, EventArgs e)
	{
		AddNewItemsAvailableInShopPopUpToQueue();
		ShowNextPopUp();
	}

	private void Notification_OnPopUpCompleted(object sender, EventArgs e)
	{
		popUPIsActive = false;
		ShowNextPopUp();
	}

	private void GameController_OnPauseGame(object sender, EventArgs e)
	{
		popUpContainerCanvasGroup.alpha = 0f;
	}

	private void GameController_OnContinueGame(object sender, EventArgs e)
	{
		popUpContainerCanvasGroup.alpha = 1f;
	}

	private void CoinDetector_OnCoinDetectorUnlocked()
	{
		AddCoinDetectorUnlockedPopUpToQueue();
		ShowNextPopUp();
	}

	private void Instance_OnAllCoinsCollectedInCurrentLevel()
	{
		AddAllCollectiblesCollectedPopUpToQueue();
		ShowNextPopUp();
	}
}
