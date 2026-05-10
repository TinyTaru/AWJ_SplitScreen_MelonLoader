using TMPro;
using Unity.Services.CloudCode.GeneratedBindings.AWebbingJourneyCloud;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public class PlayerCanvas : MonoBehaviour
{
	[Header("Buttons")]
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

	private void Awake()
	{
		unlockOfficeLevelButton.interactable = false;
		unlockKidsRoomLevelButton.interactable = false;
		buyCoffee1Button.interactable = false;
		buyCoffee2Button.interactable = false;
		buyCoffee3Button.interactable = false;
	}

	private void OnEnable()
	{
		if (Singleton<IAPStoreManager>.Instance != null)
		{
			if (Singleton<IAPStoreManager>.Instance.ProductsAreFetched)
			{
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
				unlockOfficeLevelButton.interactable = !Singleton<PlayerEconomyManager>.Instance.GetLevelUnlocked(PlayerEconomyManager.Level.Office);
				unlockKidsRoomLevelButton.interactable = !Singleton<PlayerEconomyManager>.Instance.GetLevelUnlocked(PlayerEconomyManager.Level.KidsRoom);
			}
		}
		if (openConsentFormButton != null)
		{
			openConsentFormButton.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		if (Singleton<IAPStoreManager>.Instance != null)
		{
			Singleton<IAPStoreManager>.Instance.ProductsFetched -= IapStoreManager_OnProductsFetched;
		}
		if (Singleton<PlayerEconomyManager>.Instance != null)
		{
			Singleton<PlayerEconomyManager>.Instance.PlayerEconomyDataUpdated -= PlayerEconomyManager_OnPlayerEconomyDataUpdated;
		}
	}

	public void RestorePurchases()
	{
		Singleton<IAPStoreManager>.Instance.RestorePurchases();
	}

	public void OpenConsentForm()
	{
		ConsentManager.OpenPrivacySettings();
	}

	private void IapStoreManager_OnProductsFetched()
	{
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
				buyCoffee3Button.interactable = true;
				break;
			case "COFFEE_3":
				coffee3PriceText.text = product.metadata.localizedPriceString;
				buyCoffee3Button.interactable = true;
				break;
			}
		}
	}

	private void PlayerEconomyManager_OnPlayerEconomyDataUpdated(PlayerEconomyData playerEconomyData)
	{
		unlockOfficeLevelButton.interactable = !Singleton<PlayerEconomyManager>.Instance.GetLevelUnlocked(PlayerEconomyManager.Level.Office);
		unlockKidsRoomLevelButton.interactable = !Singleton<PlayerEconomyManager>.Instance.GetLevelUnlocked(PlayerEconomyManager.Level.KidsRoom);
	}
}
