using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.Wardrobe;

namespace _Scripts.Web;

[RequireComponent(typeof(LineRenderer))]
public class MainWebVisuals : MonoBehaviour
{
	private enum MainWebVisualState
	{
		Hidden,
		Attaching,
		Visible
	}

	[SerializeField]
	private int qualityPC = 50;

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
	private float webAnimationDuration = 1f;

	[SerializeField]
	private AnimationCurve affectCurve;

	[SerializeField]
	private AnimationCurve distanceCurve;

	[SerializeField]
	private ParticleSystem webAttachedParticleSystem;

	[SerializeField]
	private DestroyedWebThreadVisuals destroyedWebThreadPrefab;

	private int quality;

	private WebController webController;

	private Spring spring;

	private LineRenderer lineRenderer;

	private Vector3 positionAnimated;

	private Transform webStartPoint;

	private Transform webEndPoint;

	private BodyMovement player;

	private Vector3 waveDirection;

	private float distance;

	private MainWebVisualState state;

	private TweenerCore<float, float, FloatOptions> webAttachTween;

	private float t;

	private Vector3 oldWebEndPosition;

	private Color webColor;

	private Material webMaterial;

	private WebSo webSo;

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
		spring = new Spring();
		spring.SetTarget(0f);
		lineRenderer.enabled = false;
		lineRenderer.SetPositions(new Vector3[2]
		{
			Vector3.zero,
			Vector3.zero
		});
		quality = qualityPC;
		state = MainWebVisualState.Hidden;
	}

	private void Start()
	{
		player = Singleton<GameController>.Instance.Player;
		webController = Singleton<WebController>.Instance;
		if (webController != null)
		{
			webController.OnWebColorChanged += WebController_OnWebColorChanged;
			webController.OnMainWebActivated += WebController_OnMainWebActivated;
			webController.OnMainWebDeactivated += WebController_OnMainWebDeactivated;
		}
	}

	private void LateUpdate()
	{
		DrawWeb();
	}

	private void StartWeb()
	{
		if (state != MainWebVisualState.Visible)
		{
			lineRenderer.enabled = true;
			lineRenderer.positionCount = quality + 1;
			webStartPoint = (SettingsController.ArachnophobiaMode ? player.Ball : player.Root);
			webEndPoint = webController.WebTarget;
			webAttachTween = DOTween.To(() => t, delegate(float x)
			{
				t = x;
			}, 1f, webAnimationDuration).SetEase(Ease.InOutQuad).OnComplete(delegate
			{
				state = MainWebVisualState.Visible;
			});
			RandomizeWaveDirection();
			spring.SetVelocity(velocity);
			spring.SetDamper(damper);
			spring.SetStrength(strength);
			webAttachedParticleSystem.transform.position = webEndPoint.position;
			ParticleSystem.MainModule main = webAttachedParticleSystem.main;
			if (webSo != null)
			{
				webAttachedParticleSystem.GetComponent<Renderer>().sharedMaterial = webSo.webParticlesMaterial;
			}
			main.startColor = webColor;
			webAttachedParticleSystem.Play();
			state = MainWebVisualState.Attaching;
		}
	}

	private void StopWeb()
	{
		if (state != 0)
		{
			webAttachTween.Kill();
			state = MainWebVisualState.Hidden;
			lineRenderer.enabled = false;
			lineRenderer.positionCount = 0;
		}
	}

	private void DrawWeb()
	{
		if (state == MainWebVisualState.Hidden)
		{
			return;
		}
		if (state == MainWebVisualState.Visible)
		{
			for (int i = 0; i < quality + 1; i++)
			{
				float num = (float)i / (float)quality;
				lineRenderer.SetPosition(i, Vector3.Lerp(webStartPoint.position, webEndPoint.position, num));
			}
			return;
		}
		if (state == MainWebVisualState.Attaching)
		{
			positionAnimated = Vector3.Lerp(webStartPoint.position, webEndPoint.position, t);
		}
		spring.Update(Time.deltaTime);
		distance = (positionAnimated - webStartPoint.position).magnitude / webController.WebDistance;
		for (int j = 0; j < quality + 1; j++)
		{
			float num2 = (float)j / (float)quality;
			Vector3 vector = waveDirection * (waveHeight * Mathf.Sin(num2 * waveCount * MathF.PI) * spring.Value * affectCurve.Evaluate(num2) * distanceCurve.Evaluate(distance));
			lineRenderer.SetPosition(j, Vector3.Lerp(webStartPoint.position, positionAnimated, num2) + vector);
		}
	}

	private void RandomizeWaveDirection()
	{
		Vector3 right = Singleton<CameraController>.Instance.MainCamera.transform.right;
		Vector3 up = Singleton<CameraController>.Instance.MainCamera.transform.up;
		waveDirection = (right * UnityEngine.Random.Range(-1f, 1f) + up * UnityEngine.Random.Range(-1f, 1f)).normalized;
	}

	private void WebController_OnWebColorChanged(object sender, WebController.OnWebColorChangedEventArgs e)
	{
		if (e.cosmeticItemWebSo != null)
		{
			webSo = e.cosmeticItemWebSo.webSo;
			webMaterial = webSo.webThreadMaterial;
			lineRenderer.sharedMaterial = webMaterial;
		}
		webColor = e.webColor;
		lineRenderer.startColor = webColor;
		lineRenderer.endColor = webColor;
	}

	private void WebController_OnMainWebActivated(object sender, WebController.OnMainWebActivatedEventArgs e)
	{
		webSo = e.cosmeticItemWebSo.webSo;
		webMaterial = e.cosmeticItemWebSo.webSo.webThreadMaterial;
		lineRenderer.sharedMaterial = webMaterial;
		webColor = e.webColor;
		lineRenderer.startColor = webColor;
		lineRenderer.endColor = webColor;
		StartWeb();
	}

	private void WebController_OnMainWebDeactivated(object sender, WebController.OnMainWebDeactivatedEventArgs e)
	{
		if (e.webTargetParent != null && e.playAnimation)
		{
			DestroyedWebThreadVisuals destroyedWebThreadVisuals = UnityEngine.Object.Instantiate(destroyedWebThreadPrefab);
			destroyedWebThreadVisuals.StartWeb(e.webTargetParent, e.webTargetPosition, webController.WebStartPoint.position);
			destroyedWebThreadVisuals.SetColor(webColor, webSo);
		}
		StopWeb();
	}
}
