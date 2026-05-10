using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class ShroompHat : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer bodyMeshRenderer;

	[SerializeField]
	private MeshRenderer legMeshRenderer;

	private Hat hat;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorID = Shader.PropertyToID("_Color");

	private static readonly int burnAmountId = Shader.PropertyToID("_BurnAmount");

	private void Awake()
	{
		hat = GetComponentInParent<Hat>();
	}

	private void OnEnable()
	{
		hat.BurnAmountChanged += Hat_OnBurnAmountChanged;
	}

	private void OnDisable()
	{
		hat.BurnAmountChanged -= Hat_OnBurnAmountChanged;
	}

	public void SetColor1(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		bodyMeshRenderer.GetPropertyBlock(mpb, 0);
		mpb.SetColor(colorID, color);
		bodyMeshRenderer.SetPropertyBlock(mpb, 0);
	}

	public void SetColor2(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		bodyMeshRenderer.GetPropertyBlock(mpb, 1);
		mpb.SetColor(colorID, color);
		bodyMeshRenderer.SetPropertyBlock(mpb, 1);
		if (legMeshRenderer != null)
		{
			legMeshRenderer.GetPropertyBlock(mpb);
			mpb.SetColor(colorID, color);
			legMeshRenderer.SetPropertyBlock(mpb);
		}
	}

	public void SetEyeBaseColor(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		bodyMeshRenderer.GetPropertyBlock(mpb, 3);
		mpb.SetColor(colorID, color);
		bodyMeshRenderer.SetPropertyBlock(mpb, 3);
	}

	public void SetEyeColor(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		bodyMeshRenderer.GetPropertyBlock(mpb, 2);
		mpb.SetColor(colorID, color);
		bodyMeshRenderer.SetPropertyBlock(mpb, 2);
	}

	public void EnableLegs(float value)
	{
		legMeshRenderer.gameObject.SetActive(value > 0f);
	}

	private void Hat_OnBurnAmountChanged(float burnAmount)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		for (int i = 0; i < bodyMeshRenderer.materials.Length; i++)
		{
			bodyMeshRenderer.GetPropertyBlock(mpb, i);
			mpb.SetFloat(burnAmountId, burnAmount);
			bodyMeshRenderer.SetPropertyBlock(mpb, i);
		}
		if (legMeshRenderer != null)
		{
			legMeshRenderer.GetPropertyBlock(mpb);
			mpb.SetFloat(burnAmountId, burnAmount);
			legMeshRenderer.SetPropertyBlock(mpb);
		}
	}
}
