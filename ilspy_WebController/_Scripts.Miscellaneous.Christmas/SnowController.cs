using System;
using System.Collections;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Miscellaneous.Christmas;

public class SnowController : MonoBehaviour
{
	[SerializeField]
	private RenderTexture renderTexture;

	[SerializeField]
	private UnityEngine.Camera snowCamera;

	[SerializeField]
	private GameObject snowflakeParticles;

	public RenderTexture Texture => renderTexture;

	private void Awake()
	{
		snowCamera.targetTexture = renderTexture;
		Shader.SetGlobalTexture("_GlobalEffectRT", renderTexture);
		Shader.SetGlobalFloat("_OrthographicCamSize", snowCamera.orthographicSize);
		Shader.SetGlobalVector("_Position", snowCamera.transform.position);
	}

	private void OnEnable()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
			if (snowflakeParticles != null)
			{
				snowflakeParticles.SetActive(SettingsController.Snowflakes);
			}
		}
	}

	private void OnDisable()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		}
	}

	private IEnumerator ResetSnowCoroutine()
	{
		snowCamera.backgroundColor = Color.clear;
		snowCamera.clearFlags = CameraClearFlags.Color;
		yield return null;
		snowCamera.clearFlags = CameraClearFlags.Nothing;
	}

	public void Initialize(RenderTexture newRenderTexture)
	{
		Debug.Log("SnowController - Initialize");
		snowCamera.targetTexture = newRenderTexture;
		Shader.SetGlobalTexture("_GlobalEffectRT", newRenderTexture);
	}

	public void ResetSnow()
	{
		StartCoroutine(ResetSnowCoroutine());
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		if (snowflakeParticles != null)
		{
			snowflakeParticles.SetActive(SettingsController.Snowflakes);
		}
	}
}
