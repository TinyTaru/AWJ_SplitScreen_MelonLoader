using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class Mohawk : MonoBehaviour
{
	[SerializeField]
	private Transform[] spikes;

	public void EnableSpikes(float value)
	{
		int num = Mathf.RoundToInt(value);
		for (int i = 0; i < spikes.Length; i++)
		{
			spikes[i].gameObject.SetActive(i < num);
		}
	}

	public void SetSize(float value)
	{
		Transform[] array = spikes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].localScale = Vector3.one * value;
		}
	}
}
