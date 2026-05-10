using UnityEngine;

namespace _Scripts.Wardrobe.Accessories;

public class Monocle : MonoBehaviour
{
	[SerializeField]
	private GameObject leftMonocle;

	[SerializeField]
	private GameObject rightMonocle;

	[Space(10f)]
	[SerializeField]
	private MeshRenderer[] glassMeshRenderers;

	[SerializeField]
	private int[] glassMaterialIndices = new int[1];

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

	public void SetGlassTransparency(float value)
	{
		if (glassMeshRenderers == null)
		{
			return;
		}
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		MeshRenderer[] array = glassMeshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			int[] array2 = glassMaterialIndices;
			foreach (int num in array2)
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

	public void EnableLeftMonocle(float value)
	{
		rightMonocle.SetActive(value > 0.5f);
	}

	public void EnableRightMonocle(float value)
	{
		leftMonocle.SetActive(value > 0.5f);
	}
}
