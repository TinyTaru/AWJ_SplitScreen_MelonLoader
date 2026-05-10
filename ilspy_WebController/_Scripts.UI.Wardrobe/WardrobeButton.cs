using System;
using MPUIKIT;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UI.Wardrobe;

public class WardrobeButton : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private MPImage cosmeticItemImage;

	[SerializeField]
	private Button button;

	private WardrobeSelector wardrobeSelector;

	private CosmeticItemSo cosmeticItemSo;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void ButtonClick()
	{
		wardrobeSelector.ChangeItem(cosmeticItemSo, saveItem: true);
	}

	public void Setup(WardrobeSelector selector, CosmeticItemSo item, CosmeticItemType cosmeticItemType)
	{
		wardrobeSelector = selector;
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
		button.onClick.AddListener(SelectButton);
		EventTrigger component = button.GetComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry
		{
			eventID = EventTriggerType.Select
		};
		entry.callback.AddListener(delegate
		{
			SelectButton();
		});
		component.triggers.Add(entry);
		button.onClick.AddListener(delegate
		{
			ButtonClick();
		});
	}

	public void SelectButton()
	{
		wardrobeSelector.ChangeItem(cosmeticItemSo);
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
