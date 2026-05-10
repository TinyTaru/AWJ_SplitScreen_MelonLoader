using System;
using System.Linq;
using TMPro;
using Unity.Services.CloudCode.GeneratedBindings.AWebbingJourneyCloud;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using _Scripts.Singletons;
using _Scripts.UI.MobileMonetization;
using _Scripts.UnityGamingServices;

namespace _Prefabs.UI.IAPShop;

public class IAPShopCanvas : MonoBehaviour
{
	[SerializeField]
	private GameObject noInternetUI;

	[SerializeField]
	private GameObject shopInitializingUI;

	[SerializeField]
	private GameObject shopUnavailableUI;

	[SerializeField]
	private GameObject shopUI;

	[SerializeField]
	private GameObject buyCoffeeUI;

	[SerializeField]
	private GameObject privacySettingsChangedUI;

	[Header("Buttons")]
	[SerializeField]
	private Button unlockAllLevelsButton;

	[SerializeField]
	private Button unlockOfficeLevelButton;

	[SerializeField]
	private Button unlockKidsRoomLevelButton;

	[SerializeField]
	private Button buyCoffee1Button;

	[SerializeField]
	private Button buyCoffee2Button;

	[SerializeField]
	private Button buyCoffee3Button;

	[SerializeField]
	private Button openConsentFormButton;

	[SerializeField]
	private GameObject connectGooglePlayGamesButton;

	[SerializeField]
	private GameObject connectAppleGameCenterButton;

	[Header("Price Texts")]
	[SerializeField]
	private TextMeshProUGUI unlockOfficeLevelPriceText;

	[SerializeField]
	private TextMeshProUGUI unlockKidsRoomLevelPriceText;

	[SerializeField]
	private TextMeshProUGUI coffee1PriceText;

	[SerializeField]
	private TextMeshProUGUI coffee2PriceText;

	[SerializeField]
	private TextMeshProUGUI coffee3PriceText;

	private string currentPurchaseId;

	private bool buyCoffeeUIOpen;

	private bool privacySettingsChangedUIOpen;

	private bool popupOpen;

	private void Awake()
	{
		unlockAllLevelsButton.interactable = false;
		unlockOfficeLevelButton.interactable = false;
		unlockKidsRoomLevelButton.interactable = false;
		buyCoffee1Button.interactable = false;
		buyCoffee2Button.interactable = false;
		buyCoffee3Button.interactable = false;
	}

	private void OnEnable()
	{
		OnlineGate.EnsureOnlineReadyAsync();
		OnlineGate.OnStateChanged += OnlineGate_OnStateChanged;
		if (Singleton<IAPStoreManager>.Instance != null)
		{
			if (Singleton<IAPStoreManager>.Instance.ProductsAreFetched)
			{
				UpdateShopUI();
			}
			else
			{
				Singleton<IAPStoreManager>.Instance.ProductsFetched += IapStoreManager_OnProductsFetched;
			}
		}
		if (Singleton<PlayerEconomyManager>.Instance != null)
		{
			Singleton<PlayerEconomyManager>.Instance.PlayerEconomyDataUpdated += PlayerEconomyManager_OnPlayerEconomyDataUpdated;
			if (Singleton<PlayerEconomyManager>.Instance.EconomyDataIsInitialized)
			{
				UpdateUnlockButtonInteractability();
			}
		}
		buyCoffeeUIOpen = false;
		privacySettingsChangedUIOpen = false;
		popupOpen = false;
		UpdateShopUI();
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
	}

	private void UpdatePriceTexts()
	{
		if (!Singleton<IAPStoreManager>.Instance.ProductsAreFetched || !Singleton<PlayerEconomyManager>.Instance.EconomyDataIsInitialized)
		{
			return;
		}
		foreach (Product product in Singleton<IAPStoreManager>.Instance.Products)
		{
			switch (product.definition.id)
			{
			case "LEVEL_UNLOCK_OFFICE":
				unlockOfficeLevelPriceText.text = product.metadata.localizedPriceString;
				break;
			case "LEVEL_UNLOCK_KIDS_ROOM":
				unlockKidsRoomLevelPriceText.text = product.metadata.localizedPriceString;
				break;
			case "COFFEE_1":
				coffee1PriceText.text = product.metadata.localizedPriceString;
				buyCoffee1Button.interactable = true;
				break;
			case "COFFEE_2":
				coffee2PriceText.text = product.metadata.localizedPriceString;
				buyCoffee2Button.interactable = true;
				break;
			case "COFFEE_3":
				coffee3PriceText.text = product.metadata.localizedPriceString;
				buyCoffee3Button.interactable = true;
				break;
			}
		}
	}

	private void UpdateShopUI()
	{
		if (!popupOpen && !buyCoffeeUIOpen && !privacySettingsChangedUIOpen)
		{
			bool flag = OnlineGate.State == OnlineState.OnlineReady;
			bool flag2 = Singleton<IAPStoreManager>.Instance != null && Singleton<IAPStoreManager>.Instance.ProductsAreFetched && Singleton<PlayerEconomyManager>.Instance.EconomyDataIsInitialized;
			bool flag3 = Singleton<PlayerEconomyManager>.Instance.InitializationFailed || Singleton<IAPStoreManager>.Instance.InitializationFailed;
			noInternetUI.SetActive(!flag);
			shopUnavailableUI.SetActive(flag3);
			shopInitializingUI.SetActive(!flag3 && flag && !flag2);
			shopUI.SetActive(!flag3 && flag && flag2);
			buyCoffeeUI.SetActive(value: false);
			privacySettingsChangedUI.SetActive(value: false);
			UpdatePriceTexts();
			if (openConsentFormButton != null)
			{
				openConsentFormButton.gameObject.SetActive(ConsentManager.ShowPrivacySettingsButton);
			}
		}
	}

