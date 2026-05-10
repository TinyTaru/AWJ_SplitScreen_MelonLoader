using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _Scripts.Interactable;

namespace _Scripts.ShopAndCoins;

public class CollectableCoins : MonoBehaviour
{
	private Dictionary<int, InteractableCoin> coins;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void FindCoinsInLevel()
	{
		coins = new Dictionary<int, InteractableCoin>();
		InteractableCoin[] array = Object.FindObjectsByType<InteractableCoin>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (InteractableCoin interactableCoin in array)
		{
			if (interactableCoin.CoinId < 0)
			{
				Debug.LogError("Negative coin ID found for coin " + interactableCoin.name + "! Click on this message to select coin " + interactableCoin.name + ".", interactableCoin);
				return;
			}
			if (!coins.TryAdd(interactableCoin.CoinId, interactableCoin))
			{
				Debug.LogError($"Can't add coin {interactableCoin.name} to the dictionary because there already exists a coin with the coin ID {interactableCoin.CoinId}! Click on this message to select coin {interactableCoin.name}.", interactableCoin);
			}
		}
		coins = coins.OrderBy((KeyValuePair<int, InteractableCoin> pair) => pair.Key).ToDictionary((KeyValuePair<int, InteractableCoin> pair) => pair.Key, (KeyValuePair<int, InteractableCoin> pair) => pair.Value);
		Debug.Log($"Found {coins.Count} collectible coins in the level. The first coin ID is {coins.First().Key} and the last coin ID is {coins.Last().Key}.");
		if (coins.Count <= coins.Last().Key)
		{
			Debug.Log("<color=#FF0000>Oh no. Some gaps between coin IDs have been found!</color>");
			for (int j = 0; j <= coins.Last().Key; j++)
			{
				if (!coins.ContainsKey(j))
				{
					Debug.Log($"Coin ID {j} wasn't found!");
				}
			}
		}
		else
		{
			Debug.Log("<color=#00FF00>There are no gaps between coin IDs. Everything is fine.</color>");
		}
	}
}
