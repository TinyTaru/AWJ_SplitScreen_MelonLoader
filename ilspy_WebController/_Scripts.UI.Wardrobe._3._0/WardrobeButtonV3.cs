using System;
using MPUIKIT;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UI.Wardrobe._3._0;

public class WardrobeButtonV3 : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private MPImage cosmeticItemImage;

	[SerializeField]
	private Button button;

	private ItemSelectorV3 itemSelector;

	private CosmeticItemSo cosmeticItemSo;

	public void Setup(ItemSelectorV3 selector, CosmeticItemSo item, CosmeticItemType cosmeticItemType)
	{
		itemSelector = selector;
		cosmeticItemSo = item;
		switch (cosmeticItemType)
		{
		case CosmeticItemType.Accessory:
			cosmeticItemImage.sprite = ((CosmeticItemAccessorySo)cosmeticItemSo).accessorySo.accessorySprite;
			break;
		case CosmeticItemType.Hat:
			cosmeticItemImage.sprite = ((CosmeticItemHatSo)cosmeticItemSo).hatSo.hatSprite;
			break;
		case CosmeticItemType.Shoe:
			cosmeticItemImage.sprite = ((CosmeticItemShoeSo)cosmeticItemSo).shoeSo.shoeSprite;
			break;
		case CosmeticItemType.Web:
			cosmeticItemImage.sprite = ((CosmeticItemWebSo)cosmeticItemSo).webSo.webSprite;
			break;
		case CosmeticItemType.Eye:
			cosmeticItemImage.sprite = ((CosmeticItemEyeSo)cosmeticItemSo).eyeSo.eyeSprite;
			break;
		default:
			throw new ArgumentOutOfRangeException("cosmeticItemType", cosmeticItemType, null);
		}
		button.interactable = Singleton<CosmeticItemsController>.Instance.IsItemUnlocked(item);
	}

	public void ButtonClick()
	{
		itemSelector.ChangeItem(cosmeticItemSo, saveItem: true);
	}

	public void SelectButton()
	{
		itemSelector.ChangeItem(cosmeticItemSo);
	}

	public Button GetButton()
	{
		return button;
	}

	public CosmeticItemSo GetCosmeticItem()
	{
		return cosmeticItemSo;
	}
}
