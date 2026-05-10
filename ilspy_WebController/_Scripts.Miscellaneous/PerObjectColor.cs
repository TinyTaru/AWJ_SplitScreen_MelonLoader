using UnityEngine;

namespace _Scripts.Miscellaneous;

[DisallowMultipleComponent]
[ExecuteAlways]
public class PerObjectColor : MonoBehaviour
{
	public Color color = Color.white;

	public string colorProperty = "_Color";

	private static MaterialPropertyBlock mpb;

	private static int colorId = -1;

	private Renderer r;

	private void OnEnable()
	{
		Apply();
	}

	public void Apply()
	{
		if (!r)
		{
			r = GetComponent<Renderer>();
		}
		if ((bool)r)
		{
			if (colorId == -1 || Shader.PropertyToID(colorProperty) != colorId)
			{
				colorId = Shader.PropertyToID(colorProperty);
			}
			if (mpb == null)
			{
				mpb = new MaterialPropertyBlock();
			}
			r.GetPropertyBlock(mpb);
			mpb.SetColor(colorId, color);
			r.SetPropertyBlock(mpb);
		}
	}
}
