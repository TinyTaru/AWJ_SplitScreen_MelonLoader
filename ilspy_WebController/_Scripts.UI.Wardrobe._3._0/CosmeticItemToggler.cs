using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.Wardrobe;

namespace _Scripts.UI.Wardrobe._3._0;

public class CosmeticItemToggler : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private CosmeticItemType cosmetic;

	[Header("Default Buttons")]
	[SerializeField]
	private Button exitButton;

	[SerializeField]
	private Button itemButton;

	[Header("Colors")]
	[SerializeField]
	private Transform colorButtonContainer;

	[SerializeField]
	private ItemColorButton itemColorButtonPrefab;

	[Header("Effects")]
	[SerializeField]
	private Transform effectContainer;

	[SerializeField]
	private ItemEffectSlider itemEffectSliderPrefab;

	[SerializeField]
	private ItemEffectToggle itemEffectTogglePrefab;

	private List<ItemColorButton> colorButtons;

	private List<ItemEffectSlider> effectSliders;

	private List<ItemEffectToggle> effectToggles;

	private List<Selectable> effectSelectables;

	private void Awake()
	{
		colorButtons = new List<ItemColorButton>();
		effectSliders = new List<ItemEffectSlider>();
		effectToggles = new List<ItemEffectToggle>();
		effectSelectables = new List<Selectable>();
	}

	private void OnEnable()
	{
		DeleteAllColorAndEffectButtons();
		switch (cosmetic)
		{
		case CosmeticItemType.Hat:
			SetupHatButtons();
			break;
		case CosmeticItemType.Accessory:
			SetupAccessoryButtons();
			break;
		case CosmeticItemType.Shoe:
			SetupShoeButtons();
			break;
		case CosmeticItemType.Eye:
			SetupEyeButtons();
			break;
		default:
			Debug.LogWarning("No colors available");
			break;
		}
	}

	private void DeleteAllColorAndEffectButtons()
	{
		foreach (ItemColorButton colorButton in colorButtons)
		{
			UnityEngine.Object.Destroy(colorButton.gameObject);
		}
		colorButtons = new List<ItemColorButton>();
		foreach (ItemEffectSlider effectSlider in effectSliders)
		{
			UnityEngine.Object.Destroy(effectSlider.gameObject);
		}
		effectSliders = new List<ItemEffectSlider>();
		foreach (ItemEffectToggle effectToggle in effectToggles)
		{
			UnityEngine.Object.Destroy(effectToggle.gameObject);
		}
		effectToggles = new List<ItemEffectToggle>();
		effectSelectables = new List<Selectable>();
	}

	private void SetupHatButtons()
	{
		int index = Singleton<CosmeticItemsController>.Instance.LoadHat();
		CosmeticItemHatSo cosmeticItemHatSo = (CosmeticItemHatSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index);
		Hat hat = cosmeticItemHatSo.hatSo.hat;
		colorButtonContainer.gameObject.SetActive(hat.NumberOfColors > 0);
		effectContainer.gameObject.SetActive(hat.NumberOfEffects > 0);
		for (int i = 0; i < hat.NumberOfColors; i++)
		{
			ItemColorButton itemColorButton = UnityEngine.Object.Instantiate(itemColorButtonPrefab, colorButtonContainer);
			itemColorButton.Setup(cosmeticItemHatSo, i);
			colorButtons.Add(itemColorButton);
		}
		for (int j = 0; j < hat.NumberOfEffects; j++)
		{
			if ((SettingsController.ArachnophobiaMode || hat.GetEffect(j).Mode != SpiderMode.Arachnophobia) && (!SettingsController.ArachnophobiaMode || hat.GetEffect(j).Mode != SpiderMode.Normal))
			{
				switch (hat.GetEffect(j).GetCosmeticItemEffectType)
				{
				case CosmeticItemEffectType.Toggle:
				{
					ItemEffectToggle itemEffectToggle = UnityEngine.Object.Instantiate(itemEffectTogglePrefab, effectContainer);
					itemEffectToggle.Setup(cosmeticItemHatSo, j);
					effectToggles.Add(itemEffectToggle);
					effectSelectables.Add(itemEffectToggle.GetButton());
					break;
				}
				case CosmeticItemEffectType.Slider:
				{
					ItemEffectSlider itemEffectSlider = UnityEngine.Object.Instantiate(itemEffectSliderPrefab, effectContainer);
					itemEffectSlider.Setup(cosmeticItemHatSo, j);
					effectSliders.Add(itemEffectSlider);
					effectSelectables.Add(itemEffectSlider.GetSlider());
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
				case CosmeticItemEffectType.None:
					break;
				}
			}
		}
		SetupNavigation();
	}

	private void SetupAccessoryButtons()
	{
		int index = Singleton<CosmeticItemsController>.Instance.LoadAccessory();
		CosmeticItemAccessorySo cosmeticItemAccessorySo = (CosmeticItemAccessorySo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index);
		Accessory accessory = cosmeticItemAccessorySo.accessorySo.accessory;
		colorButtonContainer.gameObject.SetActive(accessory.NumberOfColors > 0);
		effectContainer.gameObject.SetActive(accessory.NumberOfEffects > 0);
		for (int i = 0; i < accessory.NumberOfColors; i++)
		{
			ItemColorButton itemColorButton = UnityEngine.Object.Instantiate(itemColorButtonPrefab, colorButtonContainer);
			itemColorButton.Setup(cosmeticItemAccessorySo, i);
			colorButtons.Add(itemColorButton);
		}
		for (int j = 0; j < accessory.NumberOfEffects; j++)
		{
			if ((SettingsController.ArachnophobiaMode || accessory.GetEffect(j).Mode != SpiderMode.Arachnophobia) && (!SettingsController.ArachnophobiaMode || accessory.GetEffect(j).Mode != SpiderMode.Normal))
			{
				switch (accessory.GetEffect(j).GetCosmeticItemEffectType)
				{
				case CosmeticItemEffectType.Toggle:
				{
					ItemEffectToggle itemEffectToggle = UnityEngine.Object.Instantiate(itemEffectTogglePrefab, effectContainer);
					itemEffectToggle.Setup(cosmeticItemAccessorySo, j);
					effectToggles.Add(itemEffectToggle);
					effectSelectables.Add(itemEffectToggle.GetButton());
					break;
				}
				case CosmeticItemEffectType.Slider:
				{
					ItemEffectSlider itemEffectSlider = UnityEngine.Object.Instantiate(itemEffectSliderPrefab, effectContainer);
					itemEffectSlider.Setup(cosmeticItemAccessorySo, j);
					effectSliders.Add(itemEffectSlider);
					effectSelectables.Add(itemEffectSlider.GetSlider());
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
				case CosmeticItemEffectType.None:
					break;
				}
			}
		}
		SetupNavigation();
	}

	private void SetupShoeButtons()
	{
		int index = Singleton<CosmeticItemsController>.Instance.LoadShoe();
		CosmeticItemShoeSo cosmeticItemShoeSo = (CosmeticItemShoeSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index);
		Shoe shoe = cosmeticItemShoeSo.shoeSo.shoe;
		colorButtonContainer.gameObject.SetActive(shoe.NumberOfColors > 0);
		effectContainer.gameObject.SetActive(shoe.NumberOfEffects > 0);
		for (int i = 0; i < shoe.NumberOfColors; i++)
		{
			ItemColorButton itemColorButton = UnityEngine.Object.Instantiate(itemColorButtonPrefab, colorButtonContainer);
			itemColorButton.Setup(cosmeticItemShoeSo, i);
			colorButtons.Add(itemColorButton);
		}
		for (int j = 0; j < shoe.NumberOfEffects; j++)
		{
			if ((SettingsController.ArachnophobiaMode || shoe.GetEffect(j).Mode != SpiderMode.Arachnophobia) && (!SettingsController.ArachnophobiaMode || shoe.GetEffect(j).Mode != SpiderMode.Normal))
			{
				switch (shoe.GetEffect(j).GetCosmeticItemEffectType)
				{
				case CosmeticItemEffectType.Toggle:
				{
					ItemEffectToggle itemEffectToggle = UnityEngine.Object.Instantiate(itemEffectTogglePrefab, effectContainer);
					itemEffectToggle.Setup(cosmeticItemShoeSo, j);
					effectToggles.Add(itemEffectToggle);
					effectSelectables.Add(itemEffectToggle.GetButton());
					break;
				}
				case CosmeticItemEffectType.Slider:
				{
					ItemEffectSlider itemEffectSlider = UnityEngine.Object.Instantiate(itemEffectSliderPrefab, effectContainer);
					itemEffectSlider.Setup(cosmeticItemShoeSo, j);
					effectSliders.Add(itemEffectSlider);
					effectSelectables.Add(itemEffectSlider.GetSlider());
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
				case CosmeticItemEffectType.None:
					break;
				}
			}
		}
		SetupNavigation();
	}

	private void SetupEyeButtons()
	{
		int index = Singleton<CosmeticItemsController>.Instance.LoadEye();
		CosmeticItemEyeSo cosmeticItemEyeSo = (CosmeticItemEyeSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index);
		Eye eye = cosmeticItemEyeSo.eyeSo.eye;
		colorButtonContainer.gameObject.SetActive(eye.NumberOfColors > 0);
		effectContainer.gameObject.SetActive(eye.NumberOfEffects > 0);
		for (int i = 0; i < eye.NumberOfColors; i++)
		{
			ItemColorButton itemColorButton = UnityEngine.Object.Instantiate(itemColorButtonPrefab, colorButtonContainer);
			itemColorButton.Setup(cosmeticItemEyeSo, i);
			colorButtons.Add(itemColorButton);
		}
		for (int j = 0; j < eye.NumberOfEffects; j++)
		{
			if ((SettingsController.ArachnophobiaMode || eye.GetEffect(j).Mode != SpiderMode.Arachnophobia) && (!SettingsController.ArachnophobiaMode || eye.GetEffect(j).Mode != SpiderMode.Normal))
			{
				switch (eye.GetEffect(j).GetCosmeticItemEffectType)
				{
				case CosmeticItemEffectType.Toggle:
				{
					ItemEffectToggle itemEffectToggle = UnityEngine.Object.Instantiate(itemEffectTogglePrefab, effectContainer);
					itemEffectToggle.Setup(cosmeticItemEyeSo, j);
					effectToggles.Add(itemEffectToggle);
					effectSelectables.Add(itemEffectToggle.GetButton());
					break;
				}
				case CosmeticItemEffectType.Slider:
				{
					ItemEffectSlider itemEffectSlider = UnityEngine.Object.Instantiate(itemEffectSliderPrefab, effectContainer);
					itemEffectSlider.Setup(cosmeticItemEyeSo, j);
					effectSliders.Add(itemEffectSlider);
					effectSelectables.Add(itemEffectSlider.GetSlider());
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
				case CosmeticItemEffectType.None:
					break;
				}
			}
		}
		SetupNavigation();
	}

	private void SetupNavigation()
	{
		Navigation navigation = default(Navigation);
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnDown = itemButton;
		Navigation navigation2 = navigation;
		exitButton.navigation = navigation2;
		navigation = default(Navigation);
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnUp = exitButton;
		navigation.selectOnDown = ((colorButtons.Count > 0) ? colorButtons[0].GetButton() : ((effectSelectables.Count > 0) ? effectSelectables[0] : null));
		Navigation navigation3 = navigation;
		itemButton.navigation = navigation3;
		for (int i = 0; i < colorButtons.Count; i++)
		{
			navigation = default(Navigation);
			navigation.mode = Navigation.Mode.Explicit;
			navigation.selectOnUp = itemButton;
			navigation.selectOnDown = ((effectSelectables.Count > 0) ? effectSelectables[0] : null);
			navigation.selectOnLeft = ((i > 0) ? colorButtons[i - 1].GetButton() : null);
			navigation.selectOnRight = ((i < colorButtons.Count - 1) ? colorButtons[i + 1].GetButton() : null);
			Navigation navigation4 = navigation;
			colorButtons[i].GetButton().navigation = navigation4;
		}
		for (int j = 0; j < effectSelectables.Count; j++)
		{
			navigation = default(Navigation);
			navigation.mode = Navigation.Mode.Explicit;
			navigation.selectOnDown = ((j < effectSelectables.Count - 1) ? effectSelectables[j + 1] : null);
			Navigation navigation5 = navigation;
			if (j == 0)
			{
				navigation5.selectOnUp = ((colorButtons.Count > 0) ? colorButtons[0].GetButton() : itemButton);
			}
			else
			{
				navigation5.selectOnUp = effectSelectables[j - 1];
			}
			effectSelectables[j].navigation = navigation5;
		}
	}
}
