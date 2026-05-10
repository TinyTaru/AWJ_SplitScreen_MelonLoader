using FMOD.Studio;
using FMODUnity;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.Singletons;

namespace _Scripts.UI.Shop;

public class ShopController : MonoBehaviour
{
	[Header("Item Selectors")]
	[SerializeField]
	private ShopSelector[] shopSelectors;

	[SerializeField]
	private Image activeCosmeticImage;

	[SerializeField]
	private Sprite defaultSprite;

	[SerializeField]
	private GameObject price;

	[SerializeField]
	private TextMeshProUGUI priceText;

	[SerializeField]
	private TextMeshProUGUI itemName;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference purchaseItemInputAction;

	[Header("Hold Time")]
	[SerializeField]
	private float requiredHoldTime = 3f;

	[Header("UI Elements")]
	[SerializeField]
	private Image progressBar;

	[SerializeField]
	private Color canBuyColor;

	[SerializeField]
	private Color cannotBuyColor;

	[SerializeField]
	private Button mobilePurchaseButton;

	[Header("Sounds")]
	[SerializeField]
	private EventReference purchaseSoundRef;

	private float holdTime;

	private CosmeticItemSo selectedItem;

	private bool purchaseInProgress;

	private EventInstance purchaseSound;

	private ShopSelector activeShopSelector;

	private void Awake()
	{
		purchaseInProgress = false;
		holdTime = 0f;
		purchaseSound = RuntimeManager.CreateInstance(purchaseSoundRef);
	}

	private void OnEnable()
	{
		SetDefaultSprite();
		ShopSelector[] array = shopSelectors;
		foreach (ShopSelector obj in array)
		{
			obj.InitializeButtons(this);
			obj.OnItemSelected += ShopController_OnItemSelected;
		}
		activeShopSelector = shopSelectors[0];
		activeShopSelector.SelectFirstButton();
	}

	private void OnDisable()
	{
		SetDefaultSprite();
		ShopSelector[] array = shopSelectors;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnItemSelected -= ShopController_OnItemSelected;
		}
	}

	private void Update()
	{
		if (purchaseItemInputAction.action.WasPressedThisFrame() && selectedItem != null && !Singleton<CosmeticItemsController>.Instance.IsItemUnlocked(selectedItem) && Singleton<CosmeticItemsController>.Instance.CanBuyItem(selectedItem))
		{
			purchaseInProgress = true;
			purchaseSound.start();
		}
		if (purchaseItemInputAction.action.WasReleasedThisFrame())
		{
			purchaseInProgress = false;
			purchaseSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			ResetFill();
		}
		if (purchaseInProgress)
		{
			holdTime += Time.unscaledDeltaTime;
			progressBar.fillAmount = Mathf.Clamp01(holdTime / requiredHoldTime);
			if (holdTime >= requiredHoldTime)
			{
				PurchaseItem();
			}
		}
	}

	private void ResetFill()
	{
		progressBar.fillAmount = 0f;
		holdTime = 0f;
	}

	private void PurchaseItem()
	{
		Singleton<CosmeticItemsController>.Instance.BuyItem(selectedItem);
		Singleton<CosmeticItemsController>.Instance.UnlockItem(selectedItem);
		purchaseInProgress = false;
		ResetFill();
		SetDefaultSprite();
		activeShopSelector.InitializeButtons(this);
		activeShopSelector.SelectFirstButton();
	}

	private void SetDefaultSprite()
	{
		selectedItem = null;
		activeCosmeticImage.sprite = null;
		activeCosmeticImage.color = new Color(1f, 1f, 1f, 0f);
		itemName.text = "";
		price.SetActive(value: false);
	}

	public void SetActiveShopSelector(ShopSelector shopSelector)
	{
		activeShopSelector = shopSelector;
		if (!activeShopSelector.HasButtons())
		{
			SetDefaultSprite();
		}
	}

	public void PurchaseMobile()
	{
		if (!(selectedItem == null) && Singleton<CosmeticItemsController>.Instance.CanBuyItem(selectedItem))
		{
			PurchaseItem();
		}
	}

	private void ShopController_OnItemSelected(CosmeticItemSo item, Image cosmeticImage, ShopButton button)
	{
		selectedItem = item;
		activeCosmeticImage.sprite = cosmeticImage.sprite;
		activeCosmeticImage.color = new Color(1f, 1f, 1f, 1f);
		itemName.text = DialogueManager.GetLocalizedText(selectedItem.displayName);
		priceText.text = selectedItem.price.ToString();
		priceText.color = (Singleton<CosmeticItemsController>.Instance.CanBuyItem(selectedItem) ? canBuyColor : cannotBuyColor);
		price.SetActive(value: true);
		purchaseSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		purchaseInProgress = false;
		ResetFill();
		mobilePurchaseButton.interactable = Singleton<CosmeticItemsController>.Instance.CanBuyItem(selectedItem);
	}
}
