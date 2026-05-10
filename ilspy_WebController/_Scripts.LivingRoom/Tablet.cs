using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;
using _Scripts.Objects;
using _Scripts.Singletons;
using _Scripts.Utils;

namespace _Scripts.LivingRoom;

public class Tablet : MonoBehaviour
{
	[SerializeField]
	private TabletShopItemSo[] shopItemSos;

	[SerializeField]
	private TabletShopItemSo[] unlockableShopItemSos;

	[SerializeField]
	private TabletShopItemUI tabletShopItemUIPrefab;

	[SerializeField]
	private GameObject emptyShopItemUIPrefab;

	[SerializeField]
	private Transform shopItemContainer;

	[SerializeField]
	private int itemsPerPage;

	[SerializeField]
	private GameObject shopUI;

	[SerializeField]
	private GameObject cartUI;

	[SerializeField]
	private CanvasGroup screenOffOverlay;

	[SerializeField]
	private float turnOnFadeDuration = 0.5f;

	[SerializeField]
	private Transform cartItemContainer;

	[SerializeField]
	private TabletCartItemUI cartItemUIPrefab;

	[SerializeField]
	private TextMeshProUGUI totalPriceText;

	[SerializeField]
	private GameObject buyButton;

	[SerializeField]
	private GameObject orderInProgressUI;

	[SerializeField]
	private float orderSpawnDelay = 5f;

	[SerializeField]
	private float orderSpawnInterval = 2f;

	[SerializeField]
	private Transform orderSpawnPosition;

	[SerializeField]
	private GameObject shopButton;

	[SerializeField]
	private GameObject cartButton;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onDeliveryStartedEvent;

	[SerializeField]
	private UnityEvent onDeliveryItemSpawnedEvent;

	[SerializeField]
	private UnityEvent onDeliveryFinishedEvent;

	private bool isOn;

	private List<TabletShopItem> shopItemList;

	private TabletButton currentButton;

	private int currentPage;

	private int numberOfPages;

	public bool IsOn => isOn;

	public event Action OnTurnOn;

	public event Action OnTurnOff;

	private void Start()
	{
		isOn = false;
		currentPage = 0;
		shopItemList = new List<TabletShopItem>();
		shopUI.SetActive(value: true);
		cartUI.SetActive(value: false);
		orderInProgressUI.SetActive(value: false);
		shopButton.SetActive(value: true);
		cartButton.SetActive(value: true);
		screenOffOverlay.gameObject.SetActive(value: true);
		screenOffOverlay.alpha = 1f;
		TabletShopItemSo[] array = shopItemSos;
		for (int i = 0; i < array.Length; i++)
		{
			TabletShopItem item = new TabletShopItem(array[i], 0);
			shopItemList.Add(item);
		}
		UpdateShopUIForCurrentPage();
		CheckUnlockableItems();
	}

