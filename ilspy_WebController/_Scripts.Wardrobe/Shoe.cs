using UnityEngine;

namespace _Scripts.Wardrobe;

public class Shoe : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer spiderLeg;

	[SerializeField]
	private MeshRenderer spiderLegFluffy;

	[Space(10f)]
	[SerializeField]
	private bool disableDefaultTip;

	[SerializeField]
	private bool flipRightShoe;

	[Space(10f)]
	[SerializeField]
	private MeshRenderer[] meshRenderers;

	[Space(10f)]
	[SerializeField]
	private CosmeticItemColor[] colors;

	[Space(10f)]
	[SerializeField]
	private CosmeticItemEffect[] effects;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorId = Shader.PropertyToID("_Color");

	private static readonly int burnAmountId = Shader.PropertyToID("_BurnAmount");

	public int NumberOfColors => colors.Length;

	public int NumberOfEffects => effects.Length;

	public bool DisableDefaultTip => disableDefaultTip;

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void Start()
	{
		if (spiderLegFluffy != null)
		{
			spiderLegFluffy.enabled = false;
		}
	}

	public Color GetDefaultColor(int index)
	{
		if (index < NumberOfColors)
		{
			return colors[index].DefaultColor;
		}
		return Color.black;
	}

	public string GetColorName(int index)
	{
		if (index < NumberOfColors)
		{
			return colors[index].Name;
		}
		return "";
	}

	public void SetShoeColor(int index, Color color)
	{
		if (meshRenderers == null || NumberOfColors == 0 || NumberOfColors < index)
		{
			return;
		}
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		colors[index].SetColor(color);
		MeshRenderer[] array = meshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (meshRenderer == null || colors[index].ColorMaterialIndices == null || colors[index].ColorMaterialIndices.Length == 0)
			{
				continue;
			}
			int[] colorMaterialIndices = colors[index].ColorMaterialIndices;
			foreach (int num in colorMaterialIndices)
			{
				if (num >= 0 && num < meshRenderer.sharedMaterials.Length)
				{
					meshRenderer.GetPropertyBlock(mpb, num);
					mpb.SetColor(colorId, color);
					meshRenderer.SetPropertyBlock(mpb, num);
				}
			}
		}
	}

	public CosmeticItemEffect GetEffect(int index)
	{
		if (index < NumberOfEffects)
		{
			return effects[index];
		}
		return null;
	}

	public float GetDefaultEffect(int index)
	{
		if (index < NumberOfEffects)
		{
			return effects[index].DefaultValue;
		}
		return 0f;
	}

	public void SetShoeEffect(int index, float newValue)
	{
		if (NumberOfEffects != 0 && index < NumberOfEffects)
		{
			effects[index].SetValue(newValue);
		}
	}

	public void SetLegFluffiness(float legFluffiness, float legShellLengthFactor)
	{
		if (!(spiderLegFluffy == null))
		{
			SimpleShell component = spiderLegFluffy.GetComponent<SimpleShell>();
			if (component != null)
			{
				component.SetLength(legFluffiness * legShellLengthFactor * 0.5f);
			}
		}
	}

	public void SetLegColor(Color color)
	{
		if (!(spiderLeg == null))
		{
			if (mpb == null)
			{
				mpb = new MaterialPropertyBlock();
			}
			spiderLeg.GetPropertyBlock(mpb);
			mpb.SetColor(colorId, color);
			spiderLeg.SetPropertyBlock(mpb);
		}
	}

	public void SetLegColorFluffy(Color shellColor, Color shellColorShadow)
	{
		if (!(spiderLegFluffy == null))
		{
			SimpleShell component = spiderLegFluffy.GetComponent<SimpleShell>();
			if (component != null)
			{
				component.UpdateColors(shellColor, shellColorShadow);
			}
		}
	}

	public void SetShoeBurnAmount(float burnAmount)
	{
		if (meshRenderers == null)
		{
			return;
		}
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		CosmeticItemColor[] array = colors;
		foreach (CosmeticItemColor cosmeticItemColor in array)
		{
			if (cosmeticItemColor.ColorMaterialIndices == null || cosmeticItemColor.ColorMaterialIndices.Length == 0)
			{
				continue;
			}
			MeshRenderer[] array2 = meshRenderers;
			foreach (MeshRenderer meshRenderer in array2)
			{
				if (meshRenderer == null)
				{
					continue;
				}
				int[] colorMaterialIndices = cosmeticItemColor.ColorMaterialIndices;
				foreach (int num in colorMaterialIndices)
				{
					if (num >= 0 && num < meshRenderer.sharedMaterials.Length)
					{
						meshRenderer.GetPropertyBlock(mpb, num);
						mpb.SetFloat(burnAmountId, burnAmount);
						meshRenderer.SetPropertyBlock(mpb, num);
					}
				}
			}
		}
	}

	public void SetLegBurnAmount(float burnAmount)
	{
		if (!(spiderLeg == null))
		{
			if (mpb == null)
			{
				mpb = new MaterialPropertyBlock();
			}
			spiderLeg.GetPropertyBlock(mpb);
			mpb.SetFloat(burnAmountId, burnAmount);
			spiderLeg.SetPropertyBlock(mpb);
			SimpleShell component = spiderLegFluffy.GetComponent<SimpleShell>();
			if (component != null)
			{
				component.SetBurnAmount(burnAmount);
			}
		}
	}

	public void Flip()
	{
		if (flipRightShoe)
		{
			Vector3 localScale = base.transform.localScale;
			localScale.x *= -1f;
			base.transform.localScale = localScale;
		}
	}
}
