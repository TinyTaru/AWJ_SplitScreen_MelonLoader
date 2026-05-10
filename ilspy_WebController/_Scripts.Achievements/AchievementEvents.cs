using System;
using _Scripts.General;
using _Scripts.Objects;

namespace _Scripts.Achievements;

public static class AchievementEvents
{
	public static event Action OnCosmeticItemChanged;

	public static event Action OnCosmeticItemUnlocked;

	public static event Action OnSpeedySpinner;

	public static event Action<int> OnCoinsCollected;

	public static event Action<int> OnDistanceCoveredFlying;

	public static event Action<float> OnWebBuildDistance;

	public static event Action OnOutsideRoom;

	public static event Action<int> OnTalkToNpc;

	public static event Action OnTryToOpenDoor;

	public static event Action OnWebBuild;

	public static event Action OnWebDeleted;

	public static event Action OnArachnophobiaMode;

	public static event Action OnFluffinessChanged;

	public static event Action OnJumpIntoVoid;

	public static event Action<StoryLevel, int> OnTasksFinished;

	public static event Action OnTutorialFinished;

	public static event Action OnItemBroken;

	public static event Action OnChristmasEventStarted;

	public static event Action OnHalloweenEventStarted;

	public static event Action<CookingRecipeSO> OnRecipeCooked;

	public static event Action OnKitchenFullyFlooded;

	public static event Action<int> OnHouseModelBuilt;

	public static event Action<int> OnHighScoreComputerGame;

	public static event Action OnBiggestFan;

	public static event Action<int> On3DModelPrinted;

	public static event Action OnSlidingPuzzleFinished;

	public static event Action OnRocketRidden;

	public static event Action<int> OnTargetHit;

	public static event Action<int> OnPlushieCompleted;

	public static event Action OnHappyBirthdayPlayed;

	public static void CosmeticItemChanged()
	{
		AchievementEvents.OnCosmeticItemChanged?.Invoke();
	}

	public static void CosmeticItemUnlocked()
	{
		AchievementEvents.OnCosmeticItemUnlocked?.Invoke();
	}

	public static void SpeedySpinner()
	{
		AchievementEvents.OnSpeedySpinner?.Invoke();
	}

	public static void CoinsCollected(int total)
	{
		AchievementEvents.OnCoinsCollected?.Invoke(total);
	}

	public static void DistanceCoveredFlying(int obj)
	{
		AchievementEvents.OnDistanceCoveredFlying?.Invoke(obj);
	}

	public static void WebBuildDistance(float obj)
	{
		AchievementEvents.OnWebBuildDistance?.Invoke(obj);
	}

	public static void OutsideRoom()
	{
		AchievementEvents.OnOutsideRoom?.Invoke();
	}

	public static void TalkToNpc(int obj)
	{
		AchievementEvents.OnTalkToNpc?.Invoke(obj);
	}

	public static void TryToOpenDoor()
	{
		AchievementEvents.OnTryToOpenDoor?.Invoke();
	}

	public static void WebBuild()
	{
		AchievementEvents.OnWebBuild?.Invoke();
	}

	public static void WebDeleted()
	{
		AchievementEvents.OnWebDeleted?.Invoke();
	}

	public static void ArachnophobiaMode()
	{
		AchievementEvents.OnArachnophobiaMode?.Invoke();
	}

	public static void FluffinessChanged()
	{
		AchievementEvents.OnFluffinessChanged?.Invoke();
	}

	public static void JumpIntoVoid()
	{
		AchievementEvents.OnJumpIntoVoid?.Invoke();
	}

	public static void TasksFinished(StoryLevel arg1, int arg2)
	{
		AchievementEvents.OnTasksFinished?.Invoke(arg1, arg2);
	}

	public static void TutorialFinished()
	{
		AchievementEvents.OnTutorialFinished?.Invoke();
	}

	public static void ItemBroken()
	{
		AchievementEvents.OnItemBroken?.Invoke();
	}

	public static void ChristmasEventStarted()
	{
		AchievementEvents.OnChristmasEventStarted?.Invoke();
	}

	public static void HalloweenEventStarted()
	{
		AchievementEvents.OnHalloweenEventStarted?.Invoke();
	}

	public static void RecipeCooked(CookingRecipeSO obj)
	{
		AchievementEvents.OnRecipeCooked?.Invoke(obj);
	}

	public static void KitchenFullyFlooded()
	{
		AchievementEvents.OnKitchenFullyFlooded?.Invoke();
	}

	public static void HouseModelBuilt(int obj)
	{
		AchievementEvents.OnHouseModelBuilt?.Invoke(obj);
	}

	public static void HighScoreComputerGame(int obj)
	{
		AchievementEvents.OnHighScoreComputerGame?.Invoke(obj);
	}

	public static void BiggestFan()
	{
		AchievementEvents.OnBiggestFan?.Invoke();
	}

	public static void ModelPrinted3D(int obj)
	{
		AchievementEvents.On3DModelPrinted?.Invoke(obj);
	}

	public static void SlidingPuzzleFinished()
	{
		AchievementEvents.OnSlidingPuzzleFinished?.Invoke();
	}

	public static void RocketRidden()
	{
		AchievementEvents.OnRocketRidden?.Invoke();
	}

	public static void TargetHit(int obj)
	{
		AchievementEvents.OnTargetHit?.Invoke(obj);
	}

	public static void PlushieCompleted(int obj)
	{
		AchievementEvents.OnPlushieCompleted?.Invoke(obj);
	}

	public static void HappyBirthdayPlayed()
	{
		AchievementEvents.OnHappyBirthdayPlayed?.Invoke();
	}
}
