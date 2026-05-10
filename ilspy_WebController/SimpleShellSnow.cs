using UnityEngine;
using UnityEngine.Rendering;

[SelectionBase]
public class SimpleShellSnow : MonoBehaviour
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

	public Color shellColor = Color.white;

	public Color shellColorShadow = Color.gray;

	private MeshRenderer[] mrs;

	private static MaterialPropertyBlock mpb;

	private static readonly int _ShellCount = Shader.PropertyToID("_ShellCount");

	private static readonly int _ShellIndex = Shader.PropertyToID("_ShellIndex");

	private static readonly int _ShellLength = Shader.PropertyToID("_ShellLength");

	private static readonly int _Density = Shader.PropertyToID("_Density");

	private static readonly int _Thickness = Shader.PropertyToID("_Thickness");

	private static readonly int _Attenuation = Shader.PropertyToID("_Attenuation");

	private static readonly int _DistanceAttenuation = Shader.PropertyToID("_ShellDistanceAttenuation");

	private static readonly int _Curvature = Shader.PropertyToID("_Curvature");

	private static readonly int _DisplacementStrength = Shader.PropertyToID("_DisplacementStrength");

	private static readonly int _OcclusionBias = Shader.PropertyToID("_OcclusionBias");

	private static readonly int _NoiseMin = Shader.PropertyToID("_NoiseMin");

	private static readonly int _NoiseMax = Shader.PropertyToID("_NoiseMax");

	private static readonly int _ColorLight = Shader.PropertyToID("_ShellColor");

	private static readonly int _ColorShadow = Shader.PropertyToID("_ShellColorShadow");

	private void Awake()
	{
		Setup();
		ApplyAll();
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
				mpb.SetFloat(_ShellLength, shellLength * 10f);
				mpb.SetFloat(_Density, density);
				mpb.SetFloat(_Thickness, thickness);
				mpb.SetFloat(_Attenuation, occlusionAttenuation);
				mpb.SetFloat(_DistanceAttenuation, distanceAttenuation);
				mpb.SetFloat(_Curvature, curvature);
				mpb.SetFloat(_DisplacementStrength, displacementStrength);
				mpb.SetFloat(_OcclusionBias, occlusionBias);
				mpb.SetFloat(_NoiseMin, noiseMin);
				mpb.SetFloat(_NoiseMax, noiseMax);
				mpb.SetColor(_ColorLight, shellColor);
				mpb.SetColor(_ColorShadow, shellColorShadow);
				meshRenderer.SetPropertyBlock(mpb);
				mrs[i].gameObject.SetActive(i < shellCount && shellLength > 0f);
			}
		}
	}
}
