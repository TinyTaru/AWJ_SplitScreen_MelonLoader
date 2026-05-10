using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using _Scripts.Singletons;
using _Scripts.Wardrobe;

namespace _Scripts.Web;

[RequireComponent(typeof(LineRenderer))]
public class WebThreadVisuals : MonoBehaviour
{
	private enum WebThreadVisualState
	{
		Hidden,
		Attaching,
		Visible,
		Drawn
	}

	[SerializeField]
	private int qualityPC = 20;

	[SerializeField]
	private int qualityMobile = 10;

	[SerializeField]
	private bool reduceToOneQuad;

	[SerializeField]
	private float damper;

	[SerializeField]
	private float strength;

	[SerializeField]
	private float velocity;

	[SerializeField]
	private float waveCount;

	[SerializeField]
	private float waveHeight;

	[SerializeField]
	private float webAnimationDuration = 1f;

	[SerializeField]
	private AnimationCurve affectCurve;

	[SerializeField]
	private AnimationCurve distanceCurve;

	[SerializeField]
	private ParticleSystem webAttachedParticlesPrefab;

	[SerializeField]
	private ParticleSystem webDestroyedParticlesPrefab;

	private int quality;

	private WebController webController;

	private Spring spring;

	private LineRenderer lineRenderer;

	private Vector3 positionAnimated;

	private Transform webStartPoint;

	private Transform webEndPoint;

	private Vector3 waveDirection;

	private float distance;

	private WebThreadVisualState state;

	private TweenerCore<float, float, FloatOptions> webAttachTween;

	private TweenerCore<float, float, FloatOptions> webDetachTween;

	private float t;

	private Vector3 oldWebEndPosition;

	private Color webColor;

	private WebJoint[] webJoints;

	private float zFightingOffset = 0.01f;

	private WebSo webSo;

	public Material WebMaterial => lineRenderer.sharedMaterial;

	private void Awake()
	{
		webController = Singleton<WebController>.Instance;
		lineRenderer = GetComponent<LineRenderer>();
		spring = new Spring();
		spring.SetTarget(0f);
		quality = qualityPC;
		state = WebThreadVisualState.Hidden;
	}

	private void OnDestroy()
	{
		webAttachTween.Kill();
	}

	private void FixedUpdate()
	{
		DrawWeb();
	}

	public void StartWeb(bool playAnimation, float webThickness, WebSo newWebSo)
	{
		webDetachTween.Kill();
		lineRenderer.positionCount = quality + 1;
		lineRenderer.startWidth = webThickness;
		lineRenderer.endWidth = webThickness;
		webSo = newWebSo;
		webStartPoint = webJoints[0].Anchor;
		webEndPoint = webJoints[1].Anchor;
		if (!playAnimation)
		{
			t = 1f;
			state = WebThreadVisualState.Visible;
			if (reduceToOneQuad)
			{
				quality = 1;
				lineRenderer.positionCount = quality + 1;
			}
			return;
		}
		webAttachTween = DOTween.To(() => t, delegate(float x)
		{
			t = x;
		}, 1f, webAnimationDuration).SetEase(Ease.InOutQuad).OnComplete(delegate
		{
			if (reduceToOneQuad)
			{
				quality = 1;
				lineRenderer.positionCount = quality + 1;
			}
			state = WebThreadVisualState.Visible;
			PlayParticles(webSo);
		});
		RandomizeWaveDirection();
		spring.SetVelocity(velocity);
		spring.SetDamper(damper);
		spring.SetStrength(strength);
		state = WebThreadVisualState.Attaching;
	}

	private void PlayParticles(WebSo webSo)
	{
		ParticleSystem particleSystem = UnityEngine.Object.Instantiate(webAttachedParticlesPrefab, webEndPoint.position, Quaternion.identity, null);
		particleSystem.GetComponent<Renderer>().sharedMaterial = webSo.webParticlesMaterial;
		ParticleSystem.MainModule main = particleSystem.main;
		main.startColor = webColor;
	}

	public void StopWeb(Vector3? position)
	{
		webAttachTween.Kill();
		if (lineRenderer != null)
		{
			lineRenderer.enabled = false;
		}
		if (position.HasValue)
		{
			ParticleSystem particleSystem = UnityEngine.Object.Instantiate(webDestroyedParticlesPrefab, position.Value, Quaternion.identity, null);
			particleSystem.GetComponent<Renderer>().sharedMaterial = webSo.webParticlesMaterial;
			ParticleSystem.MainModule main = particleSystem.main;
			main.startColor = webColor;
		}
		state = WebThreadVisualState.Hidden;
	}

	private void DrawWeb()
	{
		if (state == WebThreadVisualState.Hidden || state == WebThreadVisualState.Drawn || webStartPoint == null || webEndPoint == null || !lineRenderer.enabled)
		{
			return;
		}
		if (state == WebThreadVisualState.Visible)
		{
			for (int i = 0; i < quality + 1; i++)
			{
				float num = (float)i / (float)quality;
				lineRenderer.SetPosition(i, Vector3.Lerp(webStartPoint.position, webEndPoint.position, num));
			}
			if (webJoints[0].IsKinematic && webJoints[1].IsKinematic)
			{
				state = WebThreadVisualState.Drawn;
			}
			return;
		}
		positionAnimated = Vector3.Lerp(webStartPoint.position, webEndPoint.position, t);
		spring.Update(Time.fixedDeltaTime);
		distance = (positionAnimated - webStartPoint.position).magnitude / webController.WebDistance;
		for (int j = 0; j < quality + 1; j++)
		{
			float num2 = (float)j / (float)quality;
			Vector3 zero = Vector3.zero;
			zero = waveDirection * (waveHeight * Mathf.Sin(num2 * waveCount * MathF.PI) * spring.Value * affectCurve.Evaluate(num2) * distanceCurve.Evaluate(distance));
			if (j == 0)
			{
				zero += zFightingOffset * (positionAnimated - webStartPoint.position).normalized;
			}
			lineRenderer.SetPosition(j, Vector3.Lerp(webStartPoint.position, positionAnimated, num2) + zero);
		}
	}

	private void RandomizeWaveDirection()
	{
		Vector3 right = Singleton<CameraController>.Instance.MainCamera.transform.right;
		Vector3 up = Singleton<CameraController>.Instance.MainCamera.transform.up;
		waveDirection = (right * UnityEngine.Random.Range(-1f, 1f) + up * UnityEngine.Random.Range(-1f, 1f)).normalized;
	}

	public void SetWebJoints(WebJoint[] newWebJoints)
	{
		webJoints = new WebJoint[newWebJoints.Length];
		for (int i = 0; i < newWebJoints.Length; i++)
		{
			webJoints[i] = newWebJoints[i];
		}
	}

	public void SetColor(Color newWebColor)
	{
		webColor = newWebColor;
		lineRenderer.startColor = webColor;
		lineRenderer.endColor = webColor;
	}
}
