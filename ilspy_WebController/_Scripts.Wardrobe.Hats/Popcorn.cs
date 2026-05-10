using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class Popcorn : MonoBehaviour
{
	[SerializeField]
	private Transform[] popcornPieces;

	public void EnablePopcornPieces(float value)
	{
		int num = Mathf.RoundToInt(value);
		for (int i = 0; i < popcornPieces.Length; i++)
		{
			popcornPieces[i].gameObject.SetActive(i < num);
		}
	}

	public void SetSize(float value)
	{
		Transform[] array = popcornPieces;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].localScale = Vector3.one * value;
		}
	}
}
