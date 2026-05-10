using System.Collections;
using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

[DisallowMultipleComponent]
public class PartyHat : MonoBehaviour
{
	[SerializeField]
	private GameObject fluffBall;

	[SerializeField]
	private Mesh shellMesh;

	[SerializeField]
	private Shader shellShader;

	private const int maxShellCount = 256;

	[Range(1f, 256f)]
	[SerializeField]
	private int shellCount = 16;

	[Range(0f, 1f)]
	[SerializeField]
	private float shellLength = 0.15f;

	[Range(0.01f, 3f)]
	[SerializeField]
	private float distanceAttenuation = 1f;

	[Range(1f, 1000f)]
	[SerializeField]
	private float density = 100f;

	[Range(0f, 1f)]
	[SerializeField]
	private float noiseMin;

	[Range(0f, 1f)]
	[SerializeField]
	private float noiseMax = 1f;

	[Range(0f, 10f)]
	[SerializeField]
	private float thickness = 1f;

	[Range(0f, 10f)]
	[SerializeField]
	private float curvature = 1f;

	[Range(0f, 1f)]
	[SerializeField]
	private float displacementStrength = 0.1f;

	[SerializeField]
	private Color shellColor = Color.white;

	[SerializeField]
	private Color shellColorShadow = Color.gray;

	[Range(0f, 5f)]
	[SerializeField]
	private float occlusionAttenuation = 1f;

	[Range(0f, 1f)]
	[SerializeField]
	private float occlusionBias;

	private float burnAmount;

	private Material sharedShellMaterial;

	private GameObject[] shells;

	private MeshRenderer[] shellRenderers;

	private Hat hat;

	private static MaterialPropertyBlock mpb;

	private static readonly int shellCountId = Shader.PropertyToID("_ShellCount");

	private static readonly int shellIndexId = Shader.PropertyToID("_ShellIndex");

	private static readonly int shellLengthId = Shader.PropertyToID("_ShellLength");

	private static readonly int densityId = Shader.PropertyToID("_Density");

	private static readonly int thicknessId = Shader.PropertyToID("_Thickness");

	private static readonly int attenuationId = Shader.PropertyToID("_Attenuation");

	private static readonly int shellDistanceAttenuationId = Shader.PropertyToID("_ShellDistanceAttenuation");

	private static readonly int curvatureId = Shader.PropertyToID("_Curvature");

	private static readonly int displacementStrengthId = Shader.PropertyToID("_DisplacementStrength");

	private static readonly int occlusionBiasId = Shader.PropertyToID("_OcclusionBias");

	private static readonly int noiseMinId = Shader.PropertyToID("_NoiseMin");

	private static readonly int noiseMaxId = Shader.PropertyToID("_NoiseMax");

	private static readonly int shellColorId = Shader.PropertyToID("_ShellColor");

	private static readonly int shellColorShadowId = Shader.PropertyToID("_ShellColorShadow");

	private static readonly int colorId = Shader.PropertyToID("_Color");

	private static readonly int burnAmountId = Shader.PropertyToID("_BurnAmount");

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		hat = GetComponentInParent<Hat>();
		Setup();
	}

	private void OnEnable()
	{
		StartCoroutine(UpdateFluffColorCoroutine());
		UpdateFluffyShader();
		hat.BurnAmountChanged += Hat_OnBurnAmountChanged;
	}

	private void OnDisable()
	{
		hat.BurnAmountChanged -= Hat_OnBurnAmountChanged;
	}

	private void Setup()
	{
		if (shells == null)
		{
			SetupMeshes();
			UpdateFluffyShader();
		}
	}

	private void SetupMeshes()
	{
		if (!(fluffBall == null) && !(shellMesh == null) && !(shellShader == null))
		{
			if (sharedShellMaterial != null)
			{
				Object.DestroyImmediate(sharedShellMaterial);
			}
			sharedShellMaterial = new Material(shellShader);
			shells = new GameObject[shellCount];
			shellRenderers = new MeshRenderer[shellCount];
			for (int i = 0; i < shellCount; i++)
			{
				GameObject gameObject = new GameObject("Shell " + i);
				MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
				MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
				meshFilter.sharedMesh = shellMesh;
				meshRenderer.sharedMaterial = sharedShellMaterial;
				gameObject.transform.SetParent(fluffBall.transform, worldPositionStays: false);
				gameObject.transform.position = fluffBall.transform.position;
				gameObject.transform.rotation = fluffBall.transform.rotation;
				gameObject.transform.localScale = Vector3.one;
				shells[i] = gameObject;
				shellRenderers[i] = meshRenderer;
			}
		}
	}

	private void UpdateFluffyShader()
	{
		if (!Application.isPlaying || shellRenderers == null)
		{
			return;
		}
		for (int i = 0; i < shellRenderers.Length; i++)
		{
			MeshRenderer meshRenderer = shellRenderers[i];
			if (!(meshRenderer == null))
			{
				meshRenderer.GetPropertyBlock(mpb);
				mpb.SetInt(shellCountId, shellCount);
				mpb.SetInt(shellIndexId, i);
				mpb.SetFloat(shellLengthId, shellLength);
				mpb.SetFloat(densityId, density);
				mpb.SetFloat(thicknessId, thickness);
				mpb.SetFloat(attenuationId, occlusionAttenuation);
				mpb.SetFloat(shellDistanceAttenuationId, distanceAttenuation);
				mpb.SetFloat(curvatureId, curvature);
				mpb.SetFloat(displacementStrengthId, displacementStrength);
				mpb.SetFloat(occlusionBiasId, occlusionBias);
				mpb.SetFloat(noiseMinId, noiseMin);
				mpb.SetFloat(noiseMaxId, noiseMax);
				mpb.SetColor(shellColorId, shellColor);
				mpb.SetColor(shellColorShadowId, shellColorShadow);
				mpb.SetFloat(burnAmountId, burnAmount);
				meshRenderer.SetPropertyBlock(mpb);
				shells[i].SetActive(i < shellCount);
			}
		}
	}

	public void UpdateFluffColor(Color color)
	{
		shellColor = color;
		shellColorShadow = color * 0.6f;
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(UpdateFluffColorCoroutine());
		}
	}

	public void SetFluffiness(float value)
	{
		shellLength = value / 4f;
		if (base.gameObject.activeInHierarchy)
		{
			UpdateFluffyShader();
		}
	}

	private void SetBurnAmount(float newBurnAmount)
	{
		burnAmount = newBurnAmount;
		if (base.gameObject.activeInHierarchy)
		{
			UpdateFluffyShader();
		}
	}

	private IEnumerator UpdateFluffColorCoroutine()
	{
		yield return null;
		MeshRenderer component = fluffBall.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.GetPropertyBlock(mpb);
			mpb.SetColor(colorId, shellColor);
			component.SetPropertyBlock(mpb);
		}
		if (shellRenderers == null)
		{
			yield break;
		}
		for (int i = 0; i < shellRenderers.Length; i++)
		{
			MeshRenderer meshRenderer = shellRenderers[i];
			if (!(meshRenderer == null))
			{
				meshRenderer.GetPropertyBlock(mpb);
				mpb.SetColor(shellColorId, shellColor);
				mpb.SetColor(shellColorShadowId, shellColorShadow);
				meshRenderer.SetPropertyBlock(mpb);
			}
		}
	}

	private void Hat_OnBurnAmountChanged(float burnAmount)
	{
		SetBurnAmount(burnAmount);
	}
}
