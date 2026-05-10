using System.Collections;
using UnityEngine;

namespace _Scripts.Office;

[SelectionBase]
public class Carpet : MonoBehaviour
{
	[SerializeField]
	private bool debug;

	[Header("References")]
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private Transform shellContainer;

	[SerializeField]
	private Rigidbody thicknessHandle;

	[SerializeField]
	private Material shellMaterialPrefab;

	[Range(0f, 1f)]
	[SerializeField]
	private float hueMultiplier = 1f;

	[Range(0f, 1f)]
	[SerializeField]
	private float valueMultiplier = 1f;

	[Range(0f, 1f)]
	[SerializeField]
	private float saturationMultiplier = 1f;

	[SerializeField]
	private Color colorMultiplier = Color.white;

	[Header("Noise")]
	[SerializeField]
	private bool noise;

	[Range(0f, 30f)]
	[SerializeField]
	private float noiseSize = 10f;

	[Range(0f, 1f)]
	[SerializeField]
	private float noiseStrength = 0.5f;

	[Header("Parameters")]
	[SerializeField]
	private Vector2 scale = new Vector2(1f, 1f);

	private const int maxShellCount = 256;

	[Range(1f, 256f)]
	[SerializeField]
	private int shellCount = 16;

	[Range(0f, 4.8f)]
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

	[Range(0f, 5f)]
	[SerializeField]
	private float occlusionAttenuation = 1f;

	[Range(0f, 1f)]
	[SerializeField]
	private float occlusionBias;

	[Range(0f, 5f)]
	[SerializeField]
	private float displacementStrength = 0.1f;

	[Range(0f, 10f)]
	[SerializeField]
	private float displacementDistance = 5f;

	[Range(20f, 50f)]
	[SerializeField]
	private float edgeSteepnessX = 50f;

	[Range(20f, 50f)]
	[SerializeField]
	private float edgeSteepnessZ = 50f;

	[Range(0f, 360f)]
	[SerializeField]
	private float rotation;

	[SerializeField]
	private float thicknessThreshold = 0.1f;

	[SerializeField]
	private float minShellLength = 0.1f;

	[SerializeField]
	private float maxShellLength = 4.8f;

	[SerializeField]
	private Transform yarnBall;

	[SerializeField]
	private AnimationCurve yarnBallHeightAnimationCurve;

	[SerializeField]
	private AnimationCurve yarnBallSizeAnimationCurve;

	[SerializeField]
	private LineRenderer yarnString;

	[SerializeField]
	private float yarnBallRotationFactor = 90f;

	private Mesh shellMesh;

	private Material sharedShellMaterial;

	private GameObject[] shells;

	private MeshRenderer[] shellRenderers;

	private float thicknessValue;

	private float oldThicknessValue;

	private float thicknessHandleLimit;

	private Vector3 yarnBallRotation;

	private Vector3 yarnBallPosition;

	private static MaterialPropertyBlock mpb;

	private static readonly int _ColorMultiplierId = Shader.PropertyToID("_ColorMultiplier");

	private static readonly int _HueMultiplierId = Shader.PropertyToID("_HueMultiplier");

	private static readonly int _ValueMultiplierId = Shader.PropertyToID("_ValueMultiplier");

	private static readonly int _SaturationMultiplierId = Shader.PropertyToID("_SaturationMultiplier");

	private static readonly int _NoiseToggleId = Shader.PropertyToID("_Noise");

	private static readonly int _NoiseSizeId = Shader.PropertyToID("_NoiseSize");

	private static readonly int _NoiseStrengthId = Shader.PropertyToID("_NoiseStrength");

	private static readonly int _ScaleId = Shader.PropertyToID("_Scale");

	private static readonly int _ShellCountId = Shader.PropertyToID("_ShellCount");

	private static readonly int _ShellIndexId = Shader.PropertyToID("_ShellIndex");

	private static readonly int _ShellLengthId = Shader.PropertyToID("_ShellLength");

	private static readonly int _DensityId = Shader.PropertyToID("_Density");

	private static readonly int _ThicknessId = Shader.PropertyToID("_Thickness");

	private static readonly int _AttenuationId = Shader.PropertyToID("_Attenuation");

	private static readonly int _ShellDistanceAttenuationId = Shader.PropertyToID("_ShellDistanceAttenuation");

	private static readonly int _DisplacementDistanceId = Shader.PropertyToID("_DisplacementDistance");

	private static readonly int _DisplacementStrengthId = Shader.PropertyToID("_DisplacementStrength");

	private static readonly int _CurvatureId = Shader.PropertyToID("_Curvature");

	private static readonly int _OcclusionBiasId = Shader.PropertyToID("_OcclusionBias");

	private static readonly int _NoiseMinId = Shader.PropertyToID("_NoiseMin");

	private static readonly int _NoiseMaxId = Shader.PropertyToID("_NoiseMax");

	private static readonly int _EdgeSteepnessXId = Shader.PropertyToID("_EdgeSteepnessX");

	private static readonly int _EdgeSteepnessZId = Shader.PropertyToID("_EdgeSteepnessZ");

