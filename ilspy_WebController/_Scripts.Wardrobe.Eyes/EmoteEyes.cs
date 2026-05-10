using UnityEngine;

namespace _Scripts.Wardrobe.Eyes;

public class EmoteEyes : MonoBehaviour
{
	[SerializeField]
	private GameObject[] happyEyes;

	[SerializeField]
	private GameObject[] angryEyes;

	[SerializeField]
	private GameObject[] closedDownEyes;

	[SerializeField]
	private GameObject[] closedUpEyes;

	public void SetEyeStyle(float value)
	{
		GameObject[] array = happyEyes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		array = angryEyes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		array = closedDownEyes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		array = closedUpEyes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		switch (Mathf.RoundToInt(value))
		{
		case 0:
		{
			array = happyEyes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			break;
		}
		case 1:
		{
			array = angryEyes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			break;
		}
		case 2:
		{
			array = closedDownEyes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			break;
		}
		case 3:
		{
			array = closedUpEyes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			break;
		}
		}
	}
}
