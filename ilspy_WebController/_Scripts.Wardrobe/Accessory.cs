using System;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Wardrobe;

public class Accessory : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private GameObject normalAccessory;

	[SerializeField]
	private GameObject arachnophobiaAccessory;

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

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void Start()
	{
		ActivateCorrectAccessory();
	}

	private void OnEnable()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		}
	}

	private void OnDisable()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		}
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		ActivateCorrectAccessory();
	}

	private void ActivateCorrectAccessory()
	{
		bool arachnophobiaMode = SettingsController.ArachnophobiaMode;
		if (normalAccessory != null)
		{
			normalAccessory.SetActive(!arachnophobiaMode);
		}
		if (arachnophobiaAccessory != null)
		{
			arachnophobiaAccessory.SetActive(arachnophobiaMode);
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

	public void SetAccessoryColor(int index, Color color)
	{
		if (meshRenderers == null || NumberOfColors == 0 || index >= NumberOfColors)
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

	public void SetAccessoryEffect(int index, float newValue)
	{
		if (NumberOfEffects != 0 && index < NumberOfEffects)
		{
			effects[index].SetValue(newValue);
		}
	}

	public void SetAccessoryBurnAmount(float burnAmount)
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
}
