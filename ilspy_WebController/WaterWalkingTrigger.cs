using System;
using UnityEngine;
using _Scripts.Singletons;

public class WaterWalkingTrigger : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem waterWalkingEffect;

	private bool waterWalkingIsActive;

	private void OnEnable()
	{
		SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
	}

	private void OnDisable()
	{
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
	}

	private void Start()
	{
		OnSettingsUpdated();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (waterWalkingIsActive && !(waterWalkingEffect == null) && other.CompareTag("Water"))
		{
			waterWalkingEffect.Play();
		}
	}

	private void OnSettingsUpdated()
	{
		waterWalkingIsActive = SettingsController.WaterWalking;
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		OnSettingsUpdated();
	}
}
