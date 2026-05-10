using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudCode.GeneratedBindings.AWebbingJourneyCloud;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public class PlayerEconomyManager : Singleton<PlayerEconomyManager>
{
	public enum Level
	{
		Office,
		KidsRoom
	}

	private bool economyDataIsInitialized;

	private PlayerEconomyServiceBindings economyServiceBindings;

	private PlayerEconomyData localPlayerEconomyData = new PlayerEconomyData
	{
		Currencies = new Dictionary<string, int>(),
		ItemInventory = new Dictionary<string, int>()
	};

	public const string CoffeeCurrencyId = "COFFEE";

	public const string LevelUnlockedAllId = "LEVEL_UNLOCKED_ALL";

	public const string LevelUnlockedOfficeId = "LEVEL_UNLOCKED_OFFICE";

	public const string LevelUnlockedKidsRoomId = "LEVEL_UNLOCKED_KIDS_ROOM";

	public const string LevelUnlockAllId = "LEVEL_UNLOCK_ALL";

	public const string LevelUnlockOfficeId = "LEVEL_UNLOCK_OFFICE";

	public const string LevelUnlockKidsRoomId = "LEVEL_UNLOCK_KIDS_ROOM";

	public const string Coffee1Id = "COFFEE_1";

	public const string Coffee2Id = "COFFEE_2";

	public const string Coffee3Id = "COFFEE_3";

	public static Dictionary<string, string> ProductToInventoryItemMapping;

	private bool initialized;

	private bool initializationFailed;

	private Task? initTask;

	public bool EconomyDataIsInitialized => economyDataIsInitialized;

	public int Coffee => GetCurrencyAmount("COFFEE");

	public bool InitializationFailed => initializationFailed;

	public event Action<PlayerEconomyData>? PlayerEconomyDataUpdated;

	public event Action? EconomyConfigSynced;

	public event Action? EconomyDataInitialized;

	protected override void Awake()
	{
		base.Awake();
		ProductToInventoryItemMapping = new Dictionary<string, string>
		{
			{ "LEVEL_UNLOCK_ALL", "LEVEL_UNLOCKED_ALL" },
			{ "LEVEL_UNLOCK_OFFICE", "LEVEL_UNLOCKED_OFFICE" },
			{ "LEVEL_UNLOCK_KIDS_ROOM", "LEVEL_UNLOCKED_KIDS_ROOM" }
		};
	}

	private void OnEnable()
	{
		OnlineGate.OnStateChanged += OnlineGate_OnStateChanged;
		HandleOnlineStateChanged(OnlineGate.State);
	}

	private void OnDisable()
	{
		OnlineGate.OnStateChanged -= OnlineGate_OnStateChanged;
	}

	private void HandleOnlineStateChanged(OnlineState state)
	{
		switch (state)
		{
		case OnlineState.OnlineReady:
			EnsureInitializedAsync();
			break;
		case OnlineState.Offline:
			OnWentOffline();
			break;
		}
	}

	private async Task EnsureInitializedAsync()
	{
		if (initialized)
		{
			return;
		}
		if (initTask == null)
		{
			initTask = InitializeInternalAsync();
			try
			{
				await initTask;
				return;
			}
			finally
			{
				initTask = null;
			}
		}
		await initTask;
	}

	private async Task InitializeInternalAsync()
	{
		try
		{
			economyServiceBindings = new PlayerEconomyServiceBindings(CloudCodeService.Instance);
			await EconomyService.Instance.Configuration.SyncConfigurationAsync();
			Debug.Log("Economy configuration synced");
			initializationFailed = false;
			initialized = true;
			this.EconomyConfigSynced?.Invoke();
		}
		catch (Exception exception)
		{
			initializationFailed = true;
			Debug.LogException(exception);
		}
	}

	private void OnWentOffline()
	{
	}

	public async Task DeleteInventoryItem(string itemId)
	{
		string itemId2 = itemId;
		try
		{
			GetInventoryResult getInventoryResult = await EconomyService.Instance.PlayerInventory.GetInventoryAsync();
			if (getInventoryResult == null || getInventoryResult.PlayersInventoryItems.Count == 0)
			{
				Debug.Log("No player inventory items found!");
				return;
			}
			Debug.Log("Player Inventory Items:");
			foreach (PlayersInventoryItem playersInventoryItem in getInventoryResult.PlayersInventoryItems)
			{
				_ = playersInventoryItem;
			}
			PlayersInventoryItem[] array = getInventoryResult.PlayersInventoryItems.Where((PlayersInventoryItem item) => item.InventoryItemId == itemId2).ToArray();
			PlayersInventoryItem[] array2 = array;
			foreach (PlayersInventoryItem obj in array2)
			{
				string playersInventoryItemId = obj.PlayersInventoryItemId;
				string writeLock = obj.WriteLock;
				DeletePlayersInventoryItemOptions options = new DeletePlayersInventoryItemOptions
				{
					WriteLock = writeLock
				};
				await EconomyService.Instance.PlayerInventory.DeletePlayersInventoryItemAsync(playersInventoryItemId, options);
			}
			(await EconomyService.Instance.PlayerInventory.GetInventoryAsync()).PlayersInventoryItems.Count((PlayersInventoryItem item) => item.InventoryItemId == itemId2);
			HandleEconomyUpdate(await economyServiceBindings.GetPlayerEconomyData());
		}
		catch (EconomyException)
		{
			Debug.LogError("Economy delete failed");
			throw;
		}
		catch (Exception)
		{
			Debug.LogError("Unexpected error");
			throw;
		}
	}

	public void HandleEconomyUpdate(PlayerEconomyData playerEconomyData)
	{
		localPlayerEconomyData = playerEconomyData;
		if (!economyDataIsInitialized)
		{
			economyDataIsInitialized = true;
			this.EconomyDataInitialized?.Invoke();
		}
		this.PlayerEconomyDataUpdated?.Invoke(localPlayerEconomyData);
	}

	public int GetCurrencyAmount(string currencyKey)
	{
		return localPlayerEconomyData.Currencies.GetValueOrDefault(currencyKey, 0);
	}

	public void SetLevelUnlocked(Level level, bool value)
	{
		string key;
		switch (level)
		{
		default:
			return;
		case Level.Office:
			key = "LEVEL_UNLOCKED_OFFICE";
			break;
		case Level.KidsRoom:
			key = "LEVEL_UNLOCKED_KIDS_ROOM";
			break;
		}
		localPlayerEconomyData.ItemInventory[key] = (value ? 1 : 0);
		this.PlayerEconomyDataUpdated?.Invoke(localPlayerEconomyData);
	}

	public bool GetAllLevelsUnlocked()
	{
		return localPlayerEconomyData.ItemInventory.GetValueOrDefault("LEVEL_UNLOCKED_ALL", 0) > 0;
	}

	public bool GetLevelUnlocked(Level level)
	{
		if (GetAllLevelsUnlocked())
		{
			return true;
		}
		string key = string.Empty;
		switch (level)
		{
		case Level.Office:
			key = "LEVEL_UNLOCKED_OFFICE";
			break;
		case Level.KidsRoom:
			key = "LEVEL_UNLOCKED_KIDS_ROOM";
			break;
		}
		return localPlayerEconomyData.ItemInventory.GetValueOrDefault(key, 0) > 0;
	}

	public int GetInventoryItemAmount(string itemId)
	{
		return localPlayerEconomyData.ItemInventory.GetValueOrDefault(itemId, 0);
	}

	public void DeleteOfficeLevelUnlockedInventoryItem()
	{
		DeleteInventoryItem("LEVEL_UNLOCKED_OFFICE");
	}

	public void DeleteKidsRoomLevelUnlockedInventoryItem()
	{
		DeleteInventoryItem("LEVEL_UNLOCKED_KIDS_ROOM");
	}

	public Dictionary<string, int> GetInventoryItems()
	{
		return localPlayerEconomyData.ItemInventory;
	}

	private void OnlineGate_OnStateChanged(OnlineState state)
	{
		HandleOnlineStateChanged(state);
	}
}
