using System;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Effects;

internal class AirFuzzParticles : MonoBehaviour
{
	private ParticleSystem particles;

	private void Awake()
	{
		particles = GetComponent<ParticleSystem>();
	}

	private void Start()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated += SettingsControllerOnOnSettingsUpdated;
		}
	}

	private void OnDestroy()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated -= SettingsControllerOnOnSettingsUpdated;
		}
	}

	private void SettingsControllerOnOnSettingsUpdated(object sender, EventArgs e)
	{
		if (!(particles == null))
		{
			if (SettingsController.QualityIndex == 0)
			{
				particles.Stop();
			}
			else
			{
				particles.Play();
			}
		}
	}
}
