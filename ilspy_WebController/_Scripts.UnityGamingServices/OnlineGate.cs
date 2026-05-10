using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.UnityConsent;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public static class OnlineGate
{
	private static string environment;

	private static readonly object sync = new object();

	private static bool coreInitialized = false;

	private static bool signedIn = false;

	private static Task<bool>? inFlight;

	private static string googlePlayGamesToken;

	private static readonly object appleSync = new object();

	private static Task<bool>? appleAuthInFlight;

	public static OnlineState State { get; private set; } = OnlineState.Offline;


	public static string? LastError { get; private set; }

	public static event Action<OnlineState>? OnStateChanged;

	private static async Task<bool> EnsureInternalAsync(int timeoutMs)
	{
		_ = 1;
		try
		{
			RefreshNetworkState();
			if (State == OnlineState.Offline)
			{
				return false;
			}
			if (State == OnlineState.OnlineReady && UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
			{
				SetStateIfChanged(OnlineState.OnlineReady, null);
				return true;
			}
			bool num = UnityServices.State == ServicesInitializationState.Uninitialized;
			bool flag = false;
			if (!num && UnityServices.State == ServicesInitializationState.Initialized)
			{
				flag = !AuthenticationService.Instance.IsSignedIn;
			}
			if (num || flag)
			{
				SetStateIfChanged(OnlineState.Initializing, null);
			}
			if (num)
			{
				if (!(await WithTimeout(UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName(environment)), timeoutMs)))
				{
					SetStateIfChanged(OnlineState.Degraded, "UGS init timeout");
					return false;
				}
				ConsentState consentState = EndUserConsent.GetConsentState();
				consentState.AnalyticsIntent = (SaveController.Load("AllowAnalytics", defaultValue: false, SaveData.Settings) ? ConsentStatus.Granted : ConsentStatus.Denied);
				EndUserConsent.SetConsentState(consentState);
				coreInitialized = true;
			}
			else
			{
				coreInitialized = UnityServices.State == ServicesInitializationState.Initialized;
			}
			if (!coreInitialized)
			{
				SetStateIfChanged(OnlineState.Degraded, "UGS core not initialized");
				return false;
			}
			if (!AuthenticationService.Instance.IsSignedIn)
			{
				if (!(await WithTimeout(AuthenticationService.Instance.SignInAnonymouslyAsync(), timeoutMs)))
				{
					SetStateIfChanged(OnlineState.Degraded, "Auth sign-in failed or timed out");
					return false;
				}
				Debug.Log("Auth sign-in successful");
				Debug.Log("PlayerID: " + AuthenticationService.Instance.PlayerId);
			}
			signedIn = AuthenticationService.Instance.IsSignedIn;
			if (!signedIn)
			{
				SetStateIfChanged(OnlineState.Degraded, "Auth sign-in failed");
				return false;
			}
			SetStateIfChanged(OnlineState.OnlineReady, null);
			return true;
		}
		catch (Exception ex)
		{
			SetStateIfChanged(OnlineState.Degraded, ex.Message);
			return false;
		}
	}

	private static async Task<bool> WithTimeout(Task task, int timeoutMs)
	{
		if (await Task.WhenAny(task, Task.Delay(timeoutMs)) != task)
		{
			return false;
		}
		await task;
		return true;
	}

	private static async Task<T?> WithTimeout<T>(Task<T> task, int timeoutMs) where T : class
	{
		if (await Task.WhenAny(task, Task.Delay(timeoutMs)) != task)
		{
			return null;
		}
		return await task;
	}

	private static async Task<bool> WithTimeoutBool(Task<bool> task, int timeoutMs)
	{
		if (await Task.WhenAny(task, Task.Delay(timeoutMs)) != task)
		{
			return false;
		}
		return await task;
	}

	private static void SetStateIfChanged(OnlineState newState, string? error)
	{
		Debug.Log($"State = {newState}");
		if (newState == OnlineState.OnlineReady || State != newState || !(LastError == error))
		{
			string arg = (string.IsNullOrEmpty(error) ? "" : (" with error: " + error));
			Debug.Log($"[OnlineGate] New Online State: {newState}{arg}");
			State = newState;
			LastError = error;
			OnlineGate.OnStateChanged?.Invoke(newState);
		}
	}

	private static void RefreshNetworkState()
	{
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			SetStateIfChanged(OnlineState.Offline, "No network interface");
		}
		else if (State == OnlineState.Offline)
		{
			SetStateIfChanged(OnlineState.Degraded, null);
		}
	}

	public static void Initialize(string newEnvironment)
	{
	}

	public static Task<bool> EnsureOnlineReadyAsync(float timeoutSeconds = 8f)
	{
		return Task.FromResult(result: true);
	}
}
