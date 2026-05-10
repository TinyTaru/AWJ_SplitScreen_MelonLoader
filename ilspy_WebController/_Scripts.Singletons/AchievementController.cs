using System.Collections.Generic;
using UnityEngine;
using _Scripts.Achievements;
using _Scripts.General;

namespace _Scripts.Singletons;

public class AchievementController : Singleton<AchievementController>
{
	[SerializeField]
	private AchievementListSo achievementListSo;

	private static Dictionary<AchievementSo, bool> achievementDict;

	private static int cosmeticItemChangedCounter;

	private static int buttonsCollectedCounter;

	private static int itemBrokenCounter;

	private static int itemBrokenCounterKitchen;

	private void Start()
	{
		LoadAchievementData();
		SubscribeToEvents();
	}

	private void OnDestroy()
	{
		if (!(Singleton<AchievementController>.Instance != this))
		{
			SaveAchievementData();
			UnsubscribeFromEvents();
		}
	}

	private static void LoadAchievementData()
	{
		achievementDict = new Dictionary<AchievementSo, bool>();
		foreach (AchievementSo achievement in Singleton<AchievementController>.Instance.achievementListSo.achievements)
		{
			if (!achievementDict.ContainsKey(achievement))
			{
				bool value = SaveController.Load(achievement.id + "_Unlocked", defaultValue: false, SaveData.Achievements);
				achievementDict.Add(achievement, value);
			}
		}
		cosmeticItemChangedCounter = SaveController.Load("cosmeticItemChangedCounter", 0, SaveData.Achievements);
		itemBrokenCounter = SaveController.Load("itemBrokenCounter", 0, SaveData.Achievements);
		itemBrokenCounterKitchen = SaveController.Load("itemBrokenCounterKitchen", 0, SaveData.Achievements);
	}

	private static void SaveAchievementData()
	{
		foreach (KeyValuePair<AchievementSo, bool> item in achievementDict)
		{
			SaveController.Save(item.Key.id + "_Unlocked", item.Value, SaveData.Achievements);
		}
		SaveController.Save("cosmeticItemChangedCounter", cosmeticItemChangedCounter, SaveData.Achievements);
	}

	private static void SaveSingleAchievementDate(AchievementSo achievement, bool isUnlocked)
	{
		SaveController.Save(achievement.id + "_Unlocked", isUnlocked, SaveData.Achievements);
	}

	private static bool IsAchievementUnlocked(AchievementSo achievement)
	{
		achievementDict.TryGetValue(achievement, out var value);
		return value;
	}

	private static bool IsAchievementUnlocked(string id)
	{
		AchievementSo achievementSo = GetAchievementSo(id);
		achievementDict.TryGetValue(achievementSo, out var value);
		return value;
	}

	private static void TryUnlockAchievement(string id)
	{
		AchievementSo achievementSo = GetAchievementSo(id);
		if (!(achievementSo == null) && !IsAchievementUnlocked(achievementSo))
		{
			achievementDict[achievementSo] = true;
			SaveSingleAchievementDate(achievementSo, isUnlocked: true);
			Debug.Log("Achievement " + achievementSo.displayName + " unlocked!");
		}
	}

	private static AchievementSo GetAchievementSo(string id)
	{
		AchievementSo achievementSo = Singleton<AchievementController>.Instance.achievementListSo.achievements.Find((AchievementSo x) => x.id == id);
		if (achievementSo == null)
		{
			Debug.LogError("Trying to unlock achievement with id " + id + " but no achievement with that id was found in the achievement list!");
		}
		if (!achievementDict.ContainsKey(achievementSo))
		{
			Debug.LogError("Achievement Dictionary doesn't contain the achievement " + achievementSo.name + " with the id " + achievementSo.id + "!");
		}
		return achievementSo;
	}

	private static void SubscribeToEvents()
	{
		AchievementEvents.OnCosmeticItemChanged += AchievementEvents_OnCosmeticItemChanged;
		AchievementEvents.OnItemBroken += AchievementEvents_OnItemBroken;
		AchievementEvents.OnJumpIntoVoid += AchievementEvents_OnVoidWalker;
		AchievementEvents.OnTryToOpenDoor += AchievementEvents_OnTryOpenDoor;
		AchievementEvents.OnKitchenFullyFlooded += AchievementEvents_OnKitchenFullyFlooded;
		AchievementEvents.OnCoinsCollected += AchievementEvents_OnCoinsCollected;
	}

	private static void UnsubscribeFromEvents()
	{
		AchievementEvents.OnCosmeticItemChanged -= AchievementEvents_OnCosmeticItemChanged;
		AchievementEvents.OnItemBroken -= AchievementEvents_OnItemBroken;
		AchievementEvents.OnJumpIntoVoid -= AchievementEvents_OnVoidWalker;
		AchievementEvents.OnTryToOpenDoor -= AchievementEvents_OnTryOpenDoor;
		AchievementEvents.OnKitchenFullyFlooded -= AchievementEvents_OnKitchenFullyFlooded;
		AchievementEvents.OnCoinsCollected -= AchievementEvents_OnCoinsCollected;
	}

	private static void AchievementEvents_OnCosmeticItemChanged()
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel)
		{
			cosmeticItemChangedCounter++;
			SaveController.Save("cosmeticItemChangedCounter", cosmeticItemChangedCounter, SaveData.Achievements);
			Debug.Log($"cosmeticItemChangedCounter = {cosmeticItemChangedCounter}");
			if (cosmeticItemChangedCounter >= 50)
			{
				TryUnlockAchievement("FitCheck");
			}
		}
	}

	private static void AchievementEvents_OnItemBroken()
	{
		if (!Singleton<SceneController>.Instance.IsStoryLevel)
		{
			return;
		}
		itemBrokenCounter++;
		SaveController.Save("itemBrokenCounter", itemBrokenCounter, SaveData.Achievements);
		Debug.Log($"itemBrokenCounter = {itemBrokenCounter}");
		if (Singleton<SceneController>.Instance.GetCurrentLevelName().Contains("Kitchen"))
		{
			itemBrokenCounterKitchen++;
			SaveController.Save("itemBrokenCounterKitchen", itemBrokenCounterKitchen, SaveData.Achievements);
			Debug.Log($"itemBrokenCounterKitchen = {itemBrokenCounterKitchen}");
			if (itemBrokenCounter >= 20)
			{
				TryUnlockAchievement("Oops");
			}
		}
	}

	private static void AchievementEvents_OnVoidWalker()
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel)
		{
			TryUnlockAchievement("VoidWalker");
		}
	}

	private static void AchievementEvents_OnTryOpenDoor()
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel)
		{
			TryUnlockAchievement("Locked");
		}
	}

	private static void AchievementEvents_OnKitchenFullyFlooded()
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel)
		{
			TryUnlockAchievement("Waterworld");
		}
	}

	private static void AchievementEvents_OnCoinsCollected(int amount)
	{
		if (amount >= 100)
		{
			TryUnlockAchievement("ButtonKeeper");
		}
	}
}
