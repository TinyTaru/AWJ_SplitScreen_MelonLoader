using System.Collections.Generic;
using UnityEngine;
using _Scripts.Singletons;
using _Scripts.UI.Panels;
using _Scripts.UnityGamingServices;

namespace _Scripts.UI.MobileMonetization;

public class IapShopPopups : MonoBehaviour
{
	[SerializeField]
	private RectTransform processingPurchaseInformation;

	[SerializeField]
	private RectTransform allLevelsPopup;

	[SerializeField]
	private RectTransform officePopup;

	[SerializeField]
	private RectTransform kidsRoomPopup;

	[SerializeField]
	private RectTransform coffeePopup;

	[SerializeField]
	private Transform coffeeContainer;

	private PanelManager panelManager;

	private List<Transform> coffees;

	private object currentPurchaseId;

	private void Awake()
	{
		panelManager = GetComponentInParent<PanelManager>();
		processingPurchaseInformation.gameObject.SetActive(value: false);
		officePopup.gameObject.SetActive(value: false);
		kidsRoomPopup.gameObject.SetActive(value: false);
		coffeePopup.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		if (Singleton<IAPStoreManager>.Instance != null)
		{
			Singleton<IAPStoreManager>.Instance.SuccessfullyPurchased += IapStoreManager_OnSuccessfullyPurchased;
			Singleton<IAPStoreManager>.Instance.PurchaseFailed += IapStoreManager_OnPurchaseFailed;
		}
	}

	private void OnDisable()
	{
		if (Singleton<IAPStoreManager>.Instance != null)
		{
			Singleton<IAPStoreManager>.Instance.SuccessfullyPurchased -= IapStoreManager_OnSuccessfullyPurchased;
			Singleton<IAPStoreManager>.Instance.PurchaseFailed -= IapStoreManager_OnPurchaseFailed;
		}
	}

	private void ShowAllLevelsPopup()
	{
		panelManager.OpenPanel(allLevelsPopup);
	}

	private void ShowOfficePopup()
	{
		panelManager.OpenPanel(officePopup);
	}

	private void ShowKidsRoomPopup()
	{
		panelManager.OpenPanel(kidsRoomPopup);
	}

	private void ShowCoffeePopup()
	{
		int coffee = Singleton<PlayerEconomyManager>.Instance.Coffee;
		Debug.Log($"visibleCoffees: {coffee}");
		SetupCoffeePopup(coffee);
		panelManager.OpenPanel(coffeePopup);
	}

	private void SetupCoffeePopup(int visibleCoffees)
	{
		coffees = new List<Transform>();
		foreach (Transform item in coffeeContainer)
		{
			coffees.Add(item);
		}
		for (int num = coffees.Count - 1; num >= 0; num--)
		{
			coffees[num].gameObject.SetActive(coffees.Count - num <= visibleCoffees);
		}
	}

	public void ShowProcessingPurchaseInformation(string newPurchaseId)
	{
		currentPurchaseId = newPurchaseId;
		panelManager.OpenPanel(processingPurchaseInformation);
	}

	public void ClosePopup()
	{
		panelManager.GoBack(2);
	}

	private void IapStoreManager_OnSuccessfullyPurchased(string obj)
	{
		switch (currentPurchaseId as string)
		{
		case "LEVEL_UNLOCK_ALL":
			ShowAllLevelsPopup();
			break;
		case "LEVEL_UNLOCK_OFFICE":
			ShowOfficePopup();
			break;
		case "LEVEL_UNLOCK_KIDS_ROOM":
			ShowKidsRoomPopup();
			break;
		case "COFFEE_1":
		case "COFFEE_2":
		case "COFFEE_3":
			ShowCoffeePopup();
			break;
		}
	}

	private void IapStoreManager_OnPurchaseFailed(string obj)
	{
		panelManager.GoBack();
	}
}
