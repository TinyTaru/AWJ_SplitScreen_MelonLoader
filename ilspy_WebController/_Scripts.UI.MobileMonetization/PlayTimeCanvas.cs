using System;
using System.Collections;
using TMPro;
using Unity.Services.CloudCode.GeneratedBindings.AWebbingJourneyCloud;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.UI.Panels;
using _Scripts.UI.Scene_Loading;
using _Scripts.UnityGamingServices;

namespace _Scripts.UI.MobileMonetization;

public class PlayTimeCanvas : Singleton<PlayTimeCanvas>
{
	[SerializeField]
	private GameObject backgroundImage;

	[SerializeField]
	private RectTransform panel;

	[SerializeField]
	private TextMeshProUGUI remainingPlayTimeText;

	[SerializeField]
	private TextMeshProUGUI nextAdAvailableTimeText;

	[SerializeField]
	private TextMeshProUGUI rewardedAdText;

	[SerializeField]
	private TextMeshProUGUI unlockLevelText;

	[SerializeField]
	private GameObject quitCanvas;

	[Header("Sections")]
	[SerializeField]
	private GameObject noInternetSection;

	[SerializeField]
	private GameObject initializeDataSection;

	[SerializeField]
	private GameObject processingRewardSection;

	[SerializeField]
	private GameObject playTimeSection;

	[SerializeField]
	private GameObject loadingAdSection;

	[SerializeField]
	private GameObject nextAdAvailableInSection;

	[FormerlySerializedAs("AdsNotAllowedSection")]
	[SerializeField]
	private GameObject adsNotAllowedSection;

	[SerializeField]
	private GameObject watchAdSection;

	[SerializeField]
	private GameObject loadingShopSection;

	[SerializeField]
	private GameObject unlockLevelSection;

	[SerializeField]
	private GameObject loadHubLevelSection;

	[Header("Level Data")]
	[SerializeField]
	private LevelData mainMenuLevelData;

	[SerializeField]
	private LevelData hubLevelData;

	[SerializeField]
	private LevelData officeLevelData;

	[SerializeField]
	private LevelData kidsRoomLevelData;

	[Header("Buttons")]
	[SerializeField]
	private Button unlockLevelButton;

	[SerializeField]
	private Button watchAdButton;

	[Header("Price Texts")]
	[SerializeField]
	private TextMeshProUGUI levelPriceText;

	private LevelMusic currentLevel;

	private PanelManager panelManager;

	private string officeLevelPriceText;

	private string kidsRoomLevelPriceText;

	private bool isStoryMode;

	private bool loadLevel;

	private bool playerEconomyIsInitialized;

	private bool panelIsOpen;

	public bool IsOpen
	{
		get
		{
			if (!panelIsOpen)
			{
				if (quitCanvas != null)
				{
					return quitCanvas.activeSelf;
				}
				return false;
			}
			return true;
		}
	}

	public event Action Opened;

	public event Action Closed;

	protected override void Awake()
	{
		base.Awake();
	}

	private void OnEnable()
	{
		OnlineGate.OnStateChanged += OnlineGate_OnStateChanged;
		if (Singleton<IAPStoreManager>.Instance != null)
		{
			if (Singleton<IAPStoreManager>.Instance.ProductsAreFetched)
			{
				UpdatePriceTexts();
			}
			else
			{
				Singleton<IAPStoreManager>.Instance.ProductsFetched += IapStoreManager_OnProductsFetched;
			}
		}
		if (Singleton<PlayerEconomyManager>.Instance != null)
		{
			playerEconomyIsInitialized = Singleton<PlayerEconomyManager>.Instance.EconomyDataIsInitialized;
			Singleton<PlayerEconomyManager>.Instance.PlayerEconomyDataUpdated += PlayerEconomyManager_OnPlayerEconomyDataUpdated;
		}
		if (Singleton<RewardedAdsManager>.Instance != null)
		{
			Singleton<RewardedAdsManager>.Instance.OnAdAvailable += RewardedAdsManager_OnAdAvailable;
			Singleton<RewardedAdsManager>.Instance.OnCooldownExpired += RewardedAdsManager_OnCooldownExpired;
		}
		if (Singleton<PlaySessionManager>.Instance != null)
		{
			Singleton<PlaySessionManager>.Instance.OnPlaySessionTimeAdded += PLaySessionManager_OnPlaySessionTimeAdded;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionTimerUpdated += PlaySessionManager_OnPlaySessionTimerUpdated;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionExpired += PlaySessionManager_OnPlaySessionExpired;
			Singleton<PlaySessionManager>.Instance.OnAdAvailable += PlaySessionManager_OnAdAvailable;
		}
	}

