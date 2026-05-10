using System;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Objects;

public class Pan : MonoBehaviour
{
	[SerializeField]
	private CookingArea cookingArea;

	[SerializeField]
	private MeshRenderer[] meshRenderers;

	[FormerlySerializedAs("heatingMaterialIndizes")]
	[SerializeField]
	private int[] heatingMaterialIndices;

	[SerializeField]
	private float heatingSpeed = 0.4f;

	[SerializeField]
	private string temperatureProperty = "_Temperature";

	[SerializeField]
	private StudioEventEmitter cookingSound;

	private List<CookingArea> connectedCookingAreas = new List<CookingArea>();

	private float temperature;

	private static MaterialPropertyBlock mpb;

	private static int temperatureId;

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		temperatureId = Shader.PropertyToID(temperatureProperty);
		temperature = 0f;
	}

	private void Start()
	{
		StopCooking();
		ApplyTemperature();
	}

	private void Update()
	{
		if (!(cookingArea == null))
		{
			if (cookingArea.IsCooking)
			{
				temperature += heatingSpeed * Time.deltaTime;
			}
			else
			{
				temperature -= heatingSpeed * Time.deltaTime;
			}
			temperature = Mathf.Clamp01(temperature);
			if (temperature > 0f)
			{
				ApplyTemperature();
			}
		}
	}

	private void ApplyTemperature()
	{
		if (meshRenderers == null)
		{
			return;
		}
		for (int i = 0; i < meshRenderers.Length; i++)
		{
			MeshRenderer meshRenderer = meshRenderers[i];
			if (!(meshRenderer == null))
			{
				meshRenderer.GetPropertyBlock(mpb, heatingMaterialIndices[i]);
				mpb.SetFloat(temperatureId, temperature);
				meshRenderer.SetPropertyBlock(mpb, heatingMaterialIndices[i]);
			}
		}
	}

	private void CheckConnectedCookingAreas()
	{
		bool flag = false;
		foreach (CookingArea connectedCookingArea in connectedCookingAreas)
		{
			if (connectedCookingArea.IsCooking)
			{
				flag = true;
				break;
			}
		}
		if (!(cookingArea == null))
		{
			if (flag)
			{
				StartCooking();
			}
			else
			{
				StopCooking();
			}
		}
	}

	private void StartCooking()
	{
		if (cookingArea != null)
		{
			cookingArea.StartCooking();
		}
		if (cookingSound != null && !cookingSound.IsPlaying())
		{
			cookingSound.Play();
		}
	}

	private void StopCooking()
	{
		if (cookingArea != null)
		{
			cookingArea.StopCooking();
		}
		if (cookingSound != null)
		{
			cookingSound.Stop();
		}
	}

	public void RegisterConnectedCookingArea(CookingArea connectedCookingArea)
	{
		if (!(connectedCookingArea == null))
		{
			if (!connectedCookingAreas.Contains(connectedCookingArea))
			{
				connectedCookingAreas.Add(connectedCookingArea);
				connectedCookingArea.OnStartCooking += ConnectedCookingArea_OnStartCooking;
				connectedCookingArea.OnStopCooking += ConnectedCookingArea_OnStopCooking;
			}
			CheckConnectedCookingAreas();
		}
	}

	public void UnregisterConnectedCookingArea(CookingArea connectedCookingArea)
	{
		if (!(connectedCookingArea == null))
		{
			if (connectedCookingAreas.Contains(connectedCookingArea))
			{
				connectedCookingAreas.Remove(connectedCookingArea);
				connectedCookingArea.OnStartCooking -= ConnectedCookingArea_OnStartCooking;
				connectedCookingArea.OnStopCooking -= ConnectedCookingArea_OnStopCooking;
			}
			CheckConnectedCookingAreas();
		}
	}

	private void ConnectedCookingArea_OnStartCooking(object sender, EventArgs e)
	{
		CheckConnectedCookingAreas();
	}

	private void ConnectedCookingArea_OnStopCooking(object sender, EventArgs e)
	{
		CheckConnectedCookingAreas();
	}
}
