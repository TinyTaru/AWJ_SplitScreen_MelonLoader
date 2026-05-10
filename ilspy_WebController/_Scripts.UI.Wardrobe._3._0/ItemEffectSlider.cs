using System.Globalization;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.Utils;
using _Scripts.Wardrobe;

namespace _Scripts.UI.Wardrobe._3._0;

public class ItemEffectSlider : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private GranularSlider slider;

	[SerializeField]
	private TextMeshProUGUI minValue;

	[SerializeField]
	private TextMeshProUGUI maxValue;

	private WardrobeControllerV3 wardrobeController;

	private CosmeticItemSo cosmeticItemSo;

	private int index;

	private void Awake()
	{
		wardrobeController = GetComponentInParent<WardrobeControllerV3>();
	}

	private void OnEnable()
	{
		ApplyValue();
	}

	private void ApplyValue()
	{
		CosmeticItemSo cosmeticItemSo = this.cosmeticItemSo;
		if (!(cosmeticItemSo is CosmeticItemHatSo))
		{
			if (!(cosmeticItemSo is CosmeticItemAccessorySo))
			{
				if (!(cosmeticItemSo is CosmeticItemShoeSo))
				{
					if (cosmeticItemSo is CosmeticItemEyeSo)
					{
						slider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadEyeEffect(index));
					}
				}
				else
				{
					slider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadShoeEffect(index));
				}
			}
			else
			{
				slider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadAccessoryEffect(index));
			}
		}
		else
		{
			slider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadHatEffect(index));
		}
	}

	public void Setup(CosmeticItemSo newCosmeticItemSo, int newIndex)
	{
		cosmeticItemSo = newCosmeticItemSo;
		index = newIndex;
		CosmeticItemEffect cosmeticItemEffect = null;
		if (!(newCosmeticItemSo is CosmeticItemHatSo cosmeticItemHatSo))
		{
			if (!(newCosmeticItemSo is CosmeticItemAccessorySo cosmeticItemAccessorySo))
			{
				if (!(newCosmeticItemSo is CosmeticItemShoeSo cosmeticItemShoeSo))
				{
					if (newCosmeticItemSo is CosmeticItemEyeSo cosmeticItemEyeSo)
					{
						cosmeticItemEffect = cosmeticItemEyeSo.eyeSo.eye.GetEffect(index);
					}
				}
				else
				{
					cosmeticItemEffect = cosmeticItemShoeSo.shoeSo.shoe.GetEffect(index);
				}
			}
			else
			{
				cosmeticItemEffect = cosmeticItemAccessorySo.accessorySo.accessory.GetEffect(index);
			}
		}
		else
		{
			cosmeticItemEffect = cosmeticItemHatSo.hatSo.hat.GetEffect(index);
		}
		if (cosmeticItemEffect != null)
		{
			label.text = DialogueManager.GetLocalizedText("Wardrobe_Generic_" + ((cosmeticItemEffect.Name == "") ? (index + 1).ToString() : cosmeticItemEffect.Name));
			minValue.text = cosmeticItemEffect.MinValue.ToString(CultureInfo.InvariantCulture);
			maxValue.text = cosmeticItemEffect.MaxValue.ToString(CultureInfo.InvariantCulture);
			slider.minValue = cosmeticItemEffect.MinValue;
			slider.maxValue = cosmeticItemEffect.MaxValue;
			slider.wholeNumbers = cosmeticItemEffect.IntegerValuesOnly;
			slider.SetMoveStep(cosmeticItemEffect.IntegerValuesOnly ? 1f : cosmeticItemEffect.SliderMoveStep);
			slider.onValueChanged.AddListener(SetValue);
			ApplyValue();
		}
	}

	public void SetValue(float value)
	{
		CosmeticItemSo cosmeticItemSo = this.cosmeticItemSo;
		if (!(cosmeticItemSo is CosmeticItemHatSo))
		{
			if (!(cosmeticItemSo is CosmeticItemAccessorySo))
			{
				if (!(cosmeticItemSo is CosmeticItemShoeSo))
				{
					if (cosmeticItemSo is CosmeticItemEyeSo)
					{
						wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
						{
							x.SetEyeEffect(index, value);
						});
						Singleton<CosmeticItemsController>.Instance.SaveEyeEffect(index, value);
					}
				}
				else
				{
					wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
					{
						x.SetShoeEffect(index, value);
					});
					Singleton<CosmeticItemsController>.Instance.SaveShoeEffect(index, value);
				}
			}
			else
			{
				wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
				{
					x.SetAccessoryEffect(index, value);
				});
				Singleton<CosmeticItemsController>.Instance.SaveAccessoryEffect(index, value);
			}
		}
		else
		{
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetHatEffect(index, value);
			});
			Singleton<CosmeticItemsController>.Instance.SaveHatEffect(index, value);
		}
	}

	public Slider GetSlider()
	{
		return slider;
	}
}