	private IEnumerator ProcessingOrderCoroutine()
	{
		yield return new WaitForSeconds(orderSpawnDelay);
		onDeliveryStartedEvent?.Invoke();
		foreach (TabletShopItem shopItem in shopItemList)
		{
			if (shopItem.Amount == 0)
			{
				continue;
			}
			yield return new WaitForSeconds(orderSpawnInterval);
			if (!(orderSpawnPosition != null))
			{
				continue;
			}
			for (int i = 0; i < shopItem.Amount; i++)
			{
				if (!(shopItem.TabletShopItemSo.spawnableObjectPrefab == null))
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(shopItem.TabletShopItemSo.spawnableObjectPrefab, orderSpawnPosition.position, Quaternion.identity, null);
					Rigidbody[] componentsInChildren = gameObject.GetComponentsInChildren<Rigidbody>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						componentsInChildren[j].AddTorque(_Scripts.Utils.Utils.RandomVector3() * 20f, ForceMode.Acceleration);
					}
					SpawnableObject[] componentsInChildren2 = gameObject.GetComponentsInChildren<SpawnableObject>();
					for (int j = 0; j < componentsInChildren2.Length; j++)
					{
						componentsInChildren2[j].Setup();
					}
					onDeliveryItemSpawnedEvent?.Invoke();
					yield return new WaitForSeconds(shopItem.TabletShopItemSo.spawnInterval);
				}
			}
		}
		onDeliveryFinishedEvent?.Invoke();
		currentPage = 0;
		foreach (TabletShopItem shopItem2 in shopItemList)
		{
			shopItem2.Amount = 0;
		}
		UpdateShopUIForCurrentPage();
		shopUI.SetActive(value: true);
		shopButton.SetActive(value: true);
		cartButton.SetActive(value: true);
		orderInProgressUI.SetActive(value: false);
	}

	private void CheckUnlockableItems()
	{
		TabletShopItemSo[] array = unlockableShopItemSos;
		foreach (TabletShopItemSo tabletShopItemSo in array)
		{
			if (SaveController.Load("LivingRoom_" + tabletShopItemSo.name + "_unlocked", defaultValue: false, SaveData.Game))
			{
				AddShopItem(tabletShopItemSo);
			}
		}
	}

	private void UpdateShopUIForCurrentPage()
	{
		numberOfPages = Mathf.CeilToInt((float)shopItemList.Count / (float)itemsPerPage);
		foreach (Transform item in shopItemContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		for (int i = 0; i < itemsPerPage; i++)
		{
			int num = currentPage * itemsPerPage + i;
			if (num >= shopItemList.Count)
			{
				UnityEngine.Object.Instantiate(emptyShopItemUIPrefab, shopItemContainer);
				continue;
			}
			TabletShopItemUI tabletShopItemUI = UnityEngine.Object.Instantiate(tabletShopItemUIPrefab, shopItemContainer);
			TabletShopItem newShopItem = shopItemList[num];
			tabletShopItemUI.Setup(newShopItem);
		}
	}

	private void UpdateCartUI()
	{
		foreach (Transform item in cartItemContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		float num = 0f;
		foreach (TabletShopItem shopItem in shopItemList)
		{
			if (shopItem.Amount != 0)
			{
				UnityEngine.Object.Instantiate(cartItemUIPrefab, cartItemContainer).Setup(shopItem);
				num += shopItem.TabletShopItemSo.price * (float)shopItem.Amount;
			}
		}
		totalPriceText.text = $"Total Price: $ {num:0.00}";
		buyButton.gameObject.SetActive(num > 0f);
	}

	public void ToggleOnOff()
	{
		if (isOn)
		{
			isOn = false;
			screenOffOverlay.gameObject.SetActive(value: true);
			screenOffOverlay.DOKill();
			screenOffOverlay.DOFade(1f, 1f / turnOnFadeDuration).SetSpeedBased();
			this.OnTurnOff?.Invoke();
		}
		else
		{
			isOn = true;
			screenOffOverlay.DOKill();
			screenOffOverlay.DOFade(0f, 1f / turnOnFadeDuration).SetSpeedBased().OnComplete(delegate
			{
				screenOffOverlay.gameObject.SetActive(value: false);
			});
			this.OnTurnOn?.Invoke();
		}
	}

	public void OpenShopUI()
	{
		currentPage = 0;
		UpdateShopUIForCurrentPage();
		shopUI.SetActive(value: true);
		cartUI.SetActive(value: false);
		shopButton.SetActive(value: true);
		cartButton.SetActive(value: true);
		orderInProgressUI.SetActive(value: false);
	}

	public void OpenShoppingCartUI()
	{
		UpdateCartUI();
		shopUI.SetActive(value: false);
		cartUI.SetActive(value: true);
		shopButton.SetActive(value: true);
		cartButton.SetActive(value: true);
		orderInProgressUI.SetActive(value: false);
	}

	public void Buy()
	{
		shopUI.SetActive(value: false);
		cartUI.SetActive(value: false);
		shopButton.SetActive(value: false);
		cartButton.SetActive(value: false);
		orderInProgressUI.SetActive(value: true);
		StartCoroutine(ProcessingOrderCoroutine());
	}

	public void LoadPreviousPage()
	{
		currentPage = (currentPage - 1 + numberOfPages) % numberOfPages;
		UpdateShopUIForCurrentPage();
	}

	public void LoadNextPage()
	{
		currentPage = (currentPage + 1) % numberOfPages;
		UpdateShopUIForCurrentPage();
	}

	public void AddShopItem(TabletShopItemSo shopItemSo)
	{
		if (!shopItemList.Any((TabletShopItem shopItem) => shopItem.TabletShopItemSo == shopItemSo))
		{
			TabletShopItem item = new TabletShopItem(shopItemSo, 0);
			shopItemList.Add(item);
			SaveController.Save("LivingRoom_" + shopItemSo.name + "_unlocked", value: true, SaveData.Game);
			UpdateShopUIForCurrentPage();
		}
	}

	private void UnlockAllShopItems()
	{
		TabletShopItemSo[] array = unlockableShopItemSos;
		foreach (TabletShopItemSo shopItemSo in array)
		{
			AddShopItem(shopItemSo);
		}
	}
}
