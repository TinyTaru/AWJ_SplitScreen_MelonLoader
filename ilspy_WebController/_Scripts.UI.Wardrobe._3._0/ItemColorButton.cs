using System;
using MPUIKIT;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UI.Wardrobe._3._0;

public class ItemColorButton : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Button button;

	[SerializeField]
	private MPImage image;

	[SerializeField]
	private TextMeshProUGUI label;

	private CosmeticItemType cosmetic;

	private int index;

	private void ApplyColor()
	{
		switch (cosmetic)
		{
		case CosmeticItemType.Hat:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadHatColor(index);
			break;
		case CosmeticItemType.Accessory:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadAccessoryColor(index);
			break;
		case CosmeticItemType.Shoe:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadShoeColor(index);
			break;
		case CosmeticItemType.Eye:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadEyeColor(index);
			break;
		default:
			Debug.LogWarning("Unhandled cosmetic type: " + cosmetic);
			break;
		}
	}

	public void Setup(CosmeticItemSo cosmeticItemSo, int newIndex)
	{
		index = newIndex;
		if (!(cosmeticItemSo is CosmeticItemHatSo))
		{
			if (!(cosmeticItemSo is CosmeticItemAccessorySo))
			{
				if (!(cosmeticItemSo is CosmeticItemShoeSo))
				{
					if (cosmeticItemSo is CosmeticItemEyeSo)
					{
						cosmetic = CosmeticItemType.Eye;
					}
				}
				else
				{
					cosmetic = CosmeticItemType.Shoe;
				}
			}
			else
			{
				cosmetic = CosmeticItemType.Accessory;
			}
		}
		else
		{
			cosmetic = CosmeticItemType.Hat;
		}
		if (label != null)
		{
			label.text = DialogueManager.GetLocalizedText($"Wardrobe_Generic_Color_{index + 1}");
		}
		ColorSelectorV3.Colorable partToColor = cosmetic switch
		{
			CosmeticItemType.Hat => ColorSelectorV3.Colorable.Hat, 
			CosmeticItemType.Accessory => ColorSelectorV3.Colorable.Accessory, 
			CosmeticItemType.Shoe => ColorSelectorV3.Colorable.Shoe, 
			CosmeticItemType.Eye => index switch
			{
				1 => ColorSelectorV3.Colorable.EyeLeft, 
				2 => ColorSelectorV3.Colorable.EyeRight, 
				_ => ColorSelectorV3.Colorable.EyeBase, 
			}, 
			CosmeticItemType.Web => throw new ArgumentOutOfRangeException(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		WardrobeControllerV3 wardrobeControllerV3 = GetComponentInParent<WardrobeControllerV3>();
		button.onClick.AddListener(delegate
		{
			wardrobeControllerV3.OpenColorSelectorPanel(partToColor, index);
		});
		ApplyColor();
	}

	public Button GetButton()
	{
		return button;
	}
}
