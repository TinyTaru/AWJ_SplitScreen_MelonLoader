using System;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Environment;

public class TerrainController : MonoBehaviour
{
	private Terrain terrain;

	private void Awake()
	{
		terrain = GetComponent<Terrain>();
	}

	private void OnEnable()
	{
		UpdateTerrainSettings();
		SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
	}

	private void OnDisable()
	{
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
	}

	private void UpdateTerrainSettings()
	{
		if (!(terrain == null))
		{
			terrain.heightmapPixelError = ((SettingsController.QualityIndex == 2) ? 1f : 50f);
		}
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs eventArgs)
	{
		UpdateTerrainSettings();
	}
}
