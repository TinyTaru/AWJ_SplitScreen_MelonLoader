using System;
using UnityEngine;
using _Scripts.Miscellaneous.Halloween;
using _Scripts.Singletons;

namespace _Scripts.Objects;

[DisallowMultipleComponent]
public class LightRay : MonoBehaviour
{
	[SerializeField]
	private Material lightRayMaterialHalloween;

	private MeshRenderer meshRenderer;

	private void Awake()
	{
		meshRenderer = GetComponent<MeshRenderer>();
	}

	private void Start()
	{
		if (Singleton<KitchenHalloweenController>.Instance != null)
		{
			Singleton<KitchenHalloweenController>.Instance.OnActivateHalloweenEffects += HalloweenController_OnActivateHalloweenEffects;
		}
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated += SettingsControllerOnOnSettingsUpdated;
		}
		UpdateMeshRenderer();
	}

	private void OnDestroy()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated -= SettingsControllerOnOnSettingsUpdated;
		}
	}

	private void UpdateMeshRenderer()
	{
		if (!(meshRenderer == null) && !(Singleton<SettingsController>.Instance == null))
		{
			meshRenderer.enabled = SettingsController.QualityIndex != 0;
		}
	}

	private void HalloweenController_OnActivateHalloweenEffects(object sender, EventArgs e)
	{
		if (!(meshRenderer == null))
		{
			meshRenderer.sharedMaterial = lightRayMaterialHalloween;
		}
	}

	private void SettingsControllerOnOnSettingsUpdated(object sender, EventArgs e)
	{
		UpdateMeshRenderer();
	}
}
