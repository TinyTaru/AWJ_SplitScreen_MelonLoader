using System;
using MPUIKIT;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.General;

namespace _Scripts.UI.Shop;

public class ShopButton : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private MPImage cosmeticItemImage;

	[SerializeField]
	private Button button;

	[SerializeField]
	private Animator animator;

	private ShopSelector shopSelector;

	private CosmeticItemSo cosmeticItemSo;

	private Color webColor;

	public void Setup(ShopSelector selector, CosmeticItemSo item, CosmeticItemType cosmeticItemType)
	{
		shopSelector = selector;
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
	}

	public void SelectButton()
	{
		shopSelector.CosmeticItemSelected(cosmeticItemSo, cosmeticItemImage, this);
	}

	public Button GetButton()
	{
		return button;
	}
}
