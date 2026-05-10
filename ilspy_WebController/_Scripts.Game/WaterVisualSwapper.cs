using System;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Game;

public class WaterVisualSwapper : MonoBehaviour
{
	[Header("Water")]
	[SerializeField]
	private MeshRenderer[] waterMeshRenderers;

	[SerializeField]
	private Material waterMaterialDefault;

	[SerializeField]
	private Material waterMaterialLow;

	[SerializeField]
	private Material waterMaterialLowest;

	[Header("Waterfall")]
	[SerializeField]
	private MeshRenderer[] waterfallMeshRenderers;

	[SerializeField]
	private Material waterfallMaterialDefault;

	[SerializeField]
	private Material waterfallMaterialLow;

	[SerializeField]
	private Material waterfallMaterialLowest;

	private void OnEnable()
	{
		UpdateWaterShader();
		SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
	}

	private void OnDisable()
	{
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
	}

	private void UpdateWaterShader()
	{
		int qualityIndex = SettingsController.QualityIndex;
		MeshRenderer[] array = waterMeshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			meshRenderer.sharedMaterial = qualityIndex switch
			{
				0 => waterMaterialLowest, 
				1 => waterMaterialLow, 
				_ => waterMaterialDefault, 
			};
		}
		array = waterfallMeshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			meshRenderer.sharedMaterial = qualityIndex switch
			{
				0 => waterfallMaterialLowest, 
				1 => waterfallMaterialLow, 
				_ => waterfallMaterialDefault, 
			};
		}
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		UpdateWaterShader();
	}
}
