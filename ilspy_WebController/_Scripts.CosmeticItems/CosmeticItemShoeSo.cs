using UnityEngine;
using _Scripts.Wardrobe;

namespace _Scripts.CosmeticItems;

[CreateAssetMenu(menuName = "FTG/Cosmetic Item/New Shoes", fileName = "New Shoes", order = 0)]
public class CosmeticItemShoeSo : CosmeticItemSo
{
	[Header("Shoe specific Stuff")]
	public ShoeSo shoeSo;
}
