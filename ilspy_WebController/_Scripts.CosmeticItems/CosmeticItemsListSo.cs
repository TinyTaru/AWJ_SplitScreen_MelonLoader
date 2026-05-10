using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Scripts.CosmeticItems;

[CreateAssetMenu(menuName = "FTG/Cosmetic Item/New List", fileName = "New Cosmetic Items List", order = 0)]
public class CosmeticItemsListSo : SerializedScriptableObject
{
	public List<CosmeticItemSo> list = new List<CosmeticItemSo>();
}
