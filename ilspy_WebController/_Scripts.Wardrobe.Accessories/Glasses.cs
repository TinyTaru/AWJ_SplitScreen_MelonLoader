using UnityEngine;

namespace _Scripts.Wardrobe.Accessories;

public class Glasses : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private int[] glassMaterialIndices;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorId = Shader.PropertyToID("_Color");

	private static readonly int transparencyId = Shader.PropertyToID("_Transparency");

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	public void SetGlassColor(Color color)
	{
		if (meshRenderer == null)
		{
			return;
		}
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		int[] array = glassMaterialIndices;
		foreach (int num in array)
		{
			if (num >= 0 && num < meshRenderer.sharedMaterials.Length)
			{
				meshRenderer.GetPropertyBlock(mpb, num);
				mpb.SetColor(colorId, color);
				meshRenderer.SetPropertyBlock(mpb, num);
			}
		}
	}

	public void SetTransparency(float value)
	{
		if (meshRenderer == null)
		{
			return;
		}
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		int[] array = glassMaterialIndices;
		foreach (int num in array)
		{
			if (num >= 0 && num < meshRenderer.sharedMaterials.Length)
			{
				meshRenderer.GetPropertyBlock(mpb, num);
				mpb.SetFloat(transparencyId, value);
				meshRenderer.SetPropertyBlock(mpb, num);
			}
		}
	}
}
