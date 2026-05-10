using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using _Scripts.Singletons;
using _Scripts.Wardrobe;

namespace _Scripts.Web;

[RequireComponent(typeof(LineRenderer))]
public class DestroyedWebThreadVisuals : MonoBehaviour
{
	[SerializeField]
	private int qualityPC = 20;

	[SerializeField]
	private int qualityMobile = 10;

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
	private float webAnimationDuration = 0.2f;

	[SerializeField]
	private AnimationCurve affectCurve;

	[SerializeField]
	private AnimationCurve distanceCurve;

	private int quality;

	private WebController webController;

	private Spring spring;

	private LineRenderer lineRenderer;

	private Vector3 positionAnimated;

	private Transform webStartTransform;

	private Vector3 webStartPoint;

	private Vector3 webEndPoint;

	private Vector3 waveDirection;

	private Vector3 webOffset;

	private float distance;

	private TweenerCore<float, float, FloatOptions> webDetachTween;

	private float t;

	private void Awake()
	{
		webController = Singleton<WebController>.Instance;
		lineRenderer = GetComponent<LineRenderer>();
		spring = new Spring();
		spring.SetTarget(0f);
		quality = qualityPC;
	}

	private void LateUpdate()
	{
		_ = webStartPoint;
		_ = webEndPoint;
		DrawWeb();
	}

	public void StartWeb(Transform startTransform, Vector3 endPosition)
	{
		if (startTransform == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		webDetachTween.Kill();
		lineRenderer.positionCount = quality + 1;
		webStartTransform = startTransform;
		webOffset = Vector3.zero;
		webStartPoint = startTransform.position;
		webEndPoint = endPosition;
		distance = (webEndPoint - webStartPoint).magnitude / webController.WebDistance;
		t = 1f;
		webDetachTween = DOTween.To(() => t, delegate(float x)
		{
			t = x;
		}, 0f, webAnimationDuration).SetEase(Ease.InOutQuad).OnComplete(delegate
		{
			UnityEngine.Object.Destroy(base.gameObject);
		});
		RandomizeWaveDirection();
		spring.SetVelocity(velocity);
		spring.SetDamper(damper);
		spring.SetStrength(strength);
	}

	public void StartWeb(Transform startTransform, Vector3 startPosition, Vector3 endPosition)
	{
		webDetachTween.Kill();
		lineRenderer.positionCount = quality + 1;
		webStartTransform = startTransform;
		webOffset = webStartTransform.rotation * webStartTransform.InverseTransformPoint(startPosition) * webStartTransform.lossyScale.x;
		webStartPoint = webStartTransform.position + webOffset;
		webEndPoint = endPosition;
		distance = (webEndPoint - webStartPoint).magnitude / webController.WebDistance;
		t = 1f;
		webDetachTween = DOTween.To(() => t, delegate(float x)
		{
			t = x;
		}, 0f, webAnimationDuration).SetEase(Ease.InOutQuad).OnComplete(delegate
		{
			UnityEngine.Object.Destroy(base.gameObject);
		});
		RandomizeWaveDirection();
		spring.SetVelocity(velocity);
		spring.SetDamper(damper);
		spring.SetStrength(strength);
	}

	private void DrawWeb()
	{
		if (webStartTransform != null)
		{
			webStartPoint = webStartTransform.position + webOffset;
		}
		positionAnimated = Vector3.Lerp(webStartPoint, webEndPoint, t);
		spring.Update(Time.deltaTime);
		for (int i = 0; i < quality + 1; i++)
		{
			float num = (float)i / (float)quality;
			Vector3 vector = waveDirection * (waveHeight * Mathf.Sin(num * waveCount * MathF.PI) * spring.Value * affectCurve.Evaluate(num) * distanceCurve.Evaluate(distance));
			lineRenderer.SetPosition(i, Vector3.Lerp(webStartPoint, positionAnimated, num) + vector);
		}
	}

	private void RandomizeWaveDirection()
	{
		Vector3 right = Singleton<CameraController>.Instance.MainCamera.transform.right;
		Vector3 up = Singleton<CameraController>.Instance.MainCamera.transform.up;
		waveDirection = (right * UnityEngine.Random.Range(-1f, 1f) + up * UnityEngine.Random.Range(-1f, 1f)).normalized;
	}

	public void SetColor(Color webColor, WebSo webSo)
	{
		lineRenderer.startColor = webColor;
		lineRenderer.endColor = webColor;
		lineRenderer.sharedMaterial = webSo.webThreadMaterial;
	}
}
