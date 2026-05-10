using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UI.Shop;

public class ShopSelector : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private Transform container;

	[SerializeField]
	private CosmeticItemType cosmetic;

	[SerializeField]
	private ShopButton cosmeticPrefab;

	[SerializeField]
	private GameObject noItemsText;

	private List<ShopButton> buttons = new List<ShopButton>();

	private ShopController shopController;

	public event Action<CosmeticItemSo, Image, ShopButton> OnItemSelected;

	public bool HasButtons()
	{
		return buttons.Count > 0;
	}

	private void OnEnable()
	{
		if (shopController != null)
		{
			shopController.SetActiveShopSelector(this);
		}
		SelectFirstButton();
	}

	private void ClearButtons()
	{
		for (int num = buttons.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(buttons[num].gameObject);
		}
		buttons = new List<ShopButton>();
	}

	public void InitializeButtons(ShopController newShopController)
	{
		shopController = newShopController;
		ClearButtons();
		switch (cosmetic)
		{
		case CosmeticItemType.Accessory:
			foreach (CosmeticItemAccessorySo visibleShopItem in Singleton<CosmeticItemsController>.Instance.GetVisibleShopItems<CosmeticItemAccessorySo>())
			{
				ShopButton shopButton5 = UnityEngine.Object.Instantiate(cosmeticPrefab, container);
				shopButton5.Setup(this, visibleShopItem, cosmetic);
				buttons.Add(shopButton5);
			}
			break;
		case CosmeticItemType.Hat:
			foreach (CosmeticItemHatSo visibleShopItem2 in Singleton<CosmeticItemsController>.Instance.GetVisibleShopItems<CosmeticItemHatSo>())
			{
				ShopButton shopButton4 = UnityEngine.Object.Instantiate(cosmeticPrefab, container);
				shopButton4.Setup(this, visibleShopItem2, cosmetic);
				buttons.Add(shopButton4);
			}
			break;
		case CosmeticItemType.Shoe:
			foreach (CosmeticItemShoeSo visibleShopItem3 in Singleton<CosmeticItemsController>.Instance.GetVisibleShopItems<CosmeticItemShoeSo>())
			{
				ShopButton shopButton3 = UnityEngine.Object.Instantiate(cosmeticPrefab, container);
				shopButton3.Setup(this, visibleShopItem3, cosmetic);
				buttons.Add(shopButton3);
			}
			break;
		case CosmeticItemType.Web:
			foreach (CosmeticItemWebSo visibleShopItem4 in Singleton<CosmeticItemsController>.Instance.GetVisibleShopItems<CosmeticItemWebSo>())
			{
				ShopButton shopButton2 = UnityEngine.Object.Instantiate(cosmeticPrefab, container);
				shopButton2.Setup(this, visibleShopItem4, cosmetic);
				buttons.Add(shopButton2);
			}
			break;
		case CosmeticItemType.Eye:
			foreach (CosmeticItemEyeSo visibleShopItem5 in Singleton<CosmeticItemsController>.Instance.GetVisibleShopItems<CosmeticItemEyeSo>())
			{
				ShopButton shopButton = UnityEngine.Object.Instantiate(cosmeticPrefab, container);
				shopButton.Setup(this, visibleShopItem5, cosmetic);
				buttons.Add(shopButton);
			}
			break;
		default:
			Debug.LogWarning("Accessory type not implemented: " + cosmetic);
			return;
		}
		noItemsText.SetActive(buttons.Count == 0);
	}

	public void CosmeticItemSelected(CosmeticItemSo item, Image image, ShopButton button)
	{
		this.OnItemSelected?.Invoke(item, image, button);
	}

	public void SelectFirstButton()
	{
		if (buttons.Count != 0)
		{
			buttons[0].SelectButton();
			buttons[0].GetButton().Select();
		}
	}
}
