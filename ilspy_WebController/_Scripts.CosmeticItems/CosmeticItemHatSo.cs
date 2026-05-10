using UnityEngine;
using UnityEngine.Serialization;
using _Scripts.Wardrobe;

namespace _Scripts.CosmeticItems;

[CreateAssetMenu(menuName = "FTG/Cosmetic Item/New Hat", fileName = "New Hat", order = 0)]
public class CosmeticItemHatSo : CosmeticItemSo
{
	[Header("Hat specific Stuff")]
	[FormerlySerializedAs("hatSO")]
	public HatSo hatSo;
}
