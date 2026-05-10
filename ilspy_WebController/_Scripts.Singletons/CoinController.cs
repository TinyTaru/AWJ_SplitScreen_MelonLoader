using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Scripts.Achievements;
using _Scripts.General;
using _Scripts.Interactable;

namespace _Scripts.Singletons;

public class CoinController : Singleton<CoinController>
{
	public class OnCoinAmountChangedEventArgs : EventArgs
	{
		public int coinAmount;

		public bool coinsAdded;
	}

	private int totalCoinAmount;

	private int currentCoinAmount;

	private bool[] collectedCoinsArray;

	private const int totalCoinsPerLevel = 100;

	public int CurrentCoinAmount => currentCoinAmount;

	public int TotalCoinsPerLevel => 100;

	public event Action OnAllCoinsCollectedInCurrentLevel;

	public event Action<OnCoinAmountChangedEventArgs> OnCoinAmountChanged;

	protected override void Awake()
	{
		base.Awake();
		LoadCoinData();
	}

	private void Start()
	{
		if (Singleton<TaskListUI>.Instance != null)
		{
			Singleton<TaskListUI>.Instance.OnTaskCompleted += TaskListUI_OnTaskCompleted;
		}
		InteractableCoin[] array = UnityEngine.Object.FindObjectsByType<InteractableCoin>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnCoinCollected += Coin_OnCoinCollected;
		}
	}

	private void LoadCoinData()
	{
		Singleton<CoinController>.Instance.collectedCoinsArray = SaveController.Load(SceneManager.GetActiveScene().name + "_CollectedCoinsArray", new bool[100], SaveData.Game);
		Singleton<CoinController>.Instance.totalCoinAmount = SaveController.Load("TotalCoinAmount", 0, SaveData.Game);
		Singleton<CoinController>.Instance.currentCoinAmount = SaveController.Load("CurrentCoinAmount", 0, SaveData.Game);
	}

	private void SaveCoinData()
	{
		SaveController.Save("TotalCoinAmount", Singleton<CoinController>.Instance.totalCoinAmount, SaveData.Game);
		SaveController.Save("CurrentCoinAmount", Singleton<CoinController>.Instance.currentCoinAmount, SaveData.Game);
		SaveController.Save(SceneManager.GetActiveScene().name + "_CollectedCoinsArray", Singleton<CoinController>.Instance.collectedCoinsArray, SaveData.Game);
	}

	private void SetCoins(int amount)
	{
		Singleton<CoinController>.Instance.currentCoinAmount = amount;
		SaveCoinData();
		this.OnCoinAmountChanged?.Invoke(new OnCoinAmountChangedEventArgs
		{
			coinAmount = Singleton<CoinController>.Instance.currentCoinAmount,
			coinsAdded = true
		});
	}

	private int CheckCoins()
	{
		return Singleton<CoinController>.Instance.currentCoinAmount;
	}

	private void AddCollectibleCoin(int coinId, int amount)
	{
		if (!Singleton<SceneController>.Instance.IsStoryLevel)
		{
			return;
		}
		if (amount <= 0)
		{
			Debug.LogError("Adding zero or negative coins is not allowed. Please check the coin source.");
			return;
		}
		if (collectedCoinsArray[coinId])
		{
			Debug.LogError("Coin already collected on this profile.");
			return;
		}
		Singleton<CoinController>.Instance.collectedCoinsArray[coinId] = true;
		Singleton<CoinController>.Instance.totalCoinAmount += amount;
		Singleton<CoinController>.Instance.currentCoinAmount += amount;
		SaveCoinData();
		this.OnCoinAmountChanged?.Invoke(new OnCoinAmountChangedEventArgs
		{
			coinAmount = Singleton<CoinController>.Instance.currentCoinAmount,
			coinsAdded = true
		});
		AchievementEvents.CoinsCollected(Singleton<CoinController>.Instance.totalCoinAmount);
		if (GetCollectedCoinAmountInCurrentLevel() == 100)
		{
			this.OnAllCoinsCollectedInCurrentLevel?.Invoke();
		}
	}

	private void AddCoinsFromQuestReward(int amount)
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel && amount != 0)
		{
			Singleton<CoinController>.Instance.totalCoinAmount += amount;
			Singleton<CoinController>.Instance.currentCoinAmount += amount;
			SaveCoinData();
			this.OnCoinAmountChanged?.Invoke(new OnCoinAmountChangedEventArgs
			{
				coinAmount = Singleton<CoinController>.Instance.currentCoinAmount,
				coinsAdded = true
			});
			AchievementEvents.CoinsCollected(Singleton<CoinController>.Instance.totalCoinAmount);
		}
	}

	public void AddCoins(int amount)
	{
		Singleton<CoinController>.Instance.totalCoinAmount += amount;
		Singleton<CoinController>.Instance.currentCoinAmount += amount;
		SaveCoinData();
		this.OnCoinAmountChanged?.Invoke(new OnCoinAmountChangedEventArgs
		{
			coinAmount = Singleton<CoinController>.Instance.currentCoinAmount,
			coinsAdded = true
		});
	}

	public bool CheckIfEnoughCoins(int amount)
	{
		return Singleton<CoinController>.Instance.currentCoinAmount >= amount;
	}

	public int GetCoins()
	{
		return Singleton<CoinController>.Instance.currentCoinAmount;
	}

	public void UseCoins(int amount)
	{
		Singleton<CoinController>.Instance.currentCoinAmount -= amount;
		SaveCoinData();
		this.OnCoinAmountChanged?.Invoke(new OnCoinAmountChangedEventArgs
		{
			coinAmount = Singleton<CoinController>.Instance.currentCoinAmount,
			coinsAdded = false
		});
	}

	public bool CoinAlreadyCollected(int coinId)
	{
		return Singleton<CoinController>.Instance.collectedCoinsArray[coinId];
	}

	public int GetCollectedCoinAmountInCurrentLevel()
	{
		return Singleton<CoinController>.Instance.collectedCoinsArray.Count((bool x) => x);
	}

	private void TaskListUI_OnTaskCompleted(object sender, TaskListUI.OnTaskCompletedEventArgs e)
	{
		AddCoinsFromQuestReward(e.taskDataSO.coinAmount);
	}

	private void Coin_OnCoinCollected(object sender, InteractableCoin.OnCoinCollectedEventArgs e)
	{
		AddCollectibleCoin(e.coindId, e.coinAmount);
	}
}
