using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class BalloonHat : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer bowMeshRenderer;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorID = Shader.PropertyToID("_Color");

	public void SetColor2(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		bowMeshRenderer.GetPropertyBlock(mpb);
		mpb.SetColor(colorID, color);
		bowMeshRenderer.SetPropertyBlock(mpb);
	}
}
