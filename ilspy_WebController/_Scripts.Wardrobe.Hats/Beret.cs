using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class Beret : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer paintbrushMeshRenderer;

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

	private void SetColor(Color color, int index)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		paintbrushMeshRenderer.GetPropertyBlock(mpb, index);
		mpb.SetColor(colorID, color);
		paintbrushMeshRenderer.SetPropertyBlock(mpb, index);
	}

	public void EnablePaintbrush(float value)
	{
		paintbrushMeshRenderer.gameObject.SetActive(value > 0f);
	}

	public void SetColor1(Color color)
	{
		SetColor(color, 0);
	}

	public void SetColor2(Color color)
	{
		SetColor(color, 1);
	}

	public void SetColor3(Color color)
	{
		SetColor(color, 2);
	}

	public void SetColor4(Color color)
	{
		SetColor(color, 3);
	}

	private void Hat_OnBurnAmountChanged(float burnAmount)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		for (int i = 0; i < paintbrushMeshRenderer.materials.Length; i++)
		{
			paintbrushMeshRenderer.GetPropertyBlock(mpb, i);
			mpb.SetFloat(burnAmountId, burnAmount);
			paintbrushMeshRenderer.SetPropertyBlock(mpb, i);
		}
	}
}
