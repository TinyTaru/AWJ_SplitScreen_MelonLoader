using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using _Scripts.Singletons;

namespace _Scripts.Camera;

public class CameraWaterTrigger : MonoBehaviour
{
	[SerializeField]
	private VolumeProfile[] globalVolumeProfiles;

	private List<GameObject> waterList;

	private void Start()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnRespawnPlayer += GameController_OnOnRespawnPlayer;
		}
		waterList = new List<GameObject>();
		EnableUnderWaterPostProcessing(value: false);
	}

	private void GameController_OnOnRespawnPlayer(object sender, EventArgs e)
	{
		EnableUnderWaterPostProcessing(value: false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Water") && !waterList.Contains(other.gameObject))
		{
			waterList.Add(other.gameObject);
			EnableUnderWaterPostProcessing(value: true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Water"))
		{
			if (waterList.Contains(other.gameObject))
			{
				waterList.Remove(other.gameObject);
			}
			if (waterList.Count == 0)
			{
				EnableUnderWaterPostProcessing(value: false);
			}
		}
	}

	private void OnDestroy()
	{
		EnableUnderWaterPostProcessing(value: false);
	}

	private void EnableUnderWaterPostProcessing(bool value)
	{
		VolumeProfile[] array = globalVolumeProfiles;
		foreach (VolumeProfile obj in array)
		{
			if (obj.TryGet<ColorAdjustments>(out var component))
			{
				component.colorFilter.overrideState = value;
			}
			if (obj.TryGet<PaniniProjection>(out var component2))
			{
				component2.active = value;
			}
		}
	}
}
