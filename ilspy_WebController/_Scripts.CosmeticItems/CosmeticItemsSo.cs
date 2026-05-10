using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace _Scripts.CosmeticItems;

public class CosmeticItemsSo : SerializedScriptableObject
{
	public Dictionary<int, CosmeticItemSo> dictionary = new Dictionary<int, CosmeticItemSo>();

	private void AddDictionaryEntry()
	{
		int num = dictionary.Keys.Prepend(-1).Max();
		dictionary.Add(num + 1, null);
	}
}
