using UnityEngine;

namespace _Scripts.Wardrobe.Accessories;

public class LEDGlasses : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private int ledGlassesMaterialIndex = 1;

	private float time;

	private float speed = 1f;

	private static MaterialPropertyBlock mpb;

	private static readonly int unscaledTimeId = Shader.PropertyToID("_UnscaledTime");

	private static readonly int animationId = Shader.PropertyToID("_Animation");

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void Update()
	{
		time += Time.unscaledDeltaTime * speed;
		meshRenderer.GetPropertyBlock(mpb, ledGlassesMaterialIndex);
		mpb.SetFloat(unscaledTimeId, time);
		meshRenderer.SetPropertyBlock(mpb, ledGlassesMaterialIndex);
	}

	public void SetSpeed(float value)
	{
		speed = value;
	}

	public void SetAnimation(float value)
	{
		if (Application.isPlaying)
		{
			meshRenderer.GetPropertyBlock(mpb, ledGlassesMaterialIndex);
			mpb.SetInteger(animationId, Mathf.RoundToInt(value));
			meshRenderer.SetPropertyBlock(mpb, ledGlassesMaterialIndex);
		}
	}
}
