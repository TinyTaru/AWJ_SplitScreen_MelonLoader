using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.LivingRoom;

public class TabletCartItemUI : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private TextMeshProUGUI amountText;

	public void Setup(TabletShopItem shopItem)
	{
		image.sprite = shopItem.TabletShopItemSo.sprite;
		amountText.text = $"x{shopItem.Amount}";
	}
}
