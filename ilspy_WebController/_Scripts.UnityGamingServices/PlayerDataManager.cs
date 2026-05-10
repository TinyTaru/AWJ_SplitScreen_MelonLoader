using System;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudCode.GeneratedBindings.AWebbingJourneyCloud;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public class PlayerDataManager : Singleton<PlayerDataManager>
{
	private PlayerDataServiceBindings bindings;

	private PlayerData localPlayerData;

	private bool initialized;

	private Task? initTask;

	public event Action<PlayerData> PlayerDataUpdated;

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
			bindings = new PlayerDataServiceBindings(CloudCodeService.Instance);
			PlayerDataResponse playerDataResponse = await bindings.HandlePlayerSignIn();
			localPlayerData = playerDataResponse.PlayerData;
			this.PlayerDataUpdated?.Invoke(localPlayerData);
			Singleton<PlaySessionManager>.Instance.HandlePlaySessionDataUpdate(playerDataResponse.PlaySessionData);
			Singleton<PlayerEconomyManager>.Instance.HandleEconomyUpdate(playerDataResponse.PlayerEconomyData);
			LogResponse(playerDataResponse);
			initialized = true;
		}
		catch (CloudCodeException exception)
		{
			Debug.LogException(exception);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	private void OnWentOffline()
	{
	}

	private void LogResponse(PlayerDataResponse playerDataResponse)
	{
	}

	private void OnlineGate_OnStateChanged(OnlineState state)
	{
		HandleOnlineStateChanged(state);
	}
}
