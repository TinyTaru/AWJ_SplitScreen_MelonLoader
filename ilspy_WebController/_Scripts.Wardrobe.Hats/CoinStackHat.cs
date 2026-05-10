using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class CoinStackHat : MonoBehaviour
{
	[SerializeField]
	private GameObject[] coins;

	public void SetCoinAmount(float value)
	{
		int num = Mathf.RoundToInt(value);
		for (int i = 0; i < coins.Length; i++)
		{
			coins[i].SetActive(i < num);
		}
	}
}
