using UnityEngine;
using _Scripts.Wardrobe;

namespace _Scripts.CosmeticItems;

[CreateAssetMenu(menuName = "FTG/Cosmetic Item/New Accessory", fileName = "New Accessory", order = 0)]
public class CosmeticItemAccessorySo : CosmeticItemSo
{
	[Header("Accessory specific Stuff")]
	public AccessorySo accessorySo;
}
