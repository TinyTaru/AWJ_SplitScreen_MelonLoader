using UnityEngine;

namespace _Scripts.Wardrobe.Shoes;

public class FireShoes : MonoBehaviour
{
	[SerializeField]
	private Transform origin;

	[SerializeField]
	private ParticleSystem[] particles;

	public void UpdateColor(Color color)
	{
		ParticleSystem[] array = particles;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystem.MainModule main = array[i].main;
			main.startColor = color;
		}
	}

	public void SetSize(float value)
	{
		origin.localScale = Vector3.one * value;
	}
}
