using UnityEngine;

[DisallowMultipleComponent]
[ExecuteAlways]
public class PerSubmeshColors : MonoBehaviour
{
	[Tooltip("Color property name in your shader (_BaseColor for URP Lit, _Color for many others).")]
	public string colorProperty = "_Color";

	[Tooltip("One color per material slot on this renderer.")]
	public Color[] colors = new Color[0];

	private static MaterialPropertyBlock mpb;

	private static int colorId = -1;

	private Renderer r;

	private void Reset()
	{
		r = GetComponent<Renderer>();
		SyncArraySize();
	}

	private void OnEnable()
	{
		Apply();
	}

	private void SyncArraySize()
	{
		if (!r)
		{
			r = GetComponent<Renderer>();
		}
		if (!r)
		{
			return;
		}
		int num = ((r.sharedMaterials != null) ? r.sharedMaterials.Length : 0);
		if (num > 0 && (colors == null || colors.Length != num))
		{
			Color[] array = colors;
			colors = new Color[num];
			for (int i = 0; i < num; i++)
			{
				colors[i] = ((array != null && i < array.Length) ? array[i] : Color.white);
			}
		}
	}

	public void Apply()
	{
		if (!r)
		{
			r = GetComponent<Renderer>();
		}
		if ((bool)r)
		{
			SyncArraySize();
			int num = Shader.PropertyToID(colorProperty);
			if (num != colorId)
			{
				colorId = num;
			}
			if (mpb == null)
			{
				mpb = new MaterialPropertyBlock();
			}
			Material[] sharedMaterials = r.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				r.GetPropertyBlock(mpb, i);
				Color value = ((colors != null && i < colors.Length) ? colors[i] : Color.white);
				mpb.SetColor(colorId, value);
				r.SetPropertyBlock(mpb, i);
			}
		}
	}
}
