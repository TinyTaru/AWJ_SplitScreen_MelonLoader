using FMODUnity;
using UnityEngine;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.Utils;

namespace _Scripts.Interactable;

public class CollectibleVisuals : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private MeshRenderer[] meshRenderers;

	[SerializeField]
	private Color[] colors;

	[SerializeField]
	private ParticleSystem coinCollectionEffectPrefab;

	[Header("Sounds")]
	[SerializeField]
	private EventReference collectSound;

	private Color color;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorId = Shader.PropertyToID("_Color");

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void Start()
	{
		if (colors.Length == 0 || meshRenderers.Length == 0)
		{
			return;
		}
		color = colors.RandomValue();
		MeshRenderer[] array = meshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (SettingsController.CollectibleStyle == CollectibleStyleType.Flower)
			{
				for (int j = 0; j < 2; j++)
				{
					meshRenderer.GetPropertyBlock(mpb, j);
					mpb.SetColor(colorId, color);
					meshRenderer.SetPropertyBlock(mpb, j);
				}
			}
			else
			{
				meshRenderer.GetPropertyBlock(mpb);
				mpb.SetColor(colorId, color);
				meshRenderer.SetPropertyBlock(mpb);
			}
		}
	}

	public void OnInteract()
	{
		if (coinCollectionEffectPrefab != null)
		{
			ParticleSystem particleSystem = Object.Instantiate(coinCollectionEffectPrefab, base.transform.position, Quaternion.identity, null);
			if (colors.Length != 0)
			{
				ParticleSystem.MainModule main = particleSystem.main;
				float num = 0.8f;
				main.startColor = new ParticleSystem.MinMaxGradient(color, color * num);
			}
		}
		if (!collectSound.IsNull)
		{
			Singleton<MusicController>.Instance.PlaySound(collectSound);
		}
	}
}
