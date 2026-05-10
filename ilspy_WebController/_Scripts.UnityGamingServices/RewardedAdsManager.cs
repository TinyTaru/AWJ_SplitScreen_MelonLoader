using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudCode.GeneratedBindings.AWebbingJourneyCloud;
using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UnityConsent;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public class RewardedAdsManager : Singleton<RewardedAdsManager>
{
	[SerializeField]
	private InputActionAsset[] inputActionAssets;

	private List<InputActionMap> actionMaps;

	private const string androidAppKey = "2479890e5";

	private const string appleAppKey = "24798ca6d";

	private const float rewardCooldownSeconds = 3f;

	private bool cooldownActive;

	private AdRewardServiceBindings adRewardServiceBindings;

	private const string adUnitId_Android = "92fmd3xvrz59qteu";

	private const string adUnitId_iOS = "r020ean610mi0kni";

	private const string placementName = "Main_Menu";

	private bool sdkIsInitialized;

	private LevelPlayRewardedAd? rewardedAd;

	private string lastAdToken;

	private DateTime lastAdCompletionTime;

	private long timeStampAdStart;

	private bool adAvailable;

	private bool initializationFailed;

	private bool initialized;

	private Task? initTask;

	public bool AdAvailable => adAvailable;

	public bool InitializationFailed => initializationFailed;

	public bool CooldownActive => cooldownActive;

	public bool SDKIsInitialized => sdkIsInitialized;

	public event Action<bool>? OnAdSuccessfullyCompleted;

	public event Action<bool>? OnAdAvailable;

	public event Action? OnAdShown;

	public event Action? OnCooldownExpired;

	private void Start()
	{
		actionMaps = new List<InputActionMap>();
		InputActionAsset[] array = inputActionAssets;
		foreach (InputActionAsset inputActionAsset in array)
		{
			actionMaps.Add(inputActionAsset.FindActionMap("UI"));
		}
		RegisterSDKEvents();
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

	private void OnDestroy()
	{
		RemoveEventHandlers();
	}

	private IEnumerator CooldownCoroutine()
	{
		yield return new WaitForSecondsRealtime(3f);
		cooldownActive = false;
		this.OnCooldownExpired?.Invoke();
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
		if (initialized || !ConsentManager.LevelPlayConfigurationDone)
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
			ConsentState consentState = EndUserConsent.GetConsentState();
			consentState.AdsIntent = ConsentStatus.Denied;
			EndUserConsent.SetConsentState(consentState);
			adRewardServiceBindings = new AdRewardServiceBindings(CloudCodeService.Instance);
			string platformAppKey = GetPlatformAppKey();
			string playerId = AuthenticationService.Instance.PlayerId;
			LevelPlay.Init(platformAppKey, playerId);
			initializationFailed = false;
			initialized = true;
		}
		catch (CloudCodeException exception)
		{
			initializationFailed = true;
			Debug.LogException(exception);
		}
		catch (Exception exception2)
		{
			initializationFailed = true;
			Debug.LogException(exception2);
		}
	}

	private void OnWentOffline()
	{
	}

	private string GetPlatformAppKey()
	{
		Debug.LogWarning("Unexpected platform for ads");
		return "unexpected_platform";
	}

	private void RegisterSDKEvents()
	{
		LevelPlay.OnInitSuccess += LevelPlayer_OnInitSuccess;
		LevelPlay.OnInitFailed += LevelPlay_OnInitFailed;
	}

	private void SdkInitializationCompleted(LevelPlayConfiguration configuration)
	{
		if (!sdkIsInitialized)
		{
			sdkIsInitialized = true;
			Debug.Log("LevelPlay SDK initialized successfully.");
			CreateRewardedAd();
			RegisterToAdEvents();
			LoadRewardedAd();
		}
	}

	private void CreateRewardedAd()
	{
	}

	private void RegisterToAdEvents()
	{
		rewardedAd.OnAdLoaded += HandleAdLoadedSuccessfully;
		rewardedAd.OnAdLoadFailed += HandleAdLoadFailed;
		rewardedAd.OnAdDisplayed += HandleAdDisplayed;
		rewardedAd.OnAdDisplayFailed += HandleAdFailedToDisplay;
		rewardedAd.OnAdRewarded += ProcessAdReward;
		rewardedAd.OnAdClosed += HandleAdClosed;
		rewardedAd.OnAdClicked += HandleAdClicked;
		rewardedAd.OnAdInfoChanged += HandleAdInfoChanged;
	}

	private void RemoveEventHandlers()
	{
		LevelPlay.OnInitSuccess -= LevelPlayer_OnInitSuccess;
		LevelPlay.OnInitFailed -= LevelPlay_OnInitFailed;
		if (rewardedAd != null)
		{
			rewardedAd.OnAdLoaded -= HandleAdLoadedSuccessfully;
			rewardedAd.OnAdLoadFailed -= HandleAdLoadFailed;
			rewardedAd.OnAdDisplayed -= HandleAdDisplayed;
			rewardedAd.OnAdDisplayFailed -= HandleAdFailedToDisplay;
			rewardedAd.OnAdRewarded -= ProcessAdReward;
			rewardedAd.OnAdClosed -= HandleAdClosed;
			rewardedAd.OnAdClicked -= HandleAdClicked;
			rewardedAd.OnAdInfoChanged -= HandleAdInfoChanged;
		}
	}

	private void LoadRewardedAd()
	{
		if (rewardedAd != null)
		{
			rewardedAd.LoadAd();
		}
	}

	private void ShowRewardedAd()
	{
		timeStampAdStart = DateTime.UtcNow.Ticks;
		if (string.IsNullOrEmpty("Main_Menu"))
		{
			Debug.LogWarning("Placement name is empty, showing ad unity without placement");
			rewardedAd.ShowAd();
		}
		else
		{
			rewardedAd.ShowAd("Main_Menu");
		}
		this.OnAdShown?.Invoke();
	}

	private void LaunchTestSuite()
	{
		LevelPlay.LaunchTestSuite();
	}

	private void SdkInitializationFailed(LevelPlayInitError initError)
	{
		Debug.LogError("LevelPlay SDK initialization failed");
	}

	private float GetRemainingCooldownSeconds()
	{
		if (lastAdCompletionTime == default(DateTime))
		{
			return 0f;
		}
		return Math.Max(3f - (float)(DateTime.UtcNow - lastAdCompletionTime).TotalSeconds, 0f);
	}

	private bool HasCooldownExpired()
	{
		float remainingCooldownSeconds = GetRemainingCooldownSeconds();
		if (remainingCooldownSeconds > 0f)
		{
			Debug.Log($"Ad still on cooldown for {remainingCooldownSeconds:F1} seconds");
			return false;
		}
		return true;
	}

	public bool CanShowAd()
	{
		if (!sdkIsInitialized)
		{
			Debug.LogWarning("SDK not initialized");
			return false;
		}
		if (rewardedAd == null)
		{
			Debug.LogWarning("Rewarded ad object not created");
			return false;
		}
		bool num = rewardedAd.IsAdReady();
		bool flag = HasCooldownExpired();
		bool flag2 = LevelPlayRewardedAd.IsPlacementCapped("Main_Menu");
		if (!num)
		{
			Debug.LogWarning("Ad not ready - still loading or no inventory available");
		}
		if (flag2)
		{
			Debug.LogWarning("Placement has reached its capping limit");
		}
		if (num && flag)
		{
			return !flag2;
		}
		return false;
	}

	public void ShowAd()
	{
		if (CanShowAd())
		{
			ShowRewardedAd();
		}
		else
		{
			Debug.LogWarning("Cannot show ad.");
		}
	}

	public void InitializeAfterConsentForm()
	{
		HandleOnlineStateChanged(OnlineGate.State);
	}

	private void OnlineGate_OnStateChanged(OnlineState state)
	{
		HandleOnlineStateChanged(state);
	}

	private void LevelPlayer_OnInitSuccess(LevelPlayConfiguration configuration)
	{
		SdkInitializationCompleted(configuration);
	}

	private void LevelPlay_OnInitFailed(LevelPlayInitError initError)
	{
		SdkInitializationFailed(initError);
	}

	private void HandleAdLoadedSuccessfully(LevelPlayAdInfo adInfo)
	{
		adAvailable = true;
		this.OnAdAvailable?.Invoke(obj: true);
		Debug.Log("$Rewarded ad loaded");
	}

	private void HandleAdLoadFailed(LevelPlayAdError adError)
	{
		Debug.LogError("Rewarded ad failed to load");
		switch (adError.ErrorCode)
		{
		case 509:
			Debug.LogWarning("No ads to show, will retry");
			Invoke("LoadRewardedAd", 5f);
			break;
		case 520:
			Debug.LogWarning("No internet connection");
			adAvailable = false;
			this.OnAdAvailable?.Invoke(obj: false);
			break;
		case 524:
			Debug.LogWarning("Placement is capped, will no retry loading");
			adAvailable = false;
			this.OnAdAvailable?.Invoke(obj: false);
			break;
		case 526:
			Debug.LogWarning("Ad unity has reached daily cap, will not retry");
			adAvailable = false;
			this.OnAdAvailable?.Invoke(obj: false);
			break;
		default:
			Debug.LogWarning($"Unknown error code {adError.ErrorCode}, retrying with standard delay");
			Invoke("LoadRewardedAd", 2f);
			break;
		}
	}

	private void HandleAdDisplayed(LevelPlayAdInfo adInfo)
	{
		Debug.Log("Rewarded ad displayed");
		foreach (InputActionMap actionMap in actionMaps)
		{
			actionMap.Disable();
		}
		cooldownActive = true;
	}

	private void HandleAdFailedToDisplay(LevelPlayAdInfo adInfo, LevelPlayAdError adError)
	{
		Debug.LogError("Rewarded ad failed to display");
		this.OnAdSuccessfullyCompleted?.Invoke(obj: false);
	}

	private async void ProcessAdReward(LevelPlayAdInfo adInfo, LevelPlayReward reward)
	{
		try
		{
			Debug.Log("Validating ad reward");
			string adToken = GenerateAdToken(adInfo, reward);
			DateTime utcNow = DateTime.UtcNow;
			lastAdToken = adToken;
			lastAdCompletionTime = utcNow;
			StartCoroutine(CooldownCoroutine());
			PlaySessionData playSessionData = await adRewardServiceBindings.HandleGrantVideoAdReward(adToken);
			Singleton<PlaySessionManager>.Instance.HandlePlaySessionDataUpdate(playSessionData);
			this.OnAdSuccessfullyCompleted?.Invoke(obj: true);
			Debug.Log("Ad reward granted successfully");
		}
		catch (Exception)
		{
			Debug.LogError("Failed to validate ad reward");
			this.OnAdSuccessfullyCompleted?.Invoke(obj: false);
		}
	}

	private string GenerateAdToken(LevelPlayAdInfo adInfo, LevelPlayReward reward)
	{
		if (adInfo == null)
		{
			throw new ArgumentException("Ad info cannot be null for token generation");
		}
		if (reward == null)
		{
			throw new ArgumentException("Reward cannot be null for token generation");
		}
		string instanceId = (string.IsNullOrEmpty(adInfo.InstanceId) ? "unknown" : adInfo.InstanceId);
		string instanceName = (string.IsNullOrEmpty(adInfo.InstanceName) ? "unknown" : adInfo.InstanceName);
		return JsonConvert.SerializeObject(new
		{
			TimeStamp = DateTime.UtcNow.Ticks,
			InstanceId = instanceId,
			InstanceName = instanceName,
			AdNetwork = adInfo.AdNetwork,
			RewardName = reward.Name,
			RewardAmount = reward.Amount,
			TimeStampAdStart = timeStampAdStart
		});
	}

	private void HandleAdClosed(LevelPlayAdInfo adInfo)
	{
		Debug.Log("Rewarded ad closed");
		foreach (InputActionMap actionMap in actionMaps)
		{
			actionMap.Enable();
		}
		LoadRewardedAd();
	}

	private void HandleAdClicked(LevelPlayAdInfo adInfo)
	{
		Debug.Log("Rewarded ad clicked");
	}

	private void HandleAdInfoChanged(LevelPlayAdInfo adInfo)
	{
	}
}
