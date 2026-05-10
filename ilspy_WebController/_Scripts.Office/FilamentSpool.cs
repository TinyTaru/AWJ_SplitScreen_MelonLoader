using UnityEngine;

namespace _Scripts.Office;

[ExecuteAlways]
public class FilamentSpool : MonoBehaviour
{
	[SerializeField]
	private Color filamentColor;

	[SerializeField]
	private MeshRenderer filamentMeshRenderer;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorId = Shader.PropertyToID("_Color");

	public Color FilamentColor => filamentColor;

	private void OnEnable()
	{
		UpdateFilamentColor();
	}

	private void UpdateFilamentColor()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		filamentMeshRenderer.GetPropertyBlock(mpb, 0);
		mpb.SetColor(colorId, filamentColor);
		filamentMeshRenderer.SetPropertyBlock(mpb, 0);
	}
}
