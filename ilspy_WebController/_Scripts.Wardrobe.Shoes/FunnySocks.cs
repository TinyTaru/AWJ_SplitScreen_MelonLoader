using UnityEngine;

namespace _Scripts.Wardrobe.Shoes;

public class FunnySocks : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	private static MaterialPropertyBlock mpb;

	private static readonly int color1Id = Shader.PropertyToID("_Color1");

	private static readonly int color2Id = Shader.PropertyToID("_Color2");

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void SetGlassColor(int colorId, Color color)
	{
		if (!(meshRenderer == null))
		{
			if (mpb == null)
			{
				mpb = new MaterialPropertyBlock();
			}
			meshRenderer.GetPropertyBlock(mpb);
			mpb.SetColor(colorId, color);
			meshRenderer.SetPropertyBlock(mpb);
		}
	}

	public void SetColor1(Color color)
	{
		SetGlassColor(color1Id, color);
	}

	public void SetColor2(Color color)
	{
		SetGlassColor(color2Id, color);
	}
}
