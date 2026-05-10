using UnityEngine;

namespace _Scripts.Wardrobe.Accessories;

public class MovieGlasses3D : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private int leftGlassMaterialIndex = 1;

	[SerializeField]
	private int rightGlassMaterialIndex = 2;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorId = Shader.PropertyToID("_Color");

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void SetGlassColor(int materialIndex, Color color)
	{
		if (!(meshRenderer == null))
		{
			meshRenderer.GetPropertyBlock(mpb, materialIndex);
			mpb.SetColor(colorId, color);
			meshRenderer.SetPropertyBlock(mpb, materialIndex);
		}
	}

	public void SetLeftGlassColor(Color color)
	{
		SetGlassColor(leftGlassMaterialIndex, color);
	}

	public void SetRightGlassColor(Color color)
	{
		SetGlassColor(rightGlassMaterialIndex, color);
	}
}
