using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UI.Wardrobe;

public class CosmeticColorController : MonoBehaviour
{
	private enum Section
	{
		Color1,
		Color2,
		Color3
	}

	[Header("References")]
	[SerializeField]
	private Transform buttonContainer;

	[Header("Parameters")]
	[SerializeField]
	private Section section;

	[SerializeField]
	private CosmeticItemType cosmetic;

	private List<Button> itemColors;

	private void OnEnable()
	{
		itemColors = buttonContainer.GetComponentsInChildren<Button>().ToList();
		int index = LoadItem();
		int num = 0;
		switch (cosmetic)
		{
		case CosmeticItemType.Hat:
			num = ((CosmeticItemHatSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index)).hatSo.hat.NumberOfColors;
			break;
		case CosmeticItemType.Accessory:
			num = ((CosmeticItemAccessorySo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index)).accessorySo.accessory.NumberOfColors;
			break;
		case CosmeticItemType.Shoe:
			num = ((CosmeticItemShoeSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index)).shoeSo.shoe.NumberOfColors;
			break;
		case CosmeticItemType.Eye:
			num = ((CosmeticItemEyeSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index)).eyeSo.eye.NumberOfColors;
			break;
		default:
			Debug.LogWarning("No colors available");
			break;
		case CosmeticItemType.Web:
			break;
		}
		if (num == 0)
		{
			DisableButtons();
		}
		else if ((section == Section.Color1 && num >= 1) || (section == Section.Color2 && num >= 2) || (section == Section.Color3 && num >= 3))
		{
			itemColors[0].gameObject.SetActive(value: false);
		}
		else
		{
			DisableButtons();
		}
	}

	private void OnDisable()
	{
		EnableButtons();
	}

	private int LoadItem()
	{
		int result = 0;
		switch (cosmetic)
		{
		case CosmeticItemType.Hat:
			result = Singleton<CosmeticItemsController>.Instance.LoadHat();
			break;
		case CosmeticItemType.Accessory:
			result = Singleton<CosmeticItemsController>.Instance.LoadAccessory();
			break;
		case CosmeticItemType.Shoe:
			result = Singleton<CosmeticItemsController>.Instance.LoadShoe();
			break;
		case CosmeticItemType.Eye:
			result = Singleton<CosmeticItemsController>.Instance.LoadEye();
			break;
		default:
			Debug.LogWarning("Cosmetic color not available.");
			break;
		case CosmeticItemType.Web:
			break;
		}
		return result;
	}

	private void DisableButtons()
	{
		for (int i = 0; i < itemColors.Count; i++)
		{
			if (i != 0)
			{
				itemColors[i].gameObject.SetActive(value: false);
			}
		}
	}

	private void EnableButtons()
	{
		for (int i = 0; i < itemColors.Count; i++)
		{
			itemColors[i].gameObject.SetActive(value: true);
		}
	}
}
