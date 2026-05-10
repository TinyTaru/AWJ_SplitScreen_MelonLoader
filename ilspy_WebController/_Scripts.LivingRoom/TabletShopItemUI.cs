using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.LivingRoom;

public class TabletShopItemUI : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private TextMeshProUGUI amountText;

	[SerializeField]
	private TextMeshProUGUI priceText;

	private TabletShopItem shopItem;

	private int amount;

	private int maxAmount;

	private void Start()
	{
		UpdateAmountText();
	}

	private void UpdateAmountText()
	{
		amountText.text = amount.ToString();
	}

	public void Setup(TabletShopItem newShopItem)
	{
		shopItem = newShopItem;
		image.sprite = shopItem.TabletShopItemSo.sprite;
		priceText.text = $"$ {shopItem.TabletShopItemSo.price}";
		maxAmount = shopItem.TabletShopItemSo.maxAmount;
		amount = shopItem.Amount;
		UpdateAmountText();
	}

	public void DecreaseAmount()
	{
		amount = Mathf.Clamp(amount - 1, 0, maxAmount);
		shopItem.Amount = amount;
		UpdateAmountText();
	}

	public void IncreaseAmount()
	{
		amount = Mathf.Clamp(amount + 1, 0, maxAmount);
		shopItem.Amount = amount;
		UpdateAmountText();
	}
}