	private void OnDisable()
	{
		OnlineGate.OnStateChanged -= OnlineGate_OnStateChanged;
		if (Singleton<IAPStoreManager>.Instance != null)
		{
			Singleton<IAPStoreManager>.Instance.ProductsFetched -= IapStoreManager_OnProductsFetched;
		}
		if (Singleton<PlayerEconomyManager>.Instance != null)
		{
			Singleton<PlayerEconomyManager>.Instance.PlayerEconomyDataUpdated -= PlayerEconomyManager_OnPlayerEconomyDataUpdated;
		}
		if (Singleton<RewardedAdsManager>.Instance != null)
		{
			Singleton<RewardedAdsManager>.Instance.OnAdAvailable -= RewardedAdsManager_OnAdAvailable;
			Singleton<RewardedAdsManager>.Instance.OnCooldownExpired -= RewardedAdsManager_OnCooldownExpired;
		}
		if (Singleton<PlaySessionManager>.Instance != null)
		{
			Singleton<PlaySessionManager>.Instance.OnPlaySessionTimeAdded -= PLaySessionManager_OnPlaySessionTimeAdded;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionTimerUpdated -= PlaySessionManager_OnPlaySessionTimerUpdated;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionExpired -= PlaySessionManager_OnPlaySessionExpired;
			Singleton<PlaySessionManager>.Instance.OnAdAvailable -= PlaySessionManager_OnAdAvailable;
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
	}

	private IEnumerator HandleConsentClosedNextFrame()
	{
		yield return new WaitUntil(() => Application.isFocused);
		yield return null;
		UpdateUI();
		Singleton<RewardedAdsManager>.Instance.InitializeAfterConsentForm();
	}

	private void UpdateUI()
	{
	}

	private void UpdatePriceTexts()
	{
	}

	private void LoadLevelIfApplicable()
	{
	}

	private static string FormatRemainingPlayTime(TimeSpan timeSpan)
	{
		if (!(timeSpan.TotalMinutes >= 1.0))
		{
			return $"{timeSpan.Seconds} sec";
		}
		return $"{Mathf.CeilToInt((float)timeSpan.Minutes + (float)timeSpan.Seconds / 60f)} min";
	}

	public void Open(LevelMusic level, bool newIsStoryMode = false)
	{
	}

	public void Close()
	{
	}

	public void ShowAd()
	{
	}

	public void UnlockCurrentLevel()
	{
	}

	public void LoadHubLevel()
	{
		Singleton<SceneController>.Instance.LoadSpecificStoryLevel(hubLevelData.sceneName);
	}

	public void OpenConsentForm()
	{
		ConsentManager.LevelPlayConfigured = (Action<bool>)Delegate.Combine(ConsentManager.LevelPlayConfigured, new Action<bool>(LevelPlayConfigured));
		ConsentManager.OpenPrivacySettings();
	}

	private void LevelPlayConfigured(bool configurationSuccessful)
	{
		ConsentManager.LevelPlayConfigured = (Action<bool>)Delegate.Remove(ConsentManager.LevelPlayConfigured, new Action<bool>(LevelPlayConfigured));
		if (configurationSuccessful)
		{
			StartCoroutine(HandleConsentClosedNextFrame());
		}
	}

	public void DontQuitLevel()
	{
		if (quitCanvas != null)
		{
			quitCanvas.SetActive(value: false);
		}
		panel.gameObject.SetActive(value: true);
		UpdateUI();
	}

	public void ResetIsOpen()
	{
		panelIsOpen = false;
	}

	private void OnlineGate_OnStateChanged(OnlineState obj)
	{
	}

	private void IapStoreManager_OnProductsFetched()
	{
	}

	private void IapStoreManager_OnSuccessfullyPurchased(string message)
	{
	}

	private void IapStoreManager_OnPurchaseFailed(string obj)
	{
	}

	private void PlayerEconomyManager_OnPlayerEconomyDataUpdated(PlayerEconomyData playerEconomyData)
	{
	}

	private void RewardedAdsManager_OnAdAvailable(bool value)
	{
	}

	private void RewardedAdsManager_OnCooldownExpired()
	{
	}

	private void RewardedAdsManager_OnAdSuccessfullyCompleted(bool success)
	{
	}

	private void PlaySessionManager_OnAdAvailable()
	{
	}

	private void PlaySessionManager_OnPlaySessionTimerUpdated(TimeSpan timeSpan)
	{
	}

	private void PLaySessionManager_OnPlaySessionTimeAdded()
	{
	}

	private void PlaySessionManager_OnPlaySessionExpired()
	{
	}
}
