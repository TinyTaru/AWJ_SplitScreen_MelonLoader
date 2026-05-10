using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class FireHat : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem[] particles;

	[SerializeField]
	private GameObject smokeParticles;

	public void UpdateColor(Color color)
	{
		ParticleSystem[] array = particles;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystem.MainModule main = array[i].main;
			main.startColor = color;
		}
	}

	public void EnableSmoke(float value)
	{
		smokeParticles.SetActive(value > 0f);
	}
}
