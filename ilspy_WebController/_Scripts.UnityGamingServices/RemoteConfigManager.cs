using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Services.RemoteConfig;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public class RemoteConfigManager : Singleton<RemoteConfigManager>
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct UserAttributes
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct AppAttributes
	{
	}

	private const string rewardedAdPlaytimeMinutesId = "REWARDED_AD_PLAYTIME_MINUTES";

	private const int defaultRewardedAdPlaytimeMinutes = 10;

	private int rewardedAdPlaytimeMinutes;

	private bool initialized;

	private Task? initTask;

	public int RewardedAdPlaytimeMinutes => rewardedAdPlaytimeMinutes;

	protected override void Awake()
	{
		base.Awake();
		rewardedAdPlaytimeMinutes = 10;
	}

	private void OnEnable()
	{
		RemoteConfigService.Instance.FetchCompleted += RemoteConfigService_OnFetchCompleted;
		OnlineGate.OnStateChanged += OnlineGate_OnStateChanged;
		HandleOnlineStateChanged(OnlineGate.State);
	}

	private void OnDisable()
	{
		RemoteConfigService.Instance.FetchCompleted += RemoteConfigService_OnFetchCompleted;
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
			await RemoteConfigService.Instance.FetchConfigsAsync(default(UserAttributes), default(AppAttributes));
			initialized = true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void OnWentOffline()
	{
	}

	private void OnlineGate_OnStateChanged(OnlineState state)
	{
		HandleOnlineStateChanged(state);
	}

	private void RemoteConfigService_OnFetchCompleted(ConfigResponse obj)
	{
		rewardedAdPlaytimeMinutes = RemoteConfigService.Instance.appConfig.GetInt("REWARDED_AD_PLAYTIME_MINUTES", 10);
		if (Singleton<PlaySessionManager>.Instance != null)
		{
			Singleton<PlaySessionManager>.Instance.SetMinutesForAdAvailable(rewardedAdPlaytimeMinutes);
		}
	}
}