	private static readonly int _RotationId = Shader.PropertyToID("_Rotation");

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		Setup();
	}

	private void Update()
	{
		if (!(thicknessHandle == null))
		{
			thicknessValue = thicknessHandle.transform.localPosition.z / thicknessHandleLimit * 0.5f;
			if (Mathf.Abs(thicknessValue - oldThicknessValue) > thicknessThreshold)
			{
				oldThicknessValue = thicknessValue;
				shellLength = Mathf.Max(thicknessValue * maxShellLength, minShellLength);
				UpdateFluffyShader();
			}
			yarnString.SetPosition(0, thicknessHandle.transform.localPosition);
			yarnBallRotation.x = (0f - thicknessValue) * yarnBallRotationFactor;
			yarnBall.localRotation = Quaternion.Euler(yarnBallRotation);
			yarnBallPosition.y = yarnBallHeightAnimationCurve.Evaluate(1f - thicknessValue);
			yarnBall.localPosition = yarnBallPosition;
			yarnBall.localScale = yarnBallSizeAnimationCurve.Evaluate(1f - thicknessValue) * Vector3.one;
		}
	}

	private IEnumerator AnimateShellLengthCoroutine(float targetShellLength, float duration, float delay = 0f)
	{
		yield return new WaitForSeconds(delay);
		float timer = duration;
		float startingShellLength = shellLength;
		while (timer > 0f)
		{
			float t = 1f - timer / duration;
			shellLength = Mathf.Lerp(startingShellLength, targetShellLength, t);
			UpdateFluffyShader();
			timer -= Time.deltaTime;
			yield return null;
		}
	}

	private void Setup()
	{
		if (!(meshRenderer == null) && !(shellContainer == null) && !(shellMaterialPrefab == null))
		{
			if (mpb == null)
			{
				mpb = new MaterialPropertyBlock();
			}
			shellMesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;
			ClearMeshes();
			SetupMeshes();
			UpdateFluffyShader();
			SetupThicknessSlider();
			meshRenderer.enabled = false;
		}
	}

	private void SetupThicknessSlider()
	{
		if (!(thicknessHandle == null))
		{
			thicknessHandleLimit = thicknessHandle.GetComponent<ConfigurableJoint>().linearLimit.limit;
			thicknessValue = shellLength / maxShellLength;
			Vector3 vector = new Vector3(0f, 0f, thicknessValue * thicknessHandleLimit * 2f);
			thicknessHandle.transform.localPosition = vector;
			yarnString.SetPosition(0, vector);
			yarnBallRotation = yarnBall.localRotation.eulerAngles;
			yarnBallRotation.x = (0f - thicknessValue) * yarnBallRotationFactor;
			yarnBall.localRotation = Quaternion.Euler(yarnBallRotation);
			yarnBallPosition = yarnBall.localPosition;
			yarnBallPosition.y = yarnBallHeightAnimationCurve.Evaluate(1f - thicknessValue);
			yarnBall.localScale = yarnBallSizeAnimationCurve.Evaluate(1f - thicknessValue) * Vector3.one;
		}
	}

	private void ClearMeshes()
	{
		if (shellContainer.childCount == 0)
		{
			return;
		}
		for (int num = shellContainer.childCount - 1; num >= 0; num--)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(shellContainer.GetChild(num).gameObject);
			}
			else
			{
				Object.DestroyImmediate(shellContainer.GetChild(num).gameObject);
			}
		}
	}

	private void SetupMeshes()
	{
		if (sharedShellMaterial != null)
		{
			Object.DestroyImmediate(sharedShellMaterial);
		}
		sharedShellMaterial = new Material(shellMaterialPrefab);
		shells = new GameObject[shellCount];
		shellRenderers = new MeshRenderer[shellCount];
		for (int i = 0; i < shellCount; i++)
		{
			GameObject gameObject = new GameObject("Shell " + i);
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshFilter.sharedMesh = shellMesh;
			meshRenderer.sharedMaterial = sharedShellMaterial;
			gameObject.transform.SetParent(shellContainer, worldPositionStays: false);
			shells[i] = gameObject;
			shellRenderers[i] = meshRenderer;
		}
	}

	private void UpdateFluffyShader()
	{
		if (shellRenderers == null)
		{
			return;
		}
		for (int i = 0; i < shellRenderers.Length; i++)
		{
			MeshRenderer meshRenderer = shellRenderers[i];
			if (!(meshRenderer == null))
			{
				meshRenderer.GetPropertyBlock(mpb);
				mpb.SetColor(_ColorMultiplierId, colorMultiplier);
				mpb.SetFloat(_HueMultiplierId, hueMultiplier);
				mpb.SetFloat(_ValueMultiplierId, valueMultiplier);
				mpb.SetFloat(_SaturationMultiplierId, saturationMultiplier);
				mpb.SetFloat(_NoiseToggleId, noise ? 1f : 0f);
				mpb.SetFloat(_NoiseSizeId, noiseSize);
				mpb.SetFloat(_NoiseStrengthId, noiseStrength);
				mpb.SetVector(_ScaleId, new Vector4(scale.x, scale.y, 0f, 0f));
				mpb.SetInt(_ShellCountId, shellCount);
				mpb.SetInt(_ShellIndexId, i);
				mpb.SetFloat(_ShellLengthId, shellLength);
				mpb.SetFloat(_DensityId, density);
				mpb.SetFloat(_ThicknessId, thickness);
				mpb.SetFloat(_AttenuationId, occlusionAttenuation);
				mpb.SetFloat(_ShellDistanceAttenuationId, distanceAttenuation);
				mpb.SetFloat(_DisplacementDistanceId, displacementDistance);
				mpb.SetFloat(_DisplacementStrengthId, displacementStrength);
				mpb.SetFloat(_CurvatureId, curvature);
				mpb.SetFloat(_OcclusionBiasId, occlusionBias);
				mpb.SetFloat(_NoiseMinId, noiseMin);
				mpb.SetFloat(_NoiseMaxId, noiseMax);
				mpb.SetFloat(_EdgeSteepnessXId, edgeSteepnessX);
				mpb.SetFloat(_EdgeSteepnessZId, edgeSteepnessZ);
				mpb.SetFloat(_RotationId, rotation);
				meshRenderer.SetPropertyBlock(mpb);
			}
		}
	}
}
