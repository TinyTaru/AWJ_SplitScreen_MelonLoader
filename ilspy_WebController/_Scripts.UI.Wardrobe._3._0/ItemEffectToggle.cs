using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.Toggles;
using _Scripts.Wardrobe;

namespace _Scripts.UI.Wardrobe._3._0;

public class ItemEffectToggle : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private AnimatedToggle animatedToggle;

	[SerializeField]
	private Button button;

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
						animatedToggle.SetInitialState(Mathf.Approximately(Singleton<CosmeticItemsController>.Instance.LoadEyeEffect(index), 1f));
					}
				}
				else
				{
					animatedToggle.SetInitialState(Mathf.Approximately(Singleton<CosmeticItemsController>.Instance.LoadShoeEffect(index), 1f));
				}
			}
			else
			{
				animatedToggle.SetInitialState(Mathf.Approximately(Singleton<CosmeticItemsController>.Instance.LoadAccessoryEffect(index), 1f));
			}
		}
		else
		{
			animatedToggle.SetInitialState(Mathf.Approximately(Singleton<CosmeticItemsController>.Instance.LoadHatEffect(index), 1f));
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
			ApplyValue();
		}
	}

	public void SetValue(bool value)
	{
		float effectValue = (value ? 1f : 0f);
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
							x.SetEyeEffect(index, effectValue);
						});
						Singleton<CosmeticItemsController>.Instance.SaveEyeEffect(index, effectValue);
					}
				}
				else
				{
					wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
					{
						x.SetShoeEffect(index, effectValue);
					});
					Singleton<CosmeticItemsController>.Instance.SaveShoeEffect(index, effectValue);
				}
			}
			else
			{
				wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
				{
					x.SetAccessoryEffect(index, effectValue);
				});
				Singleton<CosmeticItemsController>.Instance.SaveAccessoryEffect(index, effectValue);
			}
		}
		else
		{
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetHatEffect(index, effectValue);
			});
			Singleton<CosmeticItemsController>.Instance.SaveHatEffect(index, effectValue);
		}
	}

	public Button GetButton()
	{
		return button;
	}
}
