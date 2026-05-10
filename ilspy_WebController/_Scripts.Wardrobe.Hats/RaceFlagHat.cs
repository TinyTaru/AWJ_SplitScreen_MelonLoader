using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class RaceFlagHat : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private int materialIndex;

	private Hat hat;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorWhiteId = Shader.PropertyToID("_ColorWhite");

	private static readonly int colorBlackId = Shader.PropertyToID("_ColorBlack");

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

	public void SetWhiteColor(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		meshRenderer.GetPropertyBlock(mpb, materialIndex);
		mpb.SetColor(colorWhiteId, color);
		meshRenderer.SetPropertyBlock(mpb, materialIndex);
	}

	public void SetBlackColor(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		meshRenderer.GetPropertyBlock(mpb, materialIndex);
		mpb.SetColor(colorBlackId, color);
		meshRenderer.SetPropertyBlock(mpb, materialIndex);
	}

	private void Hat_OnBurnAmountChanged(float burnAmount)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		meshRenderer.GetPropertyBlock(mpb, materialIndex);
		mpb.SetFloat(burnAmountId, burnAmount);
		meshRenderer.SetPropertyBlock(mpb, materialIndex);
	}
}
