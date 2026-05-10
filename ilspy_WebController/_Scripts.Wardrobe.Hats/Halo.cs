using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class Halo : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem particles;

	[SerializeField]
	private Transform origin;

	public void SetParticleColor(Color color)
	{
		ParticleSystem.MainModule main = particles.main;
		main.startColor = color;
	}

	public void EnableParticles(float value)
	{
		particles.gameObject.SetActive(value > 0f);
	}
}
