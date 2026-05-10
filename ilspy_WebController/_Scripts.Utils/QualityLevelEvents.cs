using System;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;

namespace _Scripts.Utils;

public class QualityLevelEvents : MonoBehaviour
{
	[SerializeField]
	private UnityEvent onHighQualityEvent;

	[SerializeField]
	private UnityEvent onMediumQualityEvent;

	[SerializeField]
	private UnityEvent onLowQualityEvent;

	[SerializeField]
	private UnityEvent onPotatoQualityEvent;

	private void Start()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated += SettingsControllerOnOnSettingsUpdated;
		}
	}

	private void OnEnable()
	{
		InvokeQualityEvents();
	}

	private void OnDestroy()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated -= SettingsControllerOnOnSettingsUpdated;
		}
	}

	private void InvokeQualityEvents()
	{
		switch (SettingsController.QualityIndex)
		{
		case 0:
			onPotatoQualityEvent?.Invoke();
			break;
		case 1:
			onLowQualityEvent?.Invoke();
			break;
		case 2:
			onMediumQualityEvent?.Invoke();
			break;
		case 3:
			onHighQualityEvent?.Invoke();
			break;
		}
	}

	private void SettingsControllerOnOnSettingsUpdated(object sender, EventArgs e)
	{
		InvokeQualityEvents();
	}
}