	private void UpdateUnlockButtonInteractability()
	{
		unlockAllLevelsButton.interactable = !Singleton<PlayerEconomyManager>.Instance.GetAllLevelsUnlocked();
		unlockOfficeLevelButton.interactable = !Singleton<PlayerEconomyManager>.Instance.GetLevelUnlocked(PlayerEconomyManager.Level.Office);
		unlockKidsRoomLevelButton.interactable = !Singleton<PlayerEconomyManager>.Instance.GetLevelUnlocked(PlayerEconomyManager.Level.KidsRoom);
	}

	public void RestorePurchases()
	{
		Singleton<IAPStoreManager>.Instance.RestorePurchases();
	}

	public void OpenPrivacySettings()
	{
		ConsentManager.LevelPlayConfigured = (Action<bool>)Delegate.Combine(ConsentManager.LevelPlayConfigured, new Action<bool>(LevelPlayConfigured));
		ConsentManager.OpenPrivacySettings();
	}

	private void LevelPlayConfigured(bool _)
	{
		ConsentManager.LevelPlayConfigured = (Action<bool>)Delegate.Remove(ConsentManager.LevelPlayConfigured, new Action<bool>(LevelPlayConfigured));
		ShowPrivacySettingsChangedUI();
	}

	public void ShowBuyCoffeeUI()
	{
		buyCoffeeUIOpen = true;
		shopUI.SetActive(value: false);
		buyCoffeeUI.SetActive(value: true);
	}

	public void CloseBuyCoffeeUI()
	{
		buyCoffeeUIOpen = false;
		shopUI.SetActive(value: true);
		buyCoffeeUI.SetActive(value: false);
	}

	public void ShowPrivacySettingsChangedUI()
	{
		privacySettingsChangedUIOpen = true;
		shopUI.SetActive(value: false);
		privacySettingsChangedUI.SetActive(value: true);
	}

	public void ClosePrivacySettingsChangedUI()
	{
		privacySettingsChangedUIOpen = false;
		shopUI.SetActive(value: true);
		privacySettingsChangedUI.SetActive(value: false);
	}

	public void UnlockAllLevels()
	{
		currentPurchaseId = "LEVEL_UNLOCK_ALL";
		Singleton<IAPStoreManager>.Instance.UnlockAllLevels();
		IapShopPopups iapShopPopups = UnityEngine.Object.FindObjectsByType<IapShopPopups>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
		if (iapShopPopups != null)
		{
			iapShopPopups.ShowProcessingPurchaseInformation(currentPurchaseId);
		}
	}

	public void UnlockOfficeLevel()
	{
		currentPurchaseId = "LEVEL_UNLOCK_OFFICE";
		Singleton<IAPStoreManager>.Instance.UnlockOfficeLevel();
		IapShopPopups iapShopPopups = UnityEngine.Object.FindObjectsByType<IapShopPopups>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
		if (iapShopPopups != null)
		{
			iapShopPopups.ShowProcessingPurchaseInformation(currentPurchaseId);
		}
	}

	public void UnlockKidsRoomLevel()
	{
		currentPurchaseId = "LEVEL_UNLOCK_KIDS_ROOM";
		Singleton<IAPStoreManager>.Instance.UnlockKidsRoomLevel();
		IapShopPopups iapShopPopups = UnityEngine.Object.FindObjectsByType<IapShopPopups>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
		if (iapShopPopups != null)
		{
			iapShopPopups.ShowProcessingPurchaseInformation(currentPurchaseId);
		}
	}

	public void BuyCoffee(int amount)
	{
		switch (amount)
		{
		default:
			currentPurchaseId = "COFFEE_1";
			break;
		case 2:
			currentPurchaseId = "COFFEE_2";
			break;
		case 3:
			currentPurchaseId = "COFFEE_3";
			break;
		}
		Singleton<IAPStoreManager>.Instance.BuyCoffee(amount);
		IapShopPopups iapShopPopups = UnityEngine.Object.FindObjectsByType<IapShopPopups>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
		if (iapShopPopups != null)
		{
			iapShopPopups.ShowProcessingPurchaseInformation(currentPurchaseId);
		}
	}

	public void Continue()
	{
		buyCoffeeUIOpen = false;
		popupOpen = false;
		UpdateShopUI();
	}

	public void ConnectGooglePlayGames()
	{
	}

	public void ConnectAppleGameCenter()
	{
	}

	private void OnlineGate_OnStateChanged(OnlineState obj)
	{
		UpdateShopUI();
	}

	private void IapStoreManager_OnProductsFetched()
	{
		UpdateShopUI();
	}

	private void PlayerEconomyManager_OnPlayerEconomyDataUpdated(PlayerEconomyData playerEconomyData)
	{
		UpdateUnlockButtonInteractability();
	}
}
