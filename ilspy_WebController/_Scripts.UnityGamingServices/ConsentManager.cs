using System;
using System.Collections.Generic;
using Unity.Services.LevelPlay;
using Unity.Usercentrics;
using UnityEngine;
using UnityEngine.UnityConsent;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public static class ConsentManager
{
	public static Action<bool> LevelPlayConfigured;

	private static bool isChild = true;

	private static bool canGiveConsent = false;

	private static readonly Dictionary<string, string> marketingTemplateIdsToDpsNames = new Dictionary<string, string>
	{
		{ "hpb62D82I", "Unity Ads" },
		{ "9dchbL797", "ironSource" }
	};

	private static readonly BannerSettings bannerSettings = new BannerSettings
	{
		firstLayerStyleSettings = new FirstLayerStyleSettings(UsercentricsLayout.PopupCenter),
		secondLayerStyleSettings = new SecondLayerStyleSettings(showCloseButton: true)
	};

	public static bool ShowPrivacySettingsButton { get; private set; } = true;


	public static bool LevelPlayConfigurationDone { get; private set; } = false;


	private static void LogStatusAndConsent(UsercentricsReadyStatus status, string label = "")
	{
		Debug.Log(label ?? "");
		Debug.Log("[CMP] status | " + $"shouldCollectConsent={status.shouldCollectConsent} | " + "location.countryCode=" + status.location.countryCode + " | geolocationRuleset.activeSettingsId=" + status.geolocationRuleset.activeSettingsId + " | " + $"geolocationRuleset.bannerRequiredAtLocation={status.geolocationRuleset.bannerRequiredAtLocation}");
		foreach (UsercentricsServiceConsent consent in status.consents)
		{
			Debug.Log("[CMP] consent | templateId=" + consent.templateId + " | dataProcessor=" + consent.dataProcessor + " | " + $"status={consent.status}");
		}
	}

	private static void LogConsent(List<UsercentricsServiceConsent> consents, string label = "")
	{
		Debug.Log(label ?? "");
		foreach (UsercentricsServiceConsent consent in consents)
		{
			Debug.Log("[CMP] consent | templateId=" + consent.templateId + " | dataProcessor=" + consent.dataProcessor + " | " + $"status={consent.status}");
		}
	}

	private static void InitializeUsercentrics()
	{
		Debug.Log("[CMP] Initializing Usercentrics...");
		Unity.Usercentrics.Singleton<Usercentrics>.Instance.Initialize(delegate(UsercentricsReadyStatus status)
		{
			if (!status.geolocationRuleset.bannerRequiredAtLocation)
			{
				ShowPrivacySettingsButton = false;
				Debug.Log("[CMP] No banner required");
				ApplyConsentToLevelPlay(canGiveConsent);
				Unity.Usercentrics.Singleton<Usercentrics>.Instance.GetConsents();
			}
			else if (status.shouldCollectConsent)
			{
				ShowFirstLayer();
			}
			else
			{
				ApplyConsentToLevelPlay(canGiveConsent && DetermineMarketingAcceptedBasedOnConsent(status.consents));
				Unity.Usercentrics.Singleton<Usercentrics>.Instance.GetConsents();
			}
		}, delegate(string errorMessage)
		{
			Debug.LogError("[CMP] Usercentrics init failed: " + errorMessage);
			LevelPlayConfigurationDone = false;
			LevelPlayConfigured?.Invoke(obj: false);
			if (Unity.Usercentrics.Singleton<Usercentrics>.Instance != null)
			{
				Unity.Usercentrics.Singleton<Usercentrics>.Instance.GetConsents();
			}
		});
	}

	private static void ShowFirstLayer()
	{
		Debug.Log("[CMP] Showing Usercentrics First Layer...");
		Unity.Usercentrics.Singleton<Usercentrics>.Instance.ShowFirstLayer(bannerSettings, delegate(UsercentricsConsentUserResponse userResponse)
		{
			bool personalizedAdsAllowed = DetermineMarketingAcceptedBasedOnResponse(userResponse);
			if (!canGiveConsent)
			{
				personalizedAdsAllowed = false;
			}
			ApplyConsentToLevelPlay(personalizedAdsAllowed);
		});
	}

	private static void ApplyConsentToLevelPlay(bool personalizedAdsAllowed)
	{
		if (_Scripts.Singletons.Singleton<RewardedAdsManager>.Instance.SDKIsInitialized)
		{
			LevelPlayConfigured?.Invoke(obj: false);
			return;
		}
		if (isChild)
		{
			LevelPlay.SetMetaData("is_deviceid_optout", "true");
			LevelPlay.SetMetaData("is_child_directed", "true");
			LevelPlay.SetMetaData("Google_Family_Self_Certified_SDKS", "true");
			personalizedAdsAllowed = false;
		}
		else
		{
			LevelPlay.SetMetaData("is_deviceid_optout", "false");
			LevelPlay.SetMetaData("is_child_directed", "false");
			LevelPlay.SetMetaData("Google_Family_Self_Certified_SDKS", "false");
			if (Unity.Usercentrics.Singleton<Usercentrics>.Instance != null && Unity.Usercentrics.Singleton<Usercentrics>.Instance.IsInitialized)
			{
				CCPAData uSPData = Unity.Usercentrics.Singleton<Usercentrics>.Instance.GetUSPData();
				if (uSPData.noticeGiven)
				{
					bool flag = !canGiveConsent || uSPData.optedOut;
					LevelPlay.SetMetaData("do_not_sell", flag ? "true" : "false");
				}
			}
		}
		ConsentState consentState = EndUserConsent.GetConsentState();
		consentState.AdsIntent = (personalizedAdsAllowed ? ConsentStatus.Granted : ConsentStatus.Denied);
		EndUserConsent.SetConsentState(consentState);
		LevelPlay.SetConsent(personalizedAdsAllowed);
		LevelPlayConfigurationDone = true;
		LevelPlayConfigured?.Invoke(obj: true);
	}

	private static bool DetermineMarketingAcceptedBasedOnResponse(UsercentricsConsentUserResponse userResponse)
	{
		if (userResponse == null)
		{
			return false;
		}
		return DetermineMarketingAcceptedBasedOnConsent(userResponse.consents);
	}

	private static bool DetermineMarketingAcceptedBasedOnConsent(List<UsercentricsServiceConsent> consents)
	{
		if (consents == null || consents.Count == 0)
		{
			return false;
		}
		if (marketingTemplateIdsToDpsNames == null || marketingTemplateIdsToDpsNames.Count == 0)
		{
			Debug.LogWarning("[CMP] marketingTemplateIds not set. Falling back to no-consent.");
			return false;
		}
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		foreach (UsercentricsServiceConsent consent in consents)
		{
			if (!string.IsNullOrEmpty(consent.templateId) && marketingTemplateIdsToDpsNames.TryGetValue(consent.templateId, out var value))
			{
				dictionary[value] = consent.status;
			}
		}
		foreach (KeyValuePair<string, string> marketingTemplateIdsToDpsName in marketingTemplateIdsToDpsNames)
		{
			if (dictionary.TryGetValue(marketingTemplateIdsToDpsName.Value, out var value2) && value2)
			{
				return true;
			}
		}
		return false;
	}

	public static void SetIsChild(bool value)
	{
		isChild = value;
	}

	public static void SetCanGiveConsent(bool value)
	{
		canGiveConsent = value;
	}

	public static void Setup()
	{
		if (isChild)
		{
			ShowPrivacySettingsButton = false;
			ApplyConsentToLevelPlay(personalizedAdsAllowed: false);
		}
		else if (Unity.Usercentrics.Singleton<Usercentrics>.Instance.IsPlatformSupported())
		{
			InitializeUsercentrics();
		}
		else
		{
			Debug.LogWarning("[CMP] Usercentrics not supported on this platform. Falling back to no-consent.");
			ShowPrivacySettingsButton = false;
			ApplyConsentToLevelPlay(personalizedAdsAllowed: false);
		}
	}

	public static void OpenPrivacySettings()
	{
		if (!Unity.Usercentrics.Singleton<Usercentrics>.Instance.IsInitialized)
		{
			InitializeUsercentrics();
			return;
		}
		Debug.Log("[CMP] Opening Usercentrics Second Layer...");
		Unity.Usercentrics.Singleton<Usercentrics>.Instance.ShowSecondLayer(bannerSettings, delegate(UsercentricsConsentUserResponse userResponse)
		{
			bool personalizedAdsAllowed = DetermineMarketingAcceptedBasedOnResponse(userResponse);
			if (!canGiveConsent)
			{
				personalizedAdsAllowed = false;
			}
			ApplyConsentToLevelPlay(personalizedAdsAllowed);
			Unity.Usercentrics.Singleton<Usercentrics>.Instance.GetConsents();
			OnlineGate.EnsureOnlineReadyAsync();
		});
	}
}
