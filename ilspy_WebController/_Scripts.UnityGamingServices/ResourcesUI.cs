using System;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudCode.GeneratedBindings.AWebbingJourneyCloud;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public class ResourcesUI : MonoBehaviour
{
	private enum ResourceType
	{
		Coffee,
		LevelUnlockedOffice,
		LevelUnlockedKidsRoom,
		PlaySessionExpiresAt,
		PlayerId
	}

	[Header("UI")]
	[SerializeField]
	private TextMeshProUGUI resourcesText;

	[Header("Parameters")]
	[SerializeField]
	private ResourceType resourceType;

	private void OnEnable()
	{
		Singleton<PlayerEconomyManager>.Instance.PlayerEconomyDataUpdated += PlayerEconomyManager_OnPlayerEconomyDataUpdated;
		Singleton<PlaySessionManager>.Instance.PlaySessionDataUpdated += PlaySessionManager_OnPlaySessionDataUpdated;
		if (Singleton<PlayerEconomyManager>.Instance.EconomyDataIsInitialized)
		{
			switch (resourceType)
			{
			case ResourceType.Coffee:
			{
				int coffee = Singleton<PlayerEconomyManager>.Instance.Coffee;
				resourcesText.text = $"Coffees bought: {coffee}";
				break;
			}
			case ResourceType.LevelUnlockedOffice:
			{
				bool levelUnlocked = Singleton<PlayerEconomyManager>.Instance.GetLevelUnlocked(PlayerEconomyManager.Level.Office);
				resourcesText.text = $"Office Level Unlocked: {levelUnlocked}";
				break;
			}
			case ResourceType.LevelUnlockedKidsRoom:
			{
				bool levelUnlocked2 = Singleton<PlayerEconomyManager>.Instance.GetLevelUnlocked(PlayerEconomyManager.Level.KidsRoom);
				resourcesText.text = $"Kids Room Level Unlocked: {levelUnlocked2}";
				break;
			}
			case ResourceType.PlayerId:
				resourcesText.text = ((OnlineGate.State == OnlineState.OnlineReady) ? AuthenticationService.Instance.PlayerId : string.Empty);
				break;
			case ResourceType.PlaySessionExpiresAt:
				break;
			}
		}
	}

	private void OnDisable()
	{
		Singleton<PlayerEconomyManager>.Instance.PlayerEconomyDataUpdated -= PlayerEconomyManager_OnPlayerEconomyDataUpdated;
		Singleton<PlaySessionManager>.Instance.PlaySessionDataUpdated -= PlaySessionManager_OnPlaySessionDataUpdated;
	}

	private void PlayerEconomyManager_OnPlayerEconomyDataUpdated(PlayerEconomyData playerEconomyData)
	{
		switch (resourceType)
		{
		case ResourceType.Coffee:
		{
			int coffee = Singleton<PlayerEconomyManager>.Instance.Coffee;
			resourcesText.text = $"Coffees bought: {coffee}";
			break;
		}
		case ResourceType.LevelUnlockedOffice:
		{
			bool levelUnlocked2 = Singleton<PlayerEconomyManager>.Instance.GetLevelUnlocked(PlayerEconomyManager.Level.Office);
			resourcesText.text = $"Office Level Unlocked: {levelUnlocked2}";
			break;
		}
		case ResourceType.LevelUnlockedKidsRoom:
		{
			bool levelUnlocked = Singleton<PlayerEconomyManager>.Instance.GetLevelUnlocked(PlayerEconomyManager.Level.KidsRoom);
			resourcesText.text = $"Kids Room Level Unlocked: {levelUnlocked}";
			break;
		}
		case ResourceType.PlaySessionExpiresAt:
		case ResourceType.PlayerId:
			break;
		}
	}

	private void PlaySessionManager_OnPlaySessionDataUpdated(PlaySessionData playSessionData)
	{
		switch (resourceType)
		{
		case ResourceType.PlaySessionExpiresAt:
		{
			DateTime playSessionExpiresAt = playSessionData.PlaySessionExpiresAt;
			resourcesText.text = $"PlaySession Expires At: {playSessionExpiresAt}";
			break;
		}
		case ResourceType.Coffee:
		case ResourceType.LevelUnlockedOffice:
		case ResourceType.LevelUnlockedKidsRoom:
		case ResourceType.PlayerId:
			break;
		}
	}
}
