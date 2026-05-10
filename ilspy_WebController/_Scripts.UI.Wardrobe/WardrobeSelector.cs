using System.Collections.Generic;
using UnityEngine;
using _Scripts.Achievements;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.Wardrobe;

namespace _Scripts.UI.Wardrobe;

public class WardrobeSelector : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private Transform container;

	[SerializeField]
	private CosmeticItemType cosmetic;

	[SerializeField]
	private WardrobeButton cosmeticPrefab;

	private WardrobeController wardrobeController;

	private List<WardrobeButton> buttons = new List<WardrobeButton>();

	private WardrobeButton lastActive;

	private void OnEnable()
	{
		SetCorrectItem();
	}

	private void OnDisable()
	{
		foreach (WardrobeButton button in buttons)
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
		buttons = new List<WardrobeButton>();
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

	private WardrobeButton GetActiveButton(CosmeticItemSo cosmeticItemSo)
	{
		foreach (WardrobeButton button in buttons)
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
			for (int l = 0; l < 6; l++)
			{
				if (l < hat.NumberOfColors)
				{
					Singleton<CosmeticItemsController>.Instance.SaveHatColor(l, hat.GetDefaultColor(l));
				}
				else
				{
					Singleton<CosmeticItemsController>.Instance.DeleteHatColor(l);
				}
				if (l < hat.NumberOfEffects)
				{
					Singleton<CosmeticItemsController>.Instance.SaveHatEffect(l, hat.GetDefaultEffect(l));
				}
				else
				{
					Singleton<CosmeticItemsController>.Instance.DeleteHatEffect(l);
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
			Singleton<CosmeticItemsController>.Instance.SaveShoe(shoeItemIndex);
			Shoe shoe = ((CosmeticItemShoeSo)cosmeticItemSo).shoeSo.shoe;
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
			Singleton<CosmeticItemsController>.Instance.SaveEye(eyeItemIndex);
			Singleton<CosmeticItemsController>.Instance.SaveEyeColorBase(((CosmeticItemEyeSo)cosmeticItemSo).eyeSo.eye.DefaultColorBase);
			Singleton<CosmeticItemsController>.Instance.SaveEyeColorLeft(((CosmeticItemEyeSo)cosmeticItemSo).eyeSo.eye.DefaultColorLeft);
			Singleton<CosmeticItemsController>.Instance.SaveEyeColorRight(((CosmeticItemEyeSo)cosmeticItemSo).eyeSo.eye.DefaultColorRight);
			Eye eye = ((CosmeticItemEyeSo)cosmeticItemSo).eyeSo.eye;
			for (int i = 0; i < 6; i++)
			{
				if (i < eye.NumberOfEffects)
				{
					Singleton<CosmeticItemsController>.Instance.SaveEyeEffect(i, eye.GetDefaultEffect(i));
				}
				else
				{
					Singleton<CosmeticItemsController>.Instance.DeleteEyeEffect(i);
				}
			}
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

	public void SetCorrectItem()
	{
		if (buttons.Count != 0)
		{
			CosmeticItemSo cosmeticItemSo = LoadItem();
			WardrobeButton activeButton = GetActiveButton(cosmeticItemSo);
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

	public void Setup(WardrobeController newWardrobeController)
	{
		wardrobeController = newWardrobeController;
	}

	public void InitializeAccessoryButton()
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
					WardrobeButton wardrobeButton4 = Object.Instantiate(cosmeticPrefab, container);
					wardrobeButton4.Setup(this, item, cosmetic);
					buttons.Add(wardrobeButton4);
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
					WardrobeButton wardrobeButton2 = Object.Instantiate(cosmeticPrefab, container);
					wardrobeButton2.Setup(this, item2, cosmetic);
					buttons.Add(wardrobeButton2);
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
					WardrobeButton wardrobeButton3 = Object.Instantiate(cosmeticPrefab, container);
					wardrobeButton3.Setup(this, item3, cosmetic);
					buttons.Add(wardrobeButton3);
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
					WardrobeButton wardrobeButton = Object.Instantiate(cosmeticPrefab, container);
					wardrobeButton.Setup(this, item4, cosmetic);
					buttons.Add(wardrobeButton);
				}
				break;
			}
		}
		default:
			Debug.LogWarning("Accessory type not implemented: " + cosmetic);
			break;
		}
	}

	public void SetRandomItem()
	{
		CosmeticItemSo cosmeticItemSo = ScriptableObject.CreateInstance<CosmeticItemSo>();
		switch (cosmetic)
		{
		case CosmeticItemType.Accessory:
		{
			List<CosmeticItemAccessorySo> unlockedItems2 = Singleton<CosmeticItemsController>.Instance.GetUnlockedItems<CosmeticItemAccessorySo>();
			if (unlockedItems2.Count > 0)
			{
				cosmeticItemSo = unlockedItems2[Random.Range(0, unlockedItems2.Count)];
			}
			break;
		}
		case CosmeticItemType.Hat:
		{
			List<CosmeticItemHatSo> unlockedItems3 = Singleton<CosmeticItemsController>.Instance.GetUnlockedItems<CosmeticItemHatSo>();
			if (unlockedItems3.Count > 0)
			{
				cosmeticItemSo = unlockedItems3[Random.Range(0, unlockedItems3.Count)];
			}
			break;
		}
		case CosmeticItemType.Shoe:
		{
			List<CosmeticItemShoeSo> unlockedItems = Singleton<CosmeticItemsController>.Instance.GetUnlockedItems<CosmeticItemShoeSo>();
			if (unlockedItems.Count > 0)
			{
				cosmeticItemSo = unlockedItems[Random.Range(0, unlockedItems.Count)];
			}
			break;
		}
		case CosmeticItemType.Eye:
			cosmeticItemSo = Singleton<CosmeticItemsController>.Instance.GetDefaultEye();
			break;
		default:
			Debug.LogWarning("Accessory type not implemented: " + cosmetic);
			return;
		}
		ChangeItem(cosmeticItemSo, saveItem: true, playSound: false, isRandom: true);
		SetCorrectItem();
	}

	public void SetDefaultItem()
	{
		CosmeticItemSo cosmeticItemSo = ScriptableObject.CreateInstance<CosmeticItemSo>();
		switch (cosmetic)
		{
		case CosmeticItemType.Accessory:
			cosmeticItemSo = Singleton<CosmeticItemsController>.Instance.GetDefaultAccessory();
			break;
		case CosmeticItemType.Hat:
			cosmeticItemSo = Singleton<CosmeticItemsController>.Instance.GetDefaultHat();
			break;
		case CosmeticItemType.Shoe:
			cosmeticItemSo = Singleton<CosmeticItemsController>.Instance.GetDefaultShoe();
			break;
		case CosmeticItemType.Eye:
			cosmeticItemSo = Singleton<CosmeticItemsController>.Instance.GetDefaultEye();
			break;
		default:
			Debug.LogWarning("Accessory type not implemented: " + cosmetic);
			return;
		}
		ChangeItem(cosmeticItemSo, saveItem: true, playSound: false);
		SetCorrectItem();
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
