using System.Collections.Generic;
using UnityEngine;
using _Scripts.Achievements;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.Wardrobe;

namespace _Scripts.UI.Wardrobe._3._0;

public class ItemSelectorV3 : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private Transform container;

	[SerializeField]
	private CosmeticItemType cosmetic;

	[SerializeField]
	private WardrobeButtonV3 cosmeticPrefab;

	private WardrobeControllerV3 wardrobeController;

	private List<WardrobeButtonV3> buttons = new List<WardrobeButtonV3>();

	private WardrobeButtonV3 lastActive;

	private void OnEnable()
	{
		wardrobeController = GetComponentInParent<WardrobeControllerV3>();
		InitializeItemButtons();
		SetCorrectItem();
	}

	private void OnDisable()
	{
		foreach (WardrobeButtonV3 button in buttons)
		{
			button.GetButton().OnDeselect(null);
		}
		SceneExit();
	}

	private void ClearButtons()
	{
		for (int num = buttons.Count - 1; num >= 0; num--)
		{
			Object.Destroy(buttons[num].gameObject);
		}
		buttons = new List<WardrobeButtonV3>();
	}

	private CosmeticItemSo LoadItem()
	{
		int num = 0;
		switch (cosmetic)
		{
		case CosmeticItemType.Hat:
			num = Singleton<CosmeticItemsController>.Instance.LoadHat();
			return Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(num);
		case CosmeticItemType.Accessory:
			num = Singleton<CosmeticItemsController>.Instance.LoadAccessory();
			return Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(num);
		case CosmeticItemType.Shoe:
			num = Singleton<CosmeticItemsController>.Instance.LoadShoe();
			return Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(num);
		case CosmeticItemType.Web:
			return null;
		case CosmeticItemType.Eye:
			num = Singleton<CosmeticItemsController>.Instance.LoadEye();
			return Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(num);
		default:
			Debug.LogWarning("Accessory type not implemented: " + cosmetic);
			return null;
		}
	}

	private WardrobeButtonV3 GetActiveButton(CosmeticItemSo cosmeticItemSo)
	{
		foreach (WardrobeButtonV3 button in buttons)
		{
			if (button.GetCosmeticItem() == cosmeticItemSo)
			{
				return button;
			}
		}
		return buttons[0];
	}

	public void ChangeItem(CosmeticItemSo cosmeticItemSo, bool saveItem = false, bool playSound = true, bool isRandom = false)
	{
		switch (cosmetic)
		{
		case CosmeticItemType.Accessory:
		{
			int accessoryItemIndex = Singleton<CosmeticItemsController>.Instance.GetItemIndex(cosmeticItemSo);
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.ChangeAccessory(accessoryItemIndex);
			});
			if (!saveItem)
			{
				return;
			}
			Singleton<CosmeticItemsController>.Instance.SaveAccessory(accessoryItemIndex);
			Accessory accessory = ((CosmeticItemAccessorySo)cosmeticItemSo).accessorySo.accessory;
			for (int j = 0; j < 6; j++)
			{
				if (j < accessory.NumberOfColors)
				{
					Singleton<CosmeticItemsController>.Instance.SaveAccessoryColor(j, accessory.GetDefaultColor(j));
				}
				else
				{
					Singleton<CosmeticItemsController>.Instance.DeleteAccessoryColor(j);
				}
				if (j < accessory.NumberOfEffects)
				{
					Singleton<CosmeticItemsController>.Instance.SaveAccessoryEffect(j, accessory.GetDefaultEffect(j));
				}
				else
				{
					Singleton<CosmeticItemsController>.Instance.DeleteAccessoryEffect(j);
				}
			}
			if (playSound)
			{
				Singleton<MusicController>.Instance.PlayAccessorySound(((CosmeticItemAccessorySo)cosmeticItemSo).accessorySo.accessorySound);
			}
			break;
		}
		case CosmeticItemType.Hat:
		{
			int hatItemIndex = Singleton<CosmeticItemsController>.Instance.GetItemIndex(cosmeticItemSo);
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.ChangeHat(hatItemIndex);
			});
			if (!saveItem)
			{
				return;
			}
			Singleton<CosmeticItemsController>.Instance.SaveHat(hatItemIndex);
			Hat hat = ((CosmeticItemHatSo)cosmeticItemSo).hatSo.hat;
			for (int i = 0; i < 6; i++)
			{
				if (i < hat.NumberOfColors)
				{
					Singleton<CosmeticItemsController>.Instance.SaveHatColor(i, hat.GetDefaultColor(i));
				}
				else
				{
					Singleton<CosmeticItemsController>.Instance.DeleteHatColor(i);
				}
				if (i < hat.NumberOfEffects)
				{
					Singleton<CosmeticItemsController>.Instance.SaveHatEffect(i, hat.GetDefaultEffect(i));
				}
				else
				{
					Singleton<CosmeticItemsController>.Instance.DeleteHatEffect(i);
				}
			}
			if (playSound)
			{
				Singleton<MusicController>.Instance.PlayHatSound(((CosmeticItemHatSo)cosmeticItemSo).hatSo.hatSound);
			}
			break;
		}
		case CosmeticItemType.Shoe:
		{
			int shoeItemIndex = Singleton<CosmeticItemsController>.Instance.GetItemIndex(cosmeticItemSo);
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.ChangeShoe(shoeItemIndex);
			});
			if (!saveItem)
			{
				return;
			}
			Shoe shoe = ((CosmeticItemShoeSo)cosmeticItemSo).shoeSo.shoe;
			Singleton<CosmeticItemsController>.Instance.SaveShoe(shoeItemIndex);
			for (int k = 0; k < 6; k++)
			{
				if (k < shoe.NumberOfColors)
				{
					Singleton<CosmeticItemsController>.Instance.SaveShoeColor(k, shoe.GetDefaultColor(k));
				}
				else
				{
					Singleton<CosmeticItemsController>.Instance.DeleteShoeColor(k);
				}
				if (k < shoe.NumberOfEffects)
				{
					Singleton<CosmeticItemsController>.Instance.SaveShoeEffect(k, shoe.GetDefaultEffect(k));
				}
				else
				{
					Singleton<CosmeticItemsController>.Instance.DeleteShoeEffect(k);
				}
			}
			if (playSound)
			{
				Singleton<MusicController>.Instance.PlayShoeSound(((CosmeticItemShoeSo)cosmeticItemSo).shoeSo.shoeSound);
			}
			break;
		}
		case CosmeticItemType.Eye:
		{
			int eyeItemIndex = Singleton<CosmeticItemsController>.Instance.GetItemIndex(cosmeticItemSo);
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.ChangeEye(eyeItemIndex);
			});
			if (!saveItem)
			{
				return;
			}
			_ = ((CosmeticItemEyeSo)cosmeticItemSo).eyeSo.eye;
			Singleton<CosmeticItemsController>.Instance.SaveEye(eyeItemIndex);
			if (playSound)
			{
				Singleton<MusicController>.Instance.PlayEyeSound(((CosmeticItemEyeSo)cosmeticItemSo).eyeSo.eyeSound);
			}
			break;
		}
		default:
			Debug.LogWarning("Accessory type not implemented: " + cosmetic);
			return;
		case CosmeticItemType.Web:
			break;
		}
		if (saveItem && !isRandom)
		{
			AchievementEvents.CosmeticItemChanged();
			if (!Singleton<GameController>.Instance.InputIsTouch)
			{
				wardrobeController.GoBack();
				return;
			}
		}
		if ((bool)lastActive)
		{
			lastActive.GetComponent<Animator>().SetBool("Loaded", value: false);
			lastActive.GetComponent<Animator>().SetTrigger("Normal");
			lastActive = GetActiveButton(cosmeticItemSo);
		}
		if (buttons.Count > 0)
		{
			GetActiveButton(cosmeticItemSo).GetComponent<Animator>().SetBool("Loaded", value: true);
		}
	}

	private void PerformAnimation()
	{
		foreach (SpiderCustomization spiderCustomization in wardrobeController.SpiderCustomizations)
		{
			if (!spiderCustomization.GetComponent<BodyMovement>().IsPlayer)
			{
				spiderCustomization.GetComponent<Animator>().SetTrigger("WardrobeEquip");
			}
		}
	}

	private void InitializeItemButtons()
	{
		ClearButtons();
		switch (cosmetic)
		{
		case CosmeticItemType.Accessory:
		{
			List<CosmeticItemAccessorySo> unlockedItems4 = Singleton<CosmeticItemsController>.Instance.GetUnlockedItems<CosmeticItemAccessorySo>();
			if (unlockedItems4.Count <= 0)
			{
				break;
			}
			{
				foreach (CosmeticItemAccessorySo item in unlockedItems4)
				{
					WardrobeButtonV3 wardrobeButtonV4 = Object.Instantiate(cosmeticPrefab, container);
					wardrobeButtonV4.Setup(this, item, cosmetic);
					buttons.Add(wardrobeButtonV4);
				}
				break;
			}
		}
		case CosmeticItemType.Hat:
		{
			List<CosmeticItemHatSo> unlockedItems2 = Singleton<CosmeticItemsController>.Instance.GetUnlockedItems<CosmeticItemHatSo>();
			if (unlockedItems2.Count <= 0)
			{
				break;
			}
			{
				foreach (CosmeticItemHatSo item2 in unlockedItems2)
				{
					WardrobeButtonV3 wardrobeButtonV2 = Object.Instantiate(cosmeticPrefab, container);
					wardrobeButtonV2.Setup(this, item2, cosmetic);
					buttons.Add(wardrobeButtonV2);
				}
				break;
			}
		}
		case CosmeticItemType.Shoe:
		{
			List<CosmeticItemShoeSo> unlockedItems3 = Singleton<CosmeticItemsController>.Instance.GetUnlockedItems<CosmeticItemShoeSo>();
			if (unlockedItems3.Count <= 0)
			{
				break;
			}
			{
				foreach (CosmeticItemShoeSo item3 in unlockedItems3)
				{
					WardrobeButtonV3 wardrobeButtonV3 = Object.Instantiate(cosmeticPrefab, container);
					wardrobeButtonV3.Setup(this, item3, cosmetic);
					buttons.Add(wardrobeButtonV3);
				}
				break;
			}
		}
		case CosmeticItemType.Eye:
		{
			List<CosmeticItemEyeSo> unlockedItems = Singleton<CosmeticItemsController>.Instance.GetUnlockedItems<CosmeticItemEyeSo>();
			if (unlockedItems.Count <= 0)
			{
				break;
			}
			{
				foreach (CosmeticItemEyeSo item4 in unlockedItems)
				{
					WardrobeButtonV3 wardrobeButtonV = Object.Instantiate(cosmeticPrefab, container);
					wardrobeButtonV.Setup(this, item4, cosmetic);
					buttons.Add(wardrobeButtonV);
				}
				break;
			}
		}
		default:
			Debug.LogWarning("Accessory type not implemented: " + cosmetic);
			break;
		}
	}

	public void SetCorrectItem()
	{
		if (buttons.Count != 0)
		{
			CosmeticItemSo cosmeticItemSo = LoadItem();
			WardrobeButtonV3 activeButton = GetActiveButton(cosmeticItemSo);
			if ((bool)lastActive)
			{
				lastActive.GetComponent<Animator>().SetBool("Loaded", value: false);
				lastActive = activeButton;
			}
			if (base.gameObject.activeInHierarchy)
			{
				activeButton.SelectButton();
				activeButton.GetButton().Select();
			}
			activeButton.GetComponent<Animator>().SetBool("Loaded", value: true);
			lastActive = activeButton;
		}
	}

	public void SetCosmeticItem(CosmeticItemType newCosmetic)
	{
		cosmetic = newCosmetic;
	}

	public void SceneExit()
	{
		switch (cosmetic)
		{
		case CosmeticItemType.Accessory:
		{
			int activeAccessoryIndex = Singleton<CosmeticItemsController>.Instance.LoadAccessory();
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.ChangeAccessory(activeAccessoryIndex);
			});
			for (int i = 0; i < 6; i++)
			{
				if (Singleton<CosmeticItemsController>.Instance.AccessoryColorExists(i))
				{
					Color color = Singleton<CosmeticItemsController>.Instance.LoadAccessoryColor(i);
					wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
					{
						x.SetAccessoryColor(i, color);
					});
				}
				if (Singleton<CosmeticItemsController>.Instance.AccessoryEffectExists(i))
				{
					float value = Singleton<CosmeticItemsController>.Instance.LoadAccessoryEffect(i);
					wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
					{
						x.SetAccessoryEffect(i, value);
					});
				}
			}
			break;
		}
		case CosmeticItemType.Hat:
		{
			int activeHatIndex = Singleton<CosmeticItemsController>.Instance.LoadHat();
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.ChangeHat(activeHatIndex);
			});
			for (int l = 0; l < 6; l++)
			{
				if (Singleton<CosmeticItemsController>.Instance.HatColorExists(l))
				{
					Color color3 = Singleton<CosmeticItemsController>.Instance.LoadHatColor(l);
					wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
					{
						x.SetHatColor(l, color3);
					});
				}
				if (Singleton<CosmeticItemsController>.Instance.HatEffectExists(l))
				{
					float value4 = Singleton<CosmeticItemsController>.Instance.LoadHatEffect(l);
					wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
					{
						x.SetHatEffect(l, value4);
					});
				}
			}
			break;
		}
		case CosmeticItemType.Shoe:
		{
			int activeShoeIndex = Singleton<CosmeticItemsController>.Instance.LoadShoe();
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.ChangeShoe(activeShoeIndex);
			});
			for (int j = 0; j < 6; j++)
			{
				if (Singleton<CosmeticItemsController>.Instance.ShoeColorExists(j))
				{
					Color color2 = Singleton<CosmeticItemsController>.Instance.LoadShoeColor(j);
					wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
					{
						x.SetShoeColor(j, color2);
					});
				}
				if (Singleton<CosmeticItemsController>.Instance.ShoeEffectExists(j))
				{
					float value2 = Singleton<CosmeticItemsController>.Instance.LoadShoeEffect(j);
					wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
					{
						x.SetShoeEffect(j, value2);
					});
				}
			}
			break;
		}
		case CosmeticItemType.Eye:
		{
			int activeEyeIndex = Singleton<CosmeticItemsController>.Instance.LoadEye();
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.ChangeEye(activeEyeIndex);
			});
			Color eyeColorBase = Singleton<CosmeticItemsController>.Instance.LoadEyeColorBase();
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetEyeColorBase(eyeColorBase);
			});
			Color eyeColorLeft = Singleton<CosmeticItemsController>.Instance.LoadEyeColorLeft();
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetEyeColorLeft(eyeColorLeft);
			});
			Color eyeColorRight = Singleton<CosmeticItemsController>.Instance.LoadEyeColorRight();
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetEyeColorRight(eyeColorRight);
			});
			for (int k = 0; k < 6; k++)
			{
				if (Singleton<CosmeticItemsController>.Instance.EyeEffectExists(k))
				{
					float value3 = Singleton<CosmeticItemsController>.Instance.LoadEyeEffect(k);
					wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
					{
						x.SetEyeEffect(k, value3);
					});
				}
			}
			break;
		}
		default:
			Debug.LogWarning("Accessory type not implemented: " + cosmetic);
			break;
		case CosmeticItemType.Web:
			break;
		}
	}
}
