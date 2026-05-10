using System;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Environment;

public class LODController : MonoBehaviour
{
	private LODGroup lodGroup;

	private void Awake()
	{
		lodGroup = GetComponent<LODGroup>();
	}

	private void OnEnable()
	{
		UpdateLODSettings();
		SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
	}

	private void OnDisable()
	{
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
	}

	private void UpdateLODSettings()
	{
		_ = lodGroup == null;
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs eventArgs)
	{
		UpdateLODSettings();
	}
}
