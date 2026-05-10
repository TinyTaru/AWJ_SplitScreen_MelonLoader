using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class PuzzleCube : MonoBehaviour
{
	[SerializeField]
	private GameObject[] patterns;

	public void SetPattern(float value)
	{
		for (int i = 0; i < patterns.Length; i++)
		{
			int num = Mathf.RoundToInt(value) - 1;
			patterns[i].SetActive(i == num);
		}
	}
}
