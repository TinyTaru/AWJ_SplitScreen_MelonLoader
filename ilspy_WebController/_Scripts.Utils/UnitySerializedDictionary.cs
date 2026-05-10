using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Utils;

public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[SerializeField]
	[HideInInspector]
	private List<TKey> keyData = new List<TKey>();

	[SerializeField]
	[HideInInspector]
	private List<TValue> valueData = new List<TValue>();

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		Clear();
		for (int i = 0; i < keyData.Count && i < valueData.Count; i++)
		{
			base[keyData[i]] = valueData[i];
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		keyData.Clear();
		valueData.Clear();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TKey, TValue> current = enumerator.Current;
			keyData.Add(current.Key);
			valueData.Add(current.Value);
		}
	}
}
