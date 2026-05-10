using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class Horns : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer[] hornAlphaEffects;

	[SerializeField]
	private ParticleSystem[] particleSystems;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorID = Shader.PropertyToID("_Color");

	public void EnableParticles(float value)
	{
		ParticleSystem[] array = particleSystems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value > 0f);
		}
		MeshRenderer[] array2 = hornAlphaEffects;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].gameObject.SetActive(value > 0f);
		}
	}

	public void UpdateAlphaEffectColor(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		MeshRenderer[] array = hornAlphaEffects;
		foreach (MeshRenderer obj in array)
		{
			obj.GetPropertyBlock(mpb);
			mpb.SetColor(colorID, color);
			obj.SetPropertyBlock(mpb);
		}
		ParticleSystem[] array2 = particleSystems;
		for (int i = 0; i < array2.Length; i++)
		{
			ParticleSystem.MainModule main = array2[i].main;
			main.startColor = color;
		}
	}
}
