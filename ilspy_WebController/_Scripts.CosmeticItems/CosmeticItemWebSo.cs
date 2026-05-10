using UnityEngine;
using _Scripts.Wardrobe;

namespace _Scripts.CosmeticItems;

[CreateAssetMenu(menuName = "FTG/Cosmetic Item/New Web", fileName = "New Web", order = 0)]
public class CosmeticItemWebSo : CosmeticItemSo
{
	[Header("Web specific Stuff")]
	public WebSo webSo;
}
