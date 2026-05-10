using UnityEngine;
using UnityEngine.Rendering;
using _Scripts.Spider;

[SelectionBase]
public class SimpleShellArachnophobia : MonoBehaviour
{
	public Mesh shellMesh;

	public Material shellMaterial;

	private const int maxShellCount = 256;

	[Range(1f, 256f)]
	public int shellCount = 16;

	[Range(0f, 1f)]
	public float shellLength = 0.15f;

	[Range(0.01f, 3f)]
	public float distanceAttenuation = 1f;

	[Range(1f, 1000f)]
	public float density = 100f;

	[Range(0f, 1f)]
	public float noiseMin;

	[Range(0f, 1f)]
	public float noiseMax = 1f;

	[Range(0f, 10f)]
	public float thickness = 1f;

	[Range(0f, 10f)]
	public float curvature = 1f;

	[Range(0f, 1f)]
	public float displacementStrength = 0.1f;

	[Range(0f, 5f)]
	public float occlusionAttenuation = 1f;

	[Range(0f, 1f)]
	public float occlusionBias;

	private float burnAmount;

	public Color bodyColorLight;

	public Color bodyColorShadow;

	public Color legColorLight;

	public Color legColorShadow;

	public Color jointColorLight;

	public Color jointColorShadow;

	private MeshRenderer[] mrs;

	private static MaterialPropertyBlock mpb;

	private static readonly int _ShellCount = Shader.PropertyToID("_ShellCount");

	private static readonly int _ShellIndex = Shader.PropertyToID("_ShellIndex");

	private static readonly int _ShellLength = Shader.PropertyToID("_ShellLength");

	private static readonly int _Density = Shader.PropertyToID("_Density");

	private static readonly int _Thickness = Shader.PropertyToID("_Thickness");

	private static readonly int _Attenuation = Shader.PropertyToID("_Attenuation");

	private static readonly int _DistanceAttenenuation = Shader.PropertyToID("_ShellDistanceAttenuation");

	private static readonly int _Curvature = Shader.PropertyToID("_Curvature");

	private static readonly int _DisplacementStrength = Shader.PropertyToID("_DisplacementStrength");

	private static readonly int _OcclusionBias = Shader.PropertyToID("_OcclusionBias");

	private static readonly int _NoiseMin = Shader.PropertyToID("_NoiseMin");

	private static readonly int _NoiseMax = Shader.PropertyToID("_NoiseMax");

	private static readonly int _BodyColorLight = Shader.PropertyToID("_BodyColorLight");

	private static readonly int _BodyColorShadow = Shader.PropertyToID("_BodyColorShadow");

	private static readonly int _LegColorLight = Shader.PropertyToID("_LegColorLight");

	private static readonly int _LegColorShadow = Shader.PropertyToID("_LegColorShadow");

	private static readonly int _JointColorLight = Shader.PropertyToID("_JointColorLight");

	private static readonly int _JointColorShadow = Shader.PropertyToID("_JointColorShadow");

	private static readonly int _BurnAmount = Shader.PropertyToID("_BurnAmount");

	private void Awake()
	{
		BodyMovement componentInParent = GetComponentInParent<BodyMovement>();
		if (!componentInParent || componentInParent.IsPlayer)
		{
			Setup();
			ApplyAll();
		}
	}

	private void Setup()
	{
		if (mrs == null)
		{
			mrs = new MeshRenderer[shellCount];
			for (int i = 0; i < shellCount; i++)
			{
				GameObject obj = new GameObject($"Shell {i}");
				obj.transform.SetParent(base.transform, worldPositionStays: false);
				obj.AddComponent<MeshFilter>().sharedMesh = shellMesh;
				MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
				meshRenderer.sharedMaterial = shellMaterial;
				meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
				meshRenderer.receiveShadows = false;
				mrs[i] = meshRenderer;
			}
		}
	}

	private void ApplyAll()
	{
		if (mrs == null)
		{
			return;
		}
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		for (int i = 0; i < mrs.Length; i++)
		{
			MeshRenderer meshRenderer = mrs[i];
			if ((bool)meshRenderer)
			{
				meshRenderer.GetPropertyBlock(mpb);
				mpb.SetInt(_ShellCount, shellCount);
				mpb.SetInt(_ShellIndex, i);
				mpb.SetFloat(_ShellLength, shellLength);
				mpb.SetFloat(_Density, density);
				mpb.SetFloat(_Thickness, thickness);
				mpb.SetFloat(_Attenuation, occlusionAttenuation);
				mpb.SetFloat(_DistanceAttenenuation, distanceAttenuation);
				mpb.SetFloat(_Curvature, curvature);
				mpb.SetFloat(_DisplacementStrength, displacementStrength);
				mpb.SetFloat(_OcclusionBias, occlusionBias);
				mpb.SetFloat(_NoiseMin, noiseMin);
				mpb.SetFloat(_NoiseMax, noiseMax);
				mpb.SetColor(_BodyColorLight, bodyColorLight);
				mpb.SetColor(_BodyColorShadow, bodyColorShadow);
				mpb.SetColor(_LegColorLight, legColorLight);
				mpb.SetColor(_LegColorShadow, legColorShadow);
				mpb.SetColor(_JointColorLight, jointColorLight);
				mpb.SetColor(_JointColorShadow, jointColorShadow);
				mpb.SetFloat(_BurnAmount, burnAmount);
				meshRenderer.SetPropertyBlock(mpb);
				meshRenderer.gameObject.SetActive(i < shellCount && shellLength > 0f);
			}
		}
	}

	public void UpdateBodyColors(Color newBodyColorLight, Color newBodyColorShadow)
	{
		Setup();
		bodyColorLight = newBodyColorLight;
		bodyColorShadow = newBodyColorShadow;
		ApplyAll();
	}

	public void UpdateLegColors(Color newLegColorLight, Color newLegColorShadow)
	{
		Setup();
		legColorLight = newLegColorLight;
		legColorShadow = newLegColorShadow;
		ApplyAll();
	}

	public void UpdateJointColors(Color newJointColorLight, Color newJointColorShadow)
	{
		Setup();
		jointColorLight = newJointColorLight;
		jointColorShadow = newJointColorShadow;
		ApplyAll();
	}

	public void SetLength(float newLength)
	{
		Setup();
		shellLength = newLength;
		ApplyAll();
	}

	public void SetBurnAmount(float newBurnAmount)
	{
		Setup();
		burnAmount = newBurnAmount;
		ApplyAll();
	}
}
