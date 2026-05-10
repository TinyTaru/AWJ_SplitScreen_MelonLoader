using MPUIKIT;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UI.Wardrobe._3._0;

public class CosmeticItemButton : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Button button;

	[SerializeField]
	private MPImage image;

	[SerializeField]
	private TextMeshProUGUI itemLabel;

	[Header("Parameters")]
	[SerializeField]
	private CosmeticItemType cosmetic;

	private void Awake()
	{
		WardrobeControllerV3 wardrobeControllerV3 = GetComponentInParent<WardrobeControllerV3>();
		button.onClick.AddListener(delegate
		{
			wardrobeControllerV3.OpenItemSelectorPanel(cosmetic);
		});
	}

	private void OnEnable()
	{
		LoadItem();
	}

	private void LoadItem()
	{
		switch (cosmetic)
		{
		case CosmeticItemType.Hat:
			LoadHat();
			break;
		case CosmeticItemType.Accessory:
			LoadAccessory();
			break;
		case CosmeticItemType.Shoe:
			LoadShoe();
			break;
		case CosmeticItemType.Eye:
			LoadEye();
			break;
		default:
			Debug.LogWarning("Unhandled cosmetic type: " + cosmetic);
			break;
		}
	}

	private void LoadHat()
	{
		int index = Singleton<CosmeticItemsController>.Instance.LoadHat();
		CosmeticItemSo cosmeticItem = Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index);
		image.sprite = ((CosmeticItemHatSo)cosmeticItem).hatSo.hatSprite;
		itemLabel.text = DialogueManager.GetLocalizedText(cosmeticItem.displayName);
	}

	private void LoadAccessory()
	{
		int index = Singleton<CosmeticItemsController>.Instance.LoadAccessory();
		CosmeticItemSo cosmeticItem = Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index);
		image.sprite = ((CosmeticItemAccessorySo)cosmeticItem).accessorySo.accessorySprite;
		itemLabel.text = DialogueManager.GetLocalizedText(cosmeticItem.displayName);
	}

	private void LoadShoe()
	{
		int index = Singleton<CosmeticItemsController>.Instance.LoadShoe();
		CosmeticItemSo cosmeticItem = Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index);
		image.sprite = ((CosmeticItemShoeSo)cosmeticItem).shoeSo.shoeSprite;
		itemLabel.text = DialogueManager.GetLocalizedText(cosmeticItem.displayName);
	}

	private void LoadEye()
	{
		int index = Singleton<CosmeticItemsController>.Instance.LoadEye();
		CosmeticItemSo cosmeticItem = Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index);
		image.sprite = ((CosmeticItemEyeSo)cosmeticItem).eyeSo.eyeSprite;
		itemLabel.text = DialogueManager.GetLocalizedText(cosmeticItem.displayName);
	}
}
