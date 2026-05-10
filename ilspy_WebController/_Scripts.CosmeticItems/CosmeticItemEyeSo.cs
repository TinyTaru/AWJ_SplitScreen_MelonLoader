using UnityEngine;
using _Scripts.Wardrobe;

namespace _Scripts.CosmeticItems;

[CreateAssetMenu(menuName = "FTG/Cosmetic Item/New Eye", fileName = "New Eye", order = 0)]
public class CosmeticItemEyeSo : CosmeticItemSo
{
	[Header("Eye specific Stuff")]
	public EyeSo eyeSo;
}
