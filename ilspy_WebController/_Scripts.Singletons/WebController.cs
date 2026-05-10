using System;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.LevelSaving;
using _Scripts.LivingRoom;
using _Scripts.Objects;
using _Scripts.Spider;
using _Scripts.UI.MobileMonetization;
using _Scripts.Utils;
using _Scripts.Wardrobe;
using _Scripts.Web;

namespace _Scripts.Singletons;

public class WebController : Singleton<WebController>
{
	public class OnMainWebActivatedEventArgs : EventArgs
	{
		public Color webColor;

		public CosmeticItemWebSo cosmeticItemWebSo;
	}

	public class OnMainWebDeactivatedEventArgs : EventArgs
	{
		public Transform webTargetParent;

		[FormerlySerializedAs("webTargetTransform")]
		public Vector3 webTargetPosition;

		public bool playAnimation;
	}

	public class OnWebColorChangedEventArgs : EventArgs
	{
		public Color webColor;

		public CosmeticItemWebSo cosmeticItemWebSo;
	}

	private enum WebMode
	{
		Default,
		Building
	}

	private enum WebBuildingMode
	{
		MovingAnchor,
		FixedAnchor
	}

	[SerializeField]
	private float defaultWebDistance = 110f;

	[SerializeField]
	private float defaultWebThickness = 0.66666f;

	[SerializeField]
	private float webColliderRadius = 0.66666f;

	[SerializeField]
	private LayerMask whatCanBeWebbed;

	[SerializeField]
	private LayerMask whatIsWeb;

	[SerializeField]
	private LayerMask whatIsWebJoint;

	[SerializeField]
	private LayerMask whatBlocksWebBuilding;

	[SerializeField]
	private Transform webTargetPrefab;

	[SerializeField]
	private Transform webAnchorPrefab;

	[SerializeField]
	private Transform webTargetGfx;

	[SerializeField]
	private Transform webAnchorGfx;

	[SerializeField]
	private Collider webTargetCollider;

	[SerializeField]
	private Material webTargetDefaultMaterial;

	[SerializeField]
	private Material webAnchorFixedAnchorMaterial;

	[SerializeField]
	private Material webTargetFixedAnchorMaterial;

	[SerializeField]
	private Material webAnchorMovingAnchorMaterial;

	[SerializeField]
	private Material webTargetMovingAnchorMaterial;

	[SerializeField]
	private WebThread webThreadPrefab;

	[SerializeField]
	private float raycastMaxRadiusWebJoint = 0.5f;

	[SerializeField]
	private float raycastMaxRadiusWeb = 0.5f;

	[SerializeField]
	private float raycastMaxRadius;

	[SerializeField]
	private int rayCastStepAmount;

	[SerializeField]
	private float webJointFavorDistance;

	[SerializeField]
	private Transform webContainer;

	[SerializeField]
	private Transform webJointContainer;

	[SerializeField]
	private Transform wardrobeWebContainer;

	[SerializeField]
	private Transform wardrobeJointContainer;

	[SerializeField]
	private WebJoint webJointPrefab;

	[SerializeField]
	private LineRenderer webIndicationLineRenderer;

	[SerializeField]
	private AnimationCurve webTargetSize;

	[SerializeField]
	private float minBuildDistance;

	[SerializeField]
	private float midAirAimAssistFactor = 3f;

	[SerializeField]
	private float midAirAimAssistFactorMobile = 10f;

	[SerializeField]
	private WebColorPaletteSO[] webColorPalettes;

	[SerializeField]
	private float fixWebIntervalFast = 0.2f;

	[SerializeField]
	private float fixWebIntervalSlow = 2f;

	[SerializeField]
	private float deletePlayerWebsHoldDuration = 1f;

	[Header("Ancient Potions")]
	[SerializeField]
	private float ancientPotionWebDistanceMultiplier = 2f;

	[SerializeField]
	private float hueInterval = 0.02f;

	[Header("Web Types")]
	[SerializeField]
	private WebThread webThreadPrefabNormal;

	[SerializeField]
	private WebThread webThreadPrefabRGB;

	[Header("Spring Joint Swinging")]
	[SerializeField]
	private float springSwinging = 5f;

	[SerializeField]
	private float springSwingingBoost = 10f;

	[SerializeField]
	private float damperSwinging = 10f;

	[SerializeField]
	private float minDistancePercentage;

	[SerializeField]
	private float maxDistancePercentage;

	[Header("Spring Joint Web")]
	private float springWeb = 50f;

	private float damperWeb = 10f;

	private float minDistancePercentageWeb = 0.25f;

	private float maxDistancePercentageWeb = 0.25f;

	[SerializeField]
	private float springWebNormal;

	[SerializeField]
	private float damperWebNormal;

	[SerializeField]
	private float minDistancePercentageWebNormal = 0.25f;

	[SerializeField]
	private float maxDistancePercentageWebNormal = 0.25f;

	[SerializeField]
	private float springWebStrong;

	[SerializeField]
	private float damperWebStrong;

	[SerializeField]
	private float minDistancePercentageWebStrong = 0.25f;

	[SerializeField]
	private float maxDistancePercentageWebStrong = 0.25f;

	[Header("Spring Joint Movable Object")]
	[SerializeField]
	private float springMovableObject = 5f;

	[SerializeField]
	private float damperMovableObject = 10f;

	[SerializeField]
	private float minDistancePercentageMovableObject;

	[SerializeField]
	private float maxDistancePercentageMovableObject;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference shootWebInputAction;

	[SerializeField]
	private InputActionReference quickBuildInputAction;

	[SerializeField]
	private InputActionReference mobileShootWebInputAction;

	[SerializeField]
	private InputActionReference mobileQuickBuildInputAction;

	[SerializeField]
	private InputActionReference attachMovingAnchorInputAction;

	[SerializeField]
	private InputActionReference attachFixedAnchorInputAction;

	[SerializeField]
	private InputActionReference deleteWebInputAction;

	private float webDistance;

	private Color webColor;

	private float webThickness;

	private BodyMovement bodyMovement;

	private Transform webStartPoint;

	private WebMode webMode;

	private WebBuildingMode webBuildingMode;

	private bool webActive;

	private bool webTargetActive;

	private bool webAnchorActive;

	private SpringJoint springJoint;

	private int webColorIndex;

	private Transform[] webArray;

	private GameObject webTargetObject;

	private GameObject oldWebTargetObject;

	private GameObject oldWebTargetObject1;

	private GameObject webAnchorObject;

	private WebThread webThreadToDestroy;

	private WebJoint webJointToDestroy;

	private bool canAim = true;

	private bool canDeleteWebs = true;

	private bool canShootWebs = true;

	private bool canQuickBuild = true;

	private bool canBuildWebs = true;

	private bool disableWebTarget;

	private Transform webTarget;

	private Transform webAnchor;

	private bool shootWebPressed;

	private float fixWebTimer;

	private bool deleteWebPressed;

	private float deletePlayerWebsTimer;

	private Dictionary<string, WebJoint> generatedWebJointDict;

	private WebJoint playerWebJoint;

	private List<WebJoint> webJointList;

	private float ancientPotionHue;

	private bool ancientPotionHueWebActive;

	private string webSound;

	private StudioEventEmitter buildThreadSound;

	private StudioEventEmitter attachAnchorSound;

	private StudioEventEmitter deleteThreadSound;

	private StudioEventEmitter cantBuildSound;

	private StudioEventEmitter attachToPlayerSound;

	private StudioEventEmitter musicThreadSound;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorId = Shader.PropertyToID("_Color");

	public bool WebTargetActive => webTargetActive;

	public Transform WebTarget => webTarget;

	public GameObject WebTargetObject => webTargetObject;

	public bool CanShootWebs => canShootWebs;

	public bool WebActive => webActive;

	public bool MovingAnchorActive
	{
		get
		{
			if (webBuildingMode == WebBuildingMode.MovingAnchor)
			{
				return WebAnchorActive;
			}
			return false;
		}
	}

	public bool FixedAnchorActive
	{
		get
		{
			if (webBuildingMode == WebBuildingMode.FixedAnchor)
			{
				return WebAnchorActive;
			}
			return false;
		}
	}

	public bool WebAnchorActive => webAnchorActive;

	public Vector3 WebDirection => (webTarget.position - webStartPoint.position).normalized;

	public float WebDistance => webDistance;

	public Transform WebStartPoint => webStartPoint;

	public float WebColliderRadius => webColliderRadius;

	public GameObject WebAnchorObject => webAnchorObject;

	public event System.EventHandler OnModeChanged;

	public event Action OnWebBuild;

	public event EventHandler<OnMainWebActivatedEventArgs> OnMainWebActivated;

	public event EventHandler<OnMainWebDeactivatedEventArgs> OnMainWebDeactivated;

	public event EventHandler<OnWebColorChangedEventArgs> OnWebColorChanged;

	protected override void Awake()
	{
		base.Awake();
		if (Singleton<WebController>.Instance == this)
		{
			wardrobeWebContainer.gameObject.SetActive(value: false);
			wardrobeJointContainer.gameObject.SetActive(value: false);
		}
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		webThickness = defaultWebThickness;
		webJointList = new List<WebJoint>();
	}

	private void OnEnable()
	{
		shootWebInputAction.action.performed += OnShootWeb;
		quickBuildInputAction.action.performed += OnQuickBuild;
		attachFixedAnchorInputAction.action.performed += OnAttachFixedAnchor;
		attachMovingAnchorInputAction.action.performed += OnAttachMovingAnchor;
		deleteWebInputAction.action.performed += OnDeleteWeb;
	}

	private void OnDisable()
	{
		shootWebInputAction.action.performed -= OnShootWeb;
		quickBuildInputAction.action.performed -= OnQuickBuild;
		attachFixedAnchorInputAction.action.performed -= OnAttachFixedAnchor;
		attachMovingAnchorInputAction.action.performed -= OnAttachMovingAnchor;
		deleteWebInputAction.action.performed -= OnDeleteWeb;
	}

	private void Start()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnPauseGame += GameController_OnPauseGame;
		}
		webTarget = UnityEngine.Object.Instantiate(webTargetPrefab, base.transform);
		AssignSounds();
		webAnchor = UnityEngine.Object.Instantiate(webAnchorPrefab, base.transform);
		webMode = WebMode.Default;
		webBuildingMode = WebBuildingMode.MovingAnchor;
		bodyMovement = Singleton<GameController>.Instance.Player;
		webStartPoint = bodyMovement.Root;
		webDistance = defaultWebDistance;
		webTargetActive = false;
		webTargetGfx.gameObject.SetActive(webTargetActive);
		webAnchorActive = false;
		webAnchorGfx.gameObject.SetActive(WebAnchorActive);
		ReleaseWeb();
		webColorIndex = SaveController.Load("WebColor", 0, SaveData.Wardrobe);
		if (!Singleton<CosmeticItemsController>.Instance.IsWebItem(webColorIndex))
		{
			webColorIndex = Singleton<CosmeticItemsController>.Instance.GetDefaultWebIndex();
			SaveController.Save("WebColor", webColorIndex, SaveData.Wardrobe);
		}
		SetWebColor(webColorIndex);
		SetWebType(WebType.Normal);
		playerWebJoint = bodyMovement.gameObject.AddComponent<WebJoint>();
		playerWebJoint.SetupPlayerWebJoint();
		playerWebJoint.SetAnchor(Singleton<GameController>.Instance.Player.Root);
		fixWebTimer = fixWebIntervalSlow;
		if (Singleton<AncientPotionController>.Instance != null)
		{
			Singleton<AncientPotionController>.Instance.OnEffectStarted += AncientPotionController_OnEffectStarted;
			Singleton<AncientPotionController>.Instance.OnEffectEnded += AncientPotionController_OnEffectEnded;
		}
	}

	private void Update()
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Paused || Singleton<GameController>.Instance.State == GameController.GameState.Cutscene || Singleton<GameController>.Instance.State == GameController.GameState.Dialogue || Singleton<GameController>.Instance.State == GameController.GameState.FreeLook || Singleton<GameController>.Instance.State == GameController.GameState.PerformEmote)
		{
			return;
		}
		if (webTarget == null)
		{
			webTarget = UnityEngine.Object.Instantiate(webTargetPrefab, base.transform);
			AssignSounds();
		}
		if (webAnchor == null)
		{
			webAnchor = UnityEngine.Object.Instantiate(webAnchorPrefab, base.transform);
		}
		if (canAim)
		{
			webTargetGfx.position = webTarget.position;
			webTargetGfx.localScale = Vector3.one * webTargetSize.Evaluate(Vector3.Distance(webTargetGfx.position, webStartPoint.position) / webDistance);
			webAnchorGfx.position = webAnchor.position;
			webAnchorGfx.localScale = Vector3.one * webTargetSize.Evaluate(Vector3.Distance(webAnchorGfx.position, webStartPoint.position) / webDistance);
		}
		else
		{
			webTargetGfx.gameObject.SetActive(value: false);
			webAnchorGfx.gameObject.SetActive(value: false);
		}
		fixWebTimer -= Time.deltaTime;
		if (fixWebTimer <= 0f)
		{
			FixWeb();
		}
		if (deleteWebPressed)
		{
			deletePlayerWebsTimer -= Time.deltaTime;
			if (deletePlayerWebsTimer <= 0f)
			{
				deleteWebPressed = false;
				DestroyAllPlayerWebs(playAnimation: true);
			}
		}
	}

	private void FixedUpdate()
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Paused || Singleton<GameController>.Instance.State == GameController.GameState.Cutscene || Singleton<GameController>.Instance.State == GameController.GameState.Dialogue || Singleton<GameController>.Instance.State == GameController.GameState.FreeLook || Singleton<GameController>.Instance.State == GameController.GameState.PerformEmote)
		{
			return;
		}
		if (webTarget == null)
		{
			webTarget = UnityEngine.Object.Instantiate(webTargetPrefab, base.transform);
			AssignSounds();
		}
		if (webAnchor == null)
		{
			webAnchor = UnityEngine.Object.Instantiate(webAnchorPrefab, base.transform);
		}
		UpdateSpringJointAnchor();
		switch (bodyMovement.State)
		{
		case BodyMovement.MovementState.Walking:
			CheckForWebTarget();
			break;
		case BodyMovement.MovementState.Jumping:
			CheckForWebTarget(midAirAimAssistFactor);
			if (webActive)
			{
				ActivateSpringJoint(preferPlayer: true);
			}
			break;
		case BodyMovement.MovementState.ResetLegs:
			CheckForWebTarget();
			break;
		}
		WebMode webMode = this.webMode;
		if (webMode != 0 && webMode == WebMode.Building)
		{
			IndicateWeb();
		}
	}

	private void AssignSounds()
	{
		WebTarget component = webTarget.GetComponent<WebTarget>();
		attachAnchorSound = component.attachAnchorSound;
		buildThreadSound = component.buildThreadSound;
		cantBuildSound = component.cantBuildSound;
		deleteThreadSound = component.deleteThreadSound;
		attachToPlayerSound = component.attachToPlayerSound;
		musicThreadSound = component.musicThreadSound;
	}

	public List<WebThread> CreateDefaultWeb(DefaultWeb defaultWeb, Color defaultWebColor, float modifiedWebThickness, WebThread defaultWebThread, int newWebIndex)
	{
		List<WebThread> list = new List<WebThread>();
		LineRenderer[] webLineRenderers = defaultWeb.WebLineRenderers;
		if (webLineRenderers != null)
		{
			LineRenderer[] array = webLineRenderers;
			foreach (LineRenderer lineRenderer in array)
			{
				ModifyWebThickness(modifiedWebThickness);
				WebJoint[] array2 = new WebJoint[lineRenderer.positionCount];
				for (int j = 0; j < lineRenderer.positionCount; j++)
				{
					WebJoint webJoint = UnityEngine.Object.Instantiate(webJointPrefab, webJointContainer);
					webJoint.SetupWebJoint(base.gameObject, lineRenderer.transform.TransformPoint(lineRenderer.GetPosition(j)));
					array2[j] = webJoint;
					webJointList.Add(webJoint);
				}
				for (int k = 0; k < lineRenderer.positionCount - 1; k++)
				{
					WebJoint newWebJointAnchor;
					WebJoint newWebJointTarget;
					WebThread item = CreateNewWebThread(array2[k], null, null, Vector3.zero, out newWebJointAnchor, array2[k + 1], null, null, Vector3.zero, out newWebJointTarget, newWebIndex, isDefaultWeb: true, defaultWebColor, defaultWebThread);
					list.Add(item);
				}
				ResetWebThickness();
			}
		}
		SplineComputer[] splines = defaultWeb.Splines;
		if (splines != null)
		{
			SplineComputer[] array3 = splines;
			foreach (SplineComputer splineComputer in array3)
			{
				ModifyWebThickness(modifiedWebThickness);
				WebJoint[] array4 = new WebJoint[splineComputer.pointCount];
				for (int l = 0; l < splineComputer.pointCount; l++)
				{
					WebJoint webJoint2 = UnityEngine.Object.Instantiate(webJointPrefab, webJointContainer);
					webJoint2.SetupWebJoint(base.gameObject, splineComputer.GetPoint(l).position);
					array4[l] = webJoint2;
					webJointList.Add(webJoint2);
				}
				for (int m = 0; m < splineComputer.pointCount - 1; m++)
				{
					WebJoint newWebJointAnchor2;
					WebJoint newWebJointTarget2;
					WebThread item2 = CreateNewWebThread(array4[m], null, null, Vector3.zero, out newWebJointAnchor2, array4[m + 1], null, null, Vector3.zero, out newWebJointTarget2, newWebIndex, isDefaultWeb: true, defaultWebColor, defaultWebThread);
					list.Add(item2);
				}
				ResetWebThickness();
			}
		}
		return list;
	}

	private void IndicateWeb()
	{
		if (WebAnchorActive && webTargetActive)
		{
			webIndicationLineRenderer.enabled = true;
			webIndicationLineRenderer.SetPositions(new Vector3[2] { webAnchor.position, webTarget.position });
		}
		else
		{
			webIndicationLineRenderer.enabled = false;
		}
	}

	public void ReleaseWeb(bool playAnimation = true)
	{
		if (webActive)
		{
			DeactivateSpringJoint();
			webActive = false;
			webTargetCollider.enabled = false;
			oldWebTargetObject = null;
			this.OnMainWebDeactivated?.Invoke(this, new OnMainWebDeactivatedEventArgs
			{
				webTargetParent = ((webTargetObject == null) ? null : webTargetObject.transform),
				webTargetPosition = webTarget.position,
				playAnimation = playAnimation
			});
		}
	}

	private void ReleaseAnchor()
	{
		webAnchorActive = false;
		webAnchorGfx.gameObject.SetActive(WebAnchorActive);
		if (webAnchor != null)
		{
			webAnchor.parent = base.transform;
		}
		webAnchorObject = null;
		webTargetGfx.GetComponent<MeshRenderer>().sharedMaterial = webTargetDefaultMaterial;
		SetWebTargetColor();
	}

	private void ReleaseTarget()
	{
		webTargetActive = false;
		webTargetGfx.gameObject.SetActive(webTargetActive);
		webTarget.parent = base.transform;
		webTargetObject = null;
	}

	private bool ActivateSpringJoint(bool preferPlayer = false)
	{
		if (springJoint != null)
		{
			return false;
		}
		if (webTargetObject == null)
		{
			return false;
		}
		bool flag = bodyMovement.TargetRigidbody != null && bodyMovement.TargetRigidbody.gameObject == webTargetObject;
		WebJoint webJoint = webTargetObject.GetComponent<WebJoint>();
		WebThread componentInParent = webTargetObject.GetComponentInParent<WebThread>();
		if (componentInParent != null && componentInParent.gameObject.layer == LayerMask.NameToLayer("PlayerWeb"))
		{
			return false;
		}
		if (bodyMovement.State != BodyMovement.MovementState.Jumping && componentInParent != null)
		{
			webJoint = SplitWebThread(componentInParent, webTarget.position);
			webTargetObject = webJoint.gameObject;
			webTarget.parent = webTargetObject.transform;
			webActive = true;
			webTargetCollider.enabled = true;
		}
		MovableObject component = webTargetObject.GetComponent<MovableObject>();
		if ((component == null && webJoint == null) || flag)
		{
			Time.timeScale = 1f;
			springJoint = webTarget.gameObject.AddComponent<SpringJoint>();
			springJoint.spring = springSwinging;
			springJoint.damper = damperSwinging;
			float magnitude = (webTarget.position - bodyMovement.Rb.transform.position).magnitude;
			springJoint.minDistance = magnitude * minDistancePercentage;
			springJoint.maxDistance = magnitude * maxDistancePercentage;
		}
		else
		{
			springJoint = webTargetObject.AddComponent<SpringJoint>();
			springJoint.spring = springMovableObject;
			springJoint.damper = damperMovableObject;
			float magnitude2 = (webTarget.position - bodyMovement.Rb.transform.position).magnitude;
			springJoint.minDistance = magnitude2 * minDistancePercentageMovableObject;
			springJoint.maxDistance = magnitude2 * maxDistancePercentageMovableObject;
			springJoint.anchor = webTargetObject.transform.InverseTransformPoint(webTarget.position);
			springJoint.enableCollision = true;
			if (webTargetObject != oldWebTargetObject && component != null)
			{
				component.MainWebAttached();
			}
		}
		if (bodyMovement.TargetRigidbody == null || bodyMovement.TargetRigidbody.isKinematic || preferPlayer || bodyMovement.TargetRigidbody.gameObject == webTargetObject || bodyMovement.State == BodyMovement.MovementState.Jumping)
		{
			springJoint.connectedBody = bodyMovement.Rb;
		}
		else
		{
			springJoint.connectedBody = bodyMovement.TargetRigidbody;
		}
		springJoint.autoConfigureConnectedAnchor = false;
		springJoint.connectedAnchor = Vector3.zero;
		oldWebTargetObject = webTargetObject;
		return true;
	}

	private void DeactivateSpringJoint(bool deactivateMainWeb = true)
	{
		if (!(springJoint == null))
		{
			MovableObject componentInParent = springJoint.GetComponentInParent<MovableObject>();
			if (deactivateMainWeb && componentInParent != null)
			{
				componentInParent.MainWebReleased();
			}
			UnityEngine.Object.Destroy(springJoint);
			springJoint = null;
		}
	}

	private void UpdateSpringJointAnchor()
	{
		if (!(springJoint == null) && !(springJoint.connectedBody == null) && springJoint.connectedBody == bodyMovement.TargetRigidbody)
		{
			Vector3 connectedAnchor = springJoint.connectedBody.transform.InverseTransformPoint(bodyMovement.transform.position);
			MovableObject component = bodyMovement.TargetRigidbody.GetComponent<MovableObject>();
			if (component != null && component.UseCenterOfMassAsWebAnchor)
			{
				springJoint.connectedAnchor = bodyMovement.TargetRigidbody.centerOfMass + new Vector3(connectedAnchor.x, 0f, connectedAnchor.z);
			}
			else
			{
				springJoint.connectedAnchor = connectedAnchor;
			}
		}
	}

	private void AttachWeb()
	{
		if (!webTargetActive || webTargetObject == null)
		{
			return;
		}
		if (ActivateSpringJoint())
		{
			webActive = true;
			webTargetCollider.enabled = true;
			webStartPoint = (SettingsController.ArachnophobiaMode ? bodyMovement.Ball : bodyMovement.Root);
			WebModifier componentInParent = webTarget.GetComponentInParent<WebModifier>();
			if (componentInParent == null || !componentInParent.SuppressAttachSound())
			{
				PlayBuildWebSound(bodyMovement.Rb.transform);
			}
			this.OnMainWebActivated?.Invoke(this, new OnMainWebActivatedEventArgs
			{
				webColor = webColor,
				cosmeticItemWebSo = (CosmeticItemWebSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(webColorIndex)
			});
		}
		else
		{
			cantBuildSound.Play();
		}
	}

	private void PlayBuildWebSound(Transform webStartPosition)
	{
		string text = webSound;
		if (!(text == "Web"))
		{
			if (text == "Music")
			{
				float magnitude = (webTarget.position - webStartPosition.position).magnitude;
				float value = Mathf.Lerp(0f, 14f, magnitude / webDistance);
				musicThreadSound.Play();
				musicThreadSound.SetParameter("music_silk_pitch", value);
			}
		}
		else
		{
			buildThreadSound.Play();
		}
	}

	private void CheckForWebTarget(float raycastRadiusFactor = 1f)
	{
		if (!canAim || webActive)
		{
			return;
		}
		if ((webTarget.position - webStartPoint.position).magnitude > webDistance)
		{
			webActive = false;
			webTargetGfx.gameObject.SetActive(webActive);
			webTargetCollider.enabled = false;
		}
		webTarget.parent = base.transform;
		Transform transform = Singleton<CameraController>.Instance.MainCamera.transform;
		Vector3 forward = transform.forward;
		Ray ray = new Ray(transform.position + forward * Singleton<CameraController>.Instance.GetCameraDistance(), forward);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		RaycastHit hitInfo = default(RaycastHit);
		RaycastHit hitInfo2 = default(RaycastHit);
		RaycastHit hitInfo3 = default(RaycastHit);
		if (Physics.Raycast(ray, out hitInfo, webDistance, whatIsWebJoint))
		{
			flag = true;
		}
		else
		{
			for (int i = 1; i <= rayCastStepAmount; i++)
			{
				if (Physics.SphereCast(ray, raycastMaxRadiusWebJoint * raycastRadiusFactor / (float)rayCastStepAmount * (float)i, out hitInfo, webDistance, whatIsWebJoint))
				{
					flag = true;
					break;
				}
			}
		}
		if (Physics.Raycast(ray, out hitInfo2, webDistance, whatIsWeb))
		{
			flag2 = true;
		}
		else
		{
			for (int j = 1; j <= rayCastStepAmount; j++)
			{
				if (Physics.SphereCast(ray, raycastMaxRadiusWeb * raycastRadiusFactor / (float)rayCastStepAmount * (float)j, out hitInfo2, webDistance, whatIsWeb))
				{
					flag2 = true;
					break;
				}
			}
		}
		if (Physics.Raycast(ray, out hitInfo3, webDistance, whatCanBeWebbed))
		{
			flag3 = true;
		}
		else
		{
			for (int k = 1; k <= rayCastStepAmount; k++)
			{
				if (Physics.SphereCast(ray, raycastMaxRadius * raycastRadiusFactor / (float)rayCastStepAmount * (float)k, out hitInfo3, webDistance, whatCanBeWebbed))
				{
					flag3 = true;
					break;
				}
			}
		}
		float num = (flag ? (hitInfo.point - webStartPoint.position).magnitude : float.MaxValue);
		float num2 = (flag2 ? (hitInfo2.point - webStartPoint.position).magnitude : float.MaxValue);
		float num3 = (flag3 ? (hitInfo3.point - webStartPoint.position).magnitude : float.MaxValue);
		if ((flag || flag2 || flag3) && Physics.Raycast(ray, out var _, Mathf.Min(num, num2, num3), whatBlocksWebBuilding))
		{
			webTargetActive = false;
			webTargetGfx.gameObject.SetActive(webTargetActive && !disableWebTarget);
			webTargetObject = null;
			return;
		}
		if (flag)
		{
			if (flag2)
			{
				if (flag3)
				{
					float num4 = Mathf.Min(num, num2, num3);
					if (Math.Abs(num4 - num) < 0.01f)
					{
						SetWebTarget(hitInfo);
					}
					else if (Math.Abs(num4 - num2) < 0.01f)
					{
						if (Mathf.Abs(num - num2) < webJointFavorDistance)
						{
							SetWebTarget(hitInfo);
						}
						else
						{
							SetWebTarget(hitInfo2);
						}
					}
					else if (Mathf.Abs(num - num3) < webJointFavorDistance)
					{
						SetWebTarget(hitInfo);
					}
					else
					{
						SetWebTarget(hitInfo3);
					}
				}
				else if (num < num2)
				{
					SetWebTarget(hitInfo);
				}
				else
				{
					SetWebTarget(hitInfo2);
				}
			}
			else if (num < num3)
			{
				SetWebTarget(hitInfo);
			}
			else
			{
				SetWebTarget(hitInfo3);
			}
		}
		else if (flag2 && num2 < num3)
		{
			SetWebTarget(hitInfo2);
		}
		else if (flag3)
		{
			SetWebTarget(hitInfo3);
		}
		webTargetActive = flag || flag2 || flag3;
		webTargetGfx.gameObject.SetActive(webTargetActive && !disableWebTarget);
		if (!webTargetActive)
		{
			webTargetObject = null;
		}
	}

	private void SetWebTarget(RaycastHit hit)
	{
		webTargetObject = hit.transform.gameObject;
		webTarget.parent = webTargetObject.transform;
		if (hit.transform.GetComponentInParent<WebJoint>() != null)
		{
			webTarget.position = hit.transform.position;
			return;
		}
		if (hit.transform.GetComponentInParent<WebThread>() != null)
		{
			webTarget.position = hit.point - hit.normal * webColliderRadius;
			return;
		}
		if (webTargetObject != oldWebTargetObject1)
		{
			if (webTargetObject != null)
			{
				MovableObject component = webTargetObject.GetComponent<MovableObject>();
				if (component != null)
				{
					component.SetAsCurrentWebTarget(value: true);
				}
			}
			if (oldWebTargetObject1 != null)
			{
				MovableObject component2 = oldWebTargetObject1.GetComponent<MovableObject>();
				if (component2 != null)
				{
					component2.SetAsCurrentWebTarget(value: false);
				}
			}
		}
		oldWebTargetObject1 = webTargetObject;
		webTarget.position = hit.point;
	}

	private void CheckForWebThreadToDestroy()
	{
		Collider[] array = Physics.OverlapSphere(webTarget.position, webColliderRadius * 0.5f, whatIsWebJoint);
		if (array.Length == 0)
		{
			webJointToDestroy = null;
			array = Physics.OverlapSphere(webTarget.position, webColliderRadius * 0.5f, whatIsWeb);
			if (array.Length == 0)
			{
				webThreadToDestroy = null;
			}
			else
			{
				webThreadToDestroy = array[0].transform.parent.GetComponent<WebThread>();
			}
		}
		else
		{
			webThreadToDestroy = null;
			webJointToDestroy = array[0].transform.parent.GetComponent<WebJoint>();
		}
	}

	private void AttachAnchor()
	{
		if (webTargetActive && !(webTargetObject == null))
		{
			webMode = WebMode.Building;
			SetAnchorOnTargetPosition();
			ApplyWebModifiers(webAnchorObject);
			webAnchorActive = true;
			webAnchorGfx.gameObject.SetActive(WebAnchorActive);
			webAnchorGfx.GetComponent<MeshRenderer>().sharedMaterial = ((webBuildingMode == WebBuildingMode.MovingAnchor) ? webAnchorMovingAnchorMaterial : webAnchorFixedAnchorMaterial);
			webTargetGfx.GetComponent<MeshRenderer>().sharedMaterial = ((webBuildingMode == WebBuildingMode.MovingAnchor) ? webTargetMovingAnchorMaterial : webTargetFixedAnchorMaterial);
		}
	}

	private void ApplyWebModifiers(GameObject go)
	{
		WebModifier componentInParent = go.GetComponentInParent<WebModifier>();
		if (componentInParent == null)
		{
			ResetWebColor();
			ResetWebThickness();
			return;
		}
		if (componentInParent.HasColorModification(out var modifiedWebThread, out var modifiedColor))
		{
			ModifyWebColor(modifiedWebThread, modifiedColor);
		}
		if (componentInParent.HasThicknessModification(out var modifiedThickness))
		{
			ModifyWebThickness(modifiedThickness);
		}
	}

	private void SetAnchorOnTargetPosition()
	{
		webAnchorObject = webTargetObject;
		webAnchor.position = webTarget.position;
		webAnchor.parent = webTarget.parent;
		attachAnchorSound.Play();
	}

	private void BuildWeb()
	{
		if (!WebAnchorActive || webTargetObject == null)
		{
			return;
		}
		if (Vector3.Distance(webAnchor.position, webTarget.position) < minBuildDistance)
		{
			cantBuildSound.Play();
			return;
		}
		WebJoint componentInParent = webAnchorObject.transform.GetComponentInParent<WebJoint>();
		WebJoint componentInParent2 = webTargetObject.transform.GetComponentInParent<WebJoint>();
		WebThread componentInParent3 = webAnchorObject.transform.GetComponentInParent<WebThread>();
		WebThread componentInParent4 = webTargetObject.transform.GetComponentInParent<WebThread>();
		if (MoveAnchorOnInvalidWebBuild(componentInParent, componentInParent2, componentInParent4, componentInParent3))
		{
			return;
		}
		if (!CanBuildWeb(componentInParent, componentInParent2, componentInParent3, componentInParent4))
		{
			cantBuildSound.Play();
			return;
		}
		ApplyWebModifiers(webTarget.parent.gameObject);
		CreateNewWebThread(componentInParent, componentInParent3, webAnchorObject, webAnchor.position, out var newWebJointAnchor, componentInParent2, componentInParent4, webTargetObject, webTarget.position, out var newWebJointTarget, webColorIndex);
		if (componentInParent4 != null)
		{
			SplitWebThread(componentInParent4, newWebJointTarget);
		}
		switch (webBuildingMode)
		{
		case WebBuildingMode.FixedAnchor:
			webAnchorObject = newWebJointAnchor.gameObject;
			webAnchor.position = newWebJointAnchor.transform.position;
			webAnchor.parent = webAnchorObject.transform;
			break;
		case WebBuildingMode.MovingAnchor:
			webAnchorObject = newWebJointTarget.gameObject;
			webAnchor.position = newWebJointTarget.transform.position;
			webAnchor.parent = webAnchorObject.transform;
			break;
		}
		if (componentInParent3 != null)
		{
			SplitWebThread(componentInParent3, newWebJointAnchor);
		}
		Singleton<MusicController>.Instance.SetKitchenWebCount(webContainer.childCount);
		PlayBuildWebSound(webAnchor);
		webTarget.parent = base.transform;
		this.OnWebBuild?.Invoke();
	}

	private bool MoveAnchorOnInvalidWebBuild(WebJoint webJointAnchor, WebJoint webJointTarget, WebThread webThreadTarget, WebThread webThreadAnchor)
	{
		if (webBuildingMode == WebBuildingMode.MovingAnchor)
		{
			if (webJointAnchor != null)
			{
				if (webJointTarget != null && webJointAnchor.connectedWebJoints.Contains(webJointTarget))
				{
					SetAnchorOnTargetPosition();
					return true;
				}
				if (webThreadTarget != null && webJointAnchor.attachedWebThreads.Contains(webThreadTarget))
				{
					SetAnchorOnTargetPosition();
					return true;
				}
			}
			if (webThreadAnchor != null)
			{
				if (webJointTarget != null && webThreadAnchor.WebJoints.Contains(webJointTarget))
				{
					SetAnchorOnTargetPosition();
					return true;
				}
				if (webThreadTarget != null && webThreadAnchor == webThreadTarget)
				{
					SetAnchorOnTargetPosition();
					return true;
				}
			}
		}
		return false;
	}

	private bool CanBuildWeb(WebJoint webJointAnchor, WebJoint webJointTarget, WebThread webThreadAnchor, WebThread webThreadTarget)
	{
		if (webJointAnchor != null && webJointTarget != null && (webJointAnchor == webJointTarget || webJointAnchor.connectedWebJoints.Contains(webJointTarget)))
		{
			return false;
		}
		if (webThreadAnchor != null && webThreadTarget != null && webThreadAnchor == webThreadTarget)
		{
			return false;
		}
		if (webJointAnchor != null && webThreadTarget != null && webJointAnchor.attachedWebThreads.Contains(webThreadTarget))
		{
			return false;
		}
		if (webJointTarget != null && webThreadAnchor != null && webJointTarget.attachedWebThreads.Contains(webThreadAnchor))
		{
			return false;
		}
		return true;
	}

	private WebThread CreateNewWebThread(WebJoint webJointAnchor, WebThread webThreadAnchor, GameObject objectAnchor, Vector3 positionAnchor, out WebJoint newWebJointAnchor, WebJoint webJointTarget, WebThread webThreadTarget, GameObject objectTarget, Vector3 positionTarget, out WebJoint newWebJointTarget, int newWebIndex, bool isDefaultWeb = false, Color color = default(Color), WebThread webThread = null)
	{
		if (webJointAnchor == null)
		{
			newWebJointAnchor = UnityEngine.Object.Instantiate(webJointPrefab, webJointContainer);
			if (webThreadAnchor != null)
			{
				newWebJointAnchor.SetupWebJoint(null, positionAnchor);
			}
			else
			{
				newWebJointAnchor.SetupWebJoint(objectAnchor, positionAnchor);
			}
			webJointList.Add(newWebJointAnchor);
		}
		else
		{
			newWebJointAnchor = webJointAnchor;
		}
		if (webJointTarget == null)
		{
			newWebJointTarget = UnityEngine.Object.Instantiate(webJointPrefab, webJointContainer);
			if (webThreadTarget != null)
			{
				newWebJointTarget.SetupWebJoint(null, positionTarget);
			}
			else
			{
				newWebJointTarget.SetupWebJoint(objectTarget, positionTarget);
			}
			webJointList.Add(newWebJointTarget);
		}
		else
		{
			newWebJointTarget = webJointTarget;
		}
		Color newWebColor = ((color == default(Color)) ? webColor : color);
		return ConnectWebJoints(newWebJointAnchor, newWebJointTarget, newWebColor, !isDefaultWeb, webThread, newWebIndex);
	}

	private WebThread ConnectWebJoints(WebJoint webJoint1, WebJoint webJoint2, Color newWebColor, bool playAnimation = true, WebThread webThread = null, int newWebIndex = 0)
	{
		if (webJoint1 == null || webJoint2 == null)
		{
			return null;
		}
		if (webJoint1.Rb == null || webJoint2.Rb == null || webJoint1.Rb == webJoint2.Rb)
		{
			return null;
		}
		webJoint1.AddConnectedWebJoint(webJoint2);
		webJoint2.AddConnectedWebJoint(webJoint1);
		WebThread webThread2 = ((webThread == null) ? webThreadPrefab : webThread);
		int validWebIndex = Singleton<CosmeticItemsController>.Instance.GetValidWebIndex(newWebIndex);
		WebSo webSo = ((CosmeticItemWebSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(validWebIndex)).webSo;
		webThread2.SetWebSo((webThread == null) ? webSo : webThread.GetWebSo());
		WebThread webThread3 = UnityEngine.Object.Instantiate(webThread2, webContainer);
		webThread3.SetupWebThread(webJoint1, webJoint2, webThread2, validWebIndex, newWebColor, webColliderRadius, webThickness, playAnimation);
		if (webJoint1.attachedWebThreads != null && webJoint1.attachedWebThreads.Count > 0 && webJoint1.attachedWebThreads[0].gameObject.layer == LayerMask.NameToLayer("PlayerWeb"))
		{
			webThread3.SetLayer(LayerMask.NameToLayer("PlayerWeb"));
		}
		else if (webJoint2.attachedWebThreads != null && webJoint2.attachedWebThreads.Count > 0 && webJoint2.attachedWebThreads[0].gameObject.layer == LayerMask.NameToLayer("PlayerWeb"))
		{
			webThread3.SetLayer(LayerMask.NameToLayer("PlayerWeb"));
		}
		CreateSpringJoint(webJoint1, webJoint2);
		return webThread3;
	}

	private void SplitWebThread(WebThread webThreadToSplit, WebJoint newWebJoint)
	{
		WebJoint[] webJoints = webThreadToSplit.WebJoints;
		if (!(newWebJoint == null) && !(webJoints[0] == null) && !(webJoints[1] == null) && !(newWebJoint.Rb == null) && !(webJoints[0].Rb == null) && !(webJoints[1].Rb == null))
		{
			Color newWebColor = webThreadToSplit.WebColor;
			float num = webThickness;
			webThickness = webThreadToSplit.WebThickness;
			WebThread webThread = ConnectWebJoints(newWebJoint, webJoints[0], newWebColor, playAnimation: false, webThreadToSplit.WebThreadPrefab, webThreadToSplit.WebIndex);
			WebThread webThread2 = ConnectWebJoints(newWebJoint, webJoints[1], newWebColor, playAnimation: false, webThreadToSplit.WebThreadPrefab, webThreadToSplit.WebIndex);
			webThickness = num;
			webThread.SetLayer(webThread.WebJoints.Contains(playerWebJoint) ? LayerMask.NameToLayer("PlayerWeb") : LayerMask.NameToLayer("Web"));
			webThread2.SetLayer(webThread2.WebJoints.Contains(playerWebJoint) ? LayerMask.NameToLayer("PlayerWeb") : LayerMask.NameToLayer("Web"));
			DestroyPlayerSpringJoint(webThreadToSplit);
			webThreadToSplit.DeleteWebThread(destroyImmediate: false);
		}
	}

	private WebJoint SplitWebThread(WebThread webThreadToSplit, Vector3 position)
	{
		Color newWebColor = webThreadToSplit.WebColor;
		WebJoint[] webJoints = webThreadToSplit.WebJoints;
		WebJoint webJoint = UnityEngine.Object.Instantiate(webJointPrefab, position, Quaternion.identity, webJointContainer);
		webJoint.Rb.isKinematic = false;
		webJoint.SetAnchor(webJoint.transform);
		webJointList.Add(webJoint);
		WebThread webThread = ConnectWebJoints(webJoint, webJoints[0], newWebColor, playAnimation: false, webThreadToSplit.WebThreadPrefab, webThreadToSplit.WebIndex);
		WebThread webThread2 = ConnectWebJoints(webJoint, webJoints[1], newWebColor, playAnimation: false, webThreadToSplit.WebThreadPrefab, webThreadToSplit.WebIndex);
		int layer = webThreadToSplit.GetLayer();
		webThread.SetLayer(layer);
		webThread2.SetLayer(layer);
		DestroyPlayerSpringJoint(webThreadToSplit);
		webThreadToSplit.DeleteWebThread(destroyImmediate: false);
		return webJoint;
	}

	private void DestroyWebThread(bool useWebTargetPosition = true, bool playAnimation = true)
	{
		ActivateDefaultMode();
		if (webJointToDestroy == null && webThreadToDestroy == null)
		{
			return;
		}
		webTarget.parent = base.transform;
		if (webJointToDestroy != null)
		{
			for (int num = webJointToDestroy.attachedWebThreads.Count - 1; num >= 0; num--)
			{
				DestroyPlayerSpringJoint(webJointToDestroy.attachedWebThreads[num]);
				webJointToDestroy.attachedWebThreads[num].DeleteWebThread(destroyImmediate: true, useWebTargetPosition, playAnimation);
			}
		}
		else
		{
			DestroyPlayerSpringJoint(webThreadToDestroy);
			webThreadToDestroy.DeleteWebThread(destroyImmediate: true, useWebTargetPosition, playAnimation);
		}
		deleteThreadSound.Play();
		FixWebImmediate();
		webTargetGfx.gameObject.SetActive(value: false);
	}

	private void DestroyPlayerSpringJoint(WebThread webThread)
	{
		if (webThread == null || playerWebJoint == null || !webThread.WebJoints.Contains(playerWebJoint))
		{
			return;
		}
		SpringJoint[] componentsInParent = bodyMovement.GetComponentsInParent<SpringJoint>();
		foreach (SpringJoint springJoint in componentsInParent)
		{
			if (springJoint.connectedBody == webThread.WebJoints[0].Rb || springJoint.connectedBody == webThread.WebJoints[1].Rb)
			{
				UnityEngine.Object.Destroy(springJoint);
			}
		}
	}

	private void FixWebImmediate()
	{
		fixWebTimer = fixWebIntervalFast;
		foreach (Transform item in webJointContainer)
		{
			WebJoint component = item.GetComponent<WebJoint>();
			if (component.connectedWebJoints.Count == 0 && component.transform.name != webTarget.name)
			{
				component.DestroySafely();
				RemoveWebJointFromList(component);
			}
		}
	}

	private void FixWeb()
	{
		List<WebJoint> list = new List<WebJoint>();
		List<WebThread> list2 = new List<WebThread>();
		foreach (Transform item2 in webJointContainer)
		{
			WebJoint component = item2.GetComponent<WebJoint>();
			for (int num = component.connectedWebJoints.Count - 1; num >= 0; num--)
			{
				WebJoint webJoint = component.connectedWebJoints[num];
				if (webJoint == null)
				{
					component.connectedWebJoints.Remove(webJoint);
				}
			}
			for (int num2 = component.attachedWebThreads.Count - 1; num2 >= 0; num2--)
			{
				WebThread webThread = component.attachedWebThreads[num2];
				if (webThread == null)
				{
					component.attachedWebThreads.Remove(webThread);
				}
			}
			if (component.connectedWebJoints.Count == 0)
			{
				if (component.transform.name != webTarget.name)
				{
					list.Add(component);
				}
			}
			else if (component.connectedWebJoints.Count == 1 && !component.Rb.isKinematic && !component.HasFixedJoint)
			{
				for (int num3 = component.attachedWebThreads.Count - 1; num3 >= 0; num3--)
				{
					WebThread item = component.attachedWebThreads[num3];
					list2.Add(item);
				}
				if (component.transform.name != webTarget.name)
				{
					list.Add(component);
				}
			}
			else if (component.connectedWebJoints.Count == 2 && !component.Rb.isKinematic && !component.HasFixedJoint && !(component.Rb.linearVelocity.sqrMagnitude > 0.1f))
			{
				WebThread webThread2 = ((component.attachedWebThreads[0].Length > component.attachedWebThreads[1].Length) ? component.attachedWebThreads[0].WebThreadPrefab : component.attachedWebThreads[1].WebThreadPrefab);
				WebJoint[] array = component.connectedWebJoints.ToArray();
				ConnectWebJoints(array[0], array[1], webColor, playAnimation: false, webThread2, webThread2.WebIndex);
				for (int num4 = component.attachedWebThreads.Count - 1; num4 >= 0; num4--)
				{
					WebThread webThread3 = component.attachedWebThreads[num4];
					DestroyPlayerSpringJoint(webThread3);
					webThread3.DeleteWebThread(destroyImmediate: false);
				}
				if (component.transform.name != webTarget.name)
				{
					component.DestroySafely();
					RemoveWebJointFromList(component);
				}
			}
		}
		foreach (WebThread item3 in list2)
		{
			DestroyPlayerSpringJoint(item3);
			item3.DeleteWebThread(destroyImmediate: false, useWebTargetPosition: false, playAnimation: true);
		}
		foreach (WebJoint item4 in list)
		{
			item4.DestroySafely();
			RemoveWebJointFromList(item4);
		}
		fixWebTimer = ((list2.Count == 0 && list.Count == 0) ? fixWebIntervalSlow : fixWebIntervalFast);
		Singleton<MusicController>.Instance.SetKitchenWebCount(webContainer.childCount);
	}

	private void CreateSpringJoint(WebJoint originWebJoint, WebJoint targetWebJoint)
	{
		SpringJoint springJoint = originWebJoint.Rb.gameObject.AddComponent<SpringJoint>();
		springJoint.connectedBody = targetWebJoint.Rb;
		springJoint.anchor = Vector3.zero;
		springJoint.autoConfigureConnectedAnchor = false;
		springJoint.connectedAnchor = Vector3.zero;
		springJoint.damper = damperWeb;
		springJoint.enableCollision = true;
		float magnitude = (originWebJoint.Rb.transform.position - targetWebJoint.Rb.transform.position).magnitude;
		springJoint.spring = springWeb / magnitude;
		springJoint.minDistance = magnitude * minDistancePercentageWeb;
		springJoint.maxDistance = magnitude * maxDistancePercentageWeb;
		originWebJoint.springJoints.Add(springJoint);
	}

	private void QuickBuild()
	{
		if (webTargetObject == null)
		{
			return;
		}
		Ray ray = new Ray(bodyMovement.TargetTransform.position, Vector3.down);
		RaycastHit[] array = Physics.SphereCastAll(ray, raycastMaxRadius, 0f, whatIsWebJoint);
		if (array.Length != 0)
		{
			RaycastHit raycastHit = array.OrderBy((RaycastHit x) => Vector3.Distance(x.point, ray.origin)).First();
			webAnchorObject = raycastHit.transform.gameObject;
			webAnchor.position = raycastHit.transform.position;
		}
		else
		{
			webAnchorObject = bodyMovement.TargetTransform.parent.gameObject;
			webAnchor.position = bodyMovement.TargetTransform.position;
		}
		webAnchor.parent = webTargetObject.transform;
		ApplyWebModifiers(webTargetObject);
		bool flag = _Scripts.Utils.Utils.IsLayerInLayerMask(webAnchorObject.layer, (int)whatCanBeWebbed | (int)whatIsWeb | (int)whatIsWebJoint);
		if (Vector3.Distance(webAnchor.position, webTarget.position) < minBuildDistance || !flag)
		{
			cantBuildSound.Play();
			return;
		}
		WebJoint componentInParent = webAnchorObject.transform.GetComponentInParent<WebJoint>();
		WebJoint componentInParent2 = webTargetObject.transform.GetComponentInParent<WebJoint>();
		WebThread componentInParent3 = webAnchorObject.transform.GetComponentInParent<WebThread>();
		WebThread componentInParent4 = webTargetObject.transform.GetComponentInParent<WebThread>();
		if (MoveAnchorOnInvalidWebBuild(componentInParent, componentInParent2, componentInParent4, componentInParent3))
		{
			return;
		}
		if (!CanBuildWeb(componentInParent, componentInParent2, componentInParent3, componentInParent4))
		{
			cantBuildSound.Play();
			return;
		}
		CreateNewWebThread(componentInParent, componentInParent3, webAnchorObject, webAnchor.position, out var newWebJointAnchor, componentInParent2, componentInParent4, webTargetObject, webTarget.position, out var newWebJointTarget, webColorIndex);
		webTarget.parent = base.transform;
		if (componentInParent4 != null)
		{
			SplitWebThread(componentInParent4, newWebJointTarget);
		}
		if (componentInParent3 != null)
		{
			SplitWebThread(componentInParent3, newWebJointAnchor);
		}
		Singleton<MusicController>.Instance.SetKitchenWebCount(webContainer.childCount);
		PlayBuildWebSound(webAnchor);
		this.OnWebBuild?.Invoke();
		ActivateDefaultMode();
	}

	private void SetWebTargetColor()
	{
		if (!webAnchorActive)
		{
			MeshRenderer component = webTargetGfx.GetComponent<MeshRenderer>();
			int validWebIndex = Singleton<CosmeticItemsController>.Instance.GetValidWebIndex(webColorIndex);
			component.sharedMaterial = ((CosmeticItemWebSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(validWebIndex)).webSo.webTargetMaterial;
			component.GetPropertyBlock(mpb);
			mpb.SetColor(colorId, webColor);
			component.SetPropertyBlock(mpb);
		}
	}

	private void SetWebTargetColor(Color color)
	{
		if (!webAnchorActive)
		{
			MeshRenderer component = webTargetGfx.GetComponent<MeshRenderer>();
			component.GetPropertyBlock(mpb);
			mpb.SetColor(colorId, color);
			component.SetPropertyBlock(mpb);
		}
	}

	private void InitializedSavedWebJoints(List<WebJointSaveData> webJointSaveDataList)
	{
		generatedWebJointDict = new Dictionary<string, WebJoint>();
		foreach (WebJointSaveData webJointSaveData in webJointSaveDataList)
		{
			WebJoint webJoint = UnityEngine.Object.Instantiate(webJointPrefab, webJointContainer);
			if (webJointSaveData.isKinematic)
			{
				webJoint.SetupWebJoint(base.gameObject, webJointSaveData.position);
			}
			else if (webJointSaveData.hasFixedJoint)
			{
				LevelSavingController.TryGetUniqueGameObjectById(webJointSaveData.fixedJointConnectedBodyID, out var uniqueGameObject);
				webJoint.SetupWebJoint(uniqueGameObject, webJointSaveData.position);
			}
			else
			{
				webJoint.SetupWebJoint(null, webJointSaveData.position);
			}
			webJointList.Add(webJoint);
			generatedWebJointDict.TryAdd(webJointSaveData.id, webJoint);
		}
		foreach (WebJointSaveData webJointSaveData2 in webJointSaveDataList)
		{
			generatedWebJointDict.TryGetValue(webJointSaveData2.id, out var value);
			if (value == null)
			{
				continue;
			}
			foreach (SpringJointSaveData springJointSaveData in webJointSaveData2.springJointSaveDataList)
			{
				generatedWebJointDict.TryGetValue(springJointSaveData.connectedBodyID, out var value2);
				if (!(value2 == null) && !(value2 == value))
				{
					SpringJoint springJoint = value.gameObject.AddComponent<SpringJoint>();
					springJoint.connectedBody = value2.Rb;
					springJoint.anchor = Vector3.zero;
					springJoint.autoConfigureConnectedAnchor = false;
					springJoint.connectedAnchor = Vector3.zero;
					springJoint.damper = springJointSaveData.damper;
					springJoint.enableCollision = true;
					springJoint.spring = springJointSaveData.spring;
					springJoint.minDistance = springJointSaveData.minDistance;
					springJoint.maxDistance = springJointSaveData.maxDistance;
					value.springJoints.Add(springJoint);
				}
			}
		}
	}

	private void InitializeWebThreads(List<WebJointSaveData> webJointSaveDataList, List<WebThreadSaveData> webThreadSaveDataList)
	{
		foreach (WebThreadSaveData webThreadSaveData in webThreadSaveDataList)
		{
			WebJoint[] array = new WebJoint[2];
			generatedWebJointDict.TryGetValue(webThreadSaveData.webJoints[0], out array[0]);
			generatedWebJointDict.TryGetValue(webThreadSaveData.webJoints[1], out array[1]);
			if (array[0] == null)
			{
				Debug.Log("No WebJoint found for Unique ID " + webThreadSaveData.webJoints[0] + ". It was most likely a web connected to the player. It is skipped for now.");
				continue;
			}
			if (array[1] == null)
			{
				Debug.Log("No WebJoint found for Unique ID " + webThreadSaveData.webJoints[1] + ". It was most likely a web connected to the player. It is skipped for now.");
				continue;
			}
			Color newWebColor = webThreadSaveData.webColor;
			array[0].AddConnectedWebJoint(array[1]);
			array[1].AddConnectedWebJoint(array[0]);
			int validWebIndex = Singleton<CosmeticItemsController>.Instance.GetValidWebIndex(webThreadSaveData.webIndex);
			WebSo webSo = ((CosmeticItemWebSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(validWebIndex)).webSo;
			WebThread webThread = webSo.webThread;
			webThread.SetWebSo(webSo);
			UnityEngine.Object.Instantiate(webThread, webContainer).SetupWebThread(array[0], array[1], webThread, validWebIndex, newWebColor, webColliderRadius, webThreadSaveData.webThickness, playAnimation: false);
		}
	}

	private void ApplyAncientPotionEffect(AncientPotionEffectSo ancientPotionEffectSo)
	{
		switch (ancientPotionEffectSo.effectType)
		{
		case AncientPotionEffectType.LongWeb:
			SetWebDistance(defaultWebDistance * ancientPotionWebDistanceMultiplier);
			break;
		case AncientPotionEffectType.HueWeb:
			ancientPotionHueWebActive = true;
			ancientPotionHue = 0f;
			SetWebColor(Singleton<CosmeticItemsController>.Instance.GetDefaultWebIndex());
			OnWebBuild += OnWebBuild_UpdateHueWebColor;
			OnMainWebActivated += OnMainWebActivated_UpdateHueWebColor;
			break;
		case AncientPotionEffectType.SuperJump:
		case AncientPotionEffectType.SuperSpeed:
		case AncientPotionEffectType.NoGravity:
		case AncientPotionEffectType.RubberDuck:
		case AncientPotionEffectType.ColorFilter:
			break;
		}
	}

	private void RemoveAncientPotionEffect(AncientPotionEffectSo ancientPotionEffectSo)
	{
		switch (ancientPotionEffectSo.effectType)
		{
		case AncientPotionEffectType.LongWeb:
			SetWebDistance(defaultWebDistance);
			break;
		case AncientPotionEffectType.HueWeb:
			ancientPotionHueWebActive = false;
			OnWebBuild -= OnWebBuild_UpdateHueWebColor;
			OnMainWebActivated -= OnMainWebActivated_UpdateHueWebColor;
			break;
		case AncientPotionEffectType.SuperJump:
		case AncientPotionEffectType.SuperSpeed:
		case AncientPotionEffectType.NoGravity:
		case AncientPotionEffectType.RubberDuck:
		case AncientPotionEffectType.ColorFilter:
			break;
		}
	}

	private void UpdateHueWebColor()
	{
		Color modifiedColor = Color.HSVToRGB(ancientPotionHue, 0.6f, 0.85f);
		ModifyWebColor(webThreadPrefabNormal, modifiedColor);
		ancientPotionHue = (ancientPotionHue + hueInterval) % 1f;
	}

	public void SetWebThread(WebThread newWebThreadPrefab)
	{
		webThreadPrefab = newWebThreadPrefab;
	}

	public void SetWebType(WebType webType)
	{
		switch (webType)
		{
		case WebType.Normal:
			SetWebThread(webThreadPrefabNormal);
			SetWebColorPalette(webColorPalettes[0]);
			damperWeb = damperWebNormal;
			springWeb = springWebNormal;
			minDistancePercentageWeb = minDistancePercentageWebNormal;
			maxDistancePercentageWeb = maxDistancePercentageWebNormal;
			break;
		case WebType.Strong:
			damperWeb = damperWebStrong;
			springWeb = springWebStrong;
			minDistancePercentageWeb = minDistancePercentageWebStrong;
			maxDistancePercentageWeb = maxDistancePercentageWebStrong;
			break;
		case WebType.RGB:
			SetWebThread(webThreadPrefabRGB);
			break;
		case WebType.ColorPalette2:
			SetWebThread(webThreadPrefabNormal);
			SetWebColorPalette(webColorPalettes[1]);
			break;
		case WebType.ColorPalette3:
			SetWebThread(webThreadPrefabNormal);
			SetWebColorPalette(webColorPalettes[2]);
			break;
		default:
			throw new ArgumentOutOfRangeException("webType", webType, null);
		case WebType.Rigid:
		case WebType.Music:
			break;
		}
	}

	public void ShowWebTarget(bool value)
	{
		webTargetGfx.gameObject.SetActive(value);
	}

	public void ActivateDefaultMode()
	{
		webIndicationLineRenderer.enabled = false;
		webMode = WebMode.Default;
		ReleaseAnchor();
		ResetWebColor();
		Singleton<WebController>.Instance.OnModeChanged?.Invoke(this, EventArgs.Empty);
	}

	public void Respawn()
	{
		shootWebPressed = false;
		ReleaseWeb();
		ReleaseAnchor();
		ReleaseTarget();
		ActivateDefaultMode();
		DeactivateSpringJoint();
	}

	private void ResetWebThickness()
	{
		webThickness = defaultWebThickness;
	}

	private void ModifyWebThickness(float modifiedThickness)
	{
		webThickness = modifiedThickness;
	}

	private void ResetWebColor()
	{
		if (!ancientPotionHueWebActive)
		{
			SetWebColor(webColorIndex);
		}
	}

	private void ModifyWebColor(WebThread modifiedWebThread, Color modifiedColor)
	{
		webThreadPrefab = modifiedWebThread;
		webColor = modifiedColor;
		SetWebTargetColor(modifiedColor);
		this.OnWebColorChanged?.Invoke(this, new OnWebColorChangedEventArgs
		{
			webColor = modifiedColor,
			cosmeticItemWebSo = null
		});
		webIndicationLineRenderer.startColor = modifiedColor;
		webIndicationLineRenderer.endColor = modifiedColor;
	}

	public void SetWebColor(int colorIndex)
	{
		webColorIndex = colorIndex;
		CosmeticItemWebSo cosmeticItemWebSo = (CosmeticItemWebSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(webColorIndex);
		webColor = cosmeticItemWebSo.webSo.webColor;
		webThreadPrefab = cosmeticItemWebSo.webSo.webThread;
		webSound = cosmeticItemWebSo.webSo.webSound;
		SetWebTargetColor();
		this.OnWebColorChanged?.Invoke(this, new OnWebColorChangedEventArgs
		{
			webColor = webColor,
			cosmeticItemWebSo = cosmeticItemWebSo
		});
		webIndicationLineRenderer.sharedMaterial = cosmeticItemWebSo.webSo.webIndicatorMaterial;
		webIndicationLineRenderer.startColor = webColor;
		webIndicationLineRenderer.endColor = webColor;
		foreach (Transform item in wardrobeWebContainer)
		{
			WebThread component = item.GetComponent<WebThread>();
			if (component != null)
			{
				component.SetColor(webColor);
			}
		}
	}

	public void RecalculateSpringJoint()
	{
		if (webActive)
		{
			DeactivateSpringJoint(deactivateMainWeb: false);
			ActivateSpringJoint();
		}
	}

	public static void StartDialogue()
	{
		Singleton<WebController>.Instance.ReleaseWeb();
		Singleton<WebController>.Instance.ActivateDefaultMode();
		Singleton<WebController>.Instance.ReleaseTarget();
		Singleton<WebController>.Instance.ShowWebTarget(value: false);
	}

	public static void EndDialogue()
	{
		Singleton<WebController>.Instance.ShowWebTarget(value: true);
	}

	public void SetWardrobeWebActive(bool value)
	{
		wardrobeWebContainer.gameObject.SetActive(value);
		wardrobeJointContainer.gameObject.SetActive(value);
	}

	public void EnableAiming(bool value)
	{
		Singleton<WebController>.Instance.canAim = value;
	}

	public void EnableWebShooting(bool value)
	{
		Singleton<WebController>.Instance.canShootWebs = value;
		if (value)
		{
			Singleton<WebController>.Instance.canAim = true;
		}
	}

	public void EnableWebBuilding(bool value)
	{
		Singleton<WebController>.Instance.canBuildWebs = value;
		if (value)
		{
			Singleton<WebController>.Instance.canAim = true;
		}
	}

	public void EnableWebDeletion(bool value)
	{
		Singleton<WebController>.Instance.canDeleteWebs = value;
		if (value)
		{
			Singleton<WebController>.Instance.canAim = true;
		}
	}

	public void EnableQuickBuild(bool value)
	{
		Singleton<WebController>.Instance.canQuickBuild = value;
		if (value)
		{
			Singleton<WebController>.Instance.canAim = true;
		}
	}

	public void SetWebDistance(float value)
	{
		Singleton<WebController>.Instance.webDistance = value;
	}

	public void DestroyWebJoint(WebJoint webJoint, bool playAnimation = false)
	{
		webJointToDestroy = webJoint;
		DestroyWebThread(useWebTargetPosition: false, playAnimation);
	}

	public void DestroyWebThread(WebThread webThread, bool playAnimation = false)
	{
		webThreadToDestroy = webThread;
		DestroyWebThread(useWebTargetPosition: false, playAnimation);
	}

	public void ToggleWebTargetActive()
	{
		disableWebTarget = !disableWebTarget;
	}

	public void MobileQuickBuild()
	{
		if (!canQuickBuild || Singleton<GameController>.Instance.State != 0 || Singleton<EmoteController>.Instance.OpenEmoteWheel != null || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			return;
		}
		if (webActive && springJoint != null)
		{
			if (canBuildWebs)
			{
				AttachWebToPlayer();
				ReleaseWeb(playAnimation: false);
			}
		}
		else if (bodyMovement.State == BodyMovement.MovementState.Walking && !webActive)
		{
			switch (webMode)
			{
			case WebMode.Default:
				QuickBuild();
				break;
			case WebMode.Building:
				BuildWeb();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public void MobileFixedAnchor()
	{
		if (canBuildWebs && !webActive && Singleton<GameController>.Instance.State == GameController.GameState.Running && bodyMovement.State != BodyMovement.MovementState.Jumping && !Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			webBuildingMode = WebBuildingMode.FixedAnchor;
			webAnchorGfx.GetComponent<MeshRenderer>().sharedMaterial = webAnchorFixedAnchorMaterial;
			webTargetGfx.GetComponent<MeshRenderer>().sharedMaterial = (WebAnchorActive ? webTargetFixedAnchorMaterial : webTargetDefaultMaterial);
			SetWebTargetColor();
			AttachAnchor();
			Singleton<WebController>.Instance.OnModeChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public void MobileMovingAnchor()
	{
		if (canBuildWebs && !webActive && Singleton<GameController>.Instance.State == GameController.GameState.Running && bodyMovement.State != BodyMovement.MovementState.Jumping && !Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			webBuildingMode = WebBuildingMode.MovingAnchor;
			webAnchorGfx.GetComponent<MeshRenderer>().sharedMaterial = webAnchorMovingAnchorMaterial;
			webTargetGfx.GetComponent<MeshRenderer>().sharedMaterial = (WebAnchorActive ? webTargetMovingAnchorMaterial : webTargetDefaultMaterial);
			SetWebTargetColor();
			AttachAnchor();
			Singleton<WebController>.Instance.OnModeChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public void MobileToggleWeb()
	{
		if (!canShootWebs || Singleton<GameController>.Instance.State != 0 || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			return;
		}
		if (!webActive)
		{
			if (Singleton<EmoteController>.Instance.OpenEmoteWheel == null && bodyMovement.State != BodyMovement.MovementState.ResetLegs && bodyMovement.State != BodyMovement.MovementState.Emote)
			{
				switch (webMode)
				{
				case WebMode.Default:
					AttachWeb();
					break;
				case WebMode.Building:
					BuildWeb();
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		else if (webActive)
		{
			ReleaseWeb();
		}
	}

	public void MobileShootWeb(bool value)
	{
		if (!canShootWebs || Singleton<GameController>.Instance.State != 0 || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			return;
		}
		if (!webActive && value)
		{
			if (Singleton<EmoteController>.Instance.OpenEmoteWheel == null && bodyMovement.State != BodyMovement.MovementState.ResetLegs && bodyMovement.State != BodyMovement.MovementState.Emote)
			{
				switch (webMode)
				{
				case WebMode.Default:
					AttachWeb();
					break;
				case WebMode.Building:
					BuildWeb();
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		else if (webActive && !value)
		{
			ReleaseWeb();
		}
	}

	public void MobileDeleteWeb()
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Running && !webActive && !Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			switch (webMode)
			{
			case WebMode.Default:
				CheckForWebThreadToDestroy();
				DestroyWebThread();
				deleteWebPressed = true;
				deletePlayerWebsTimer = deletePlayerWebsHoldDuration;
				break;
			case WebMode.Building:
				ActivateDefaultMode();
				break;
			}
		}
	}

	public void MobileDeleteWebButtonReleased()
	{
		deleteWebPressed = false;
	}

	public void SetWebColorPalette(WebColorPaletteSO colorPalette)
	{
		if (!(colorPalette == null))
		{
			SetWebColor(webColorIndex);
		}
	}

	public void DestroyAllPlayerWebs(bool playAnimation = false)
	{
		List<WebThread> list = new List<WebThread>();
		foreach (Transform item in webContainer)
		{
			WebThread component = item.GetComponent<WebThread>();
			if (component.GetLayer() == LayerMask.NameToLayer("PlayerWeb"))
			{
				list.Add(component);
			}
		}
		foreach (WebThread item2 in list)
		{
			DestroyPlayerSpringJoint(item2);
			item2.DeleteWebThread(destroyImmediate: true, useWebTargetPosition: false, playAnimation);
		}
		if (list.Count > 0)
		{
			deleteThreadSound.Play();
		}
	}

	public List<WebJointSaveData> GetWebJointSaveDataList()
	{
		List<WebJointSaveData> list = new List<WebJointSaveData>();
		foreach (Transform item3 in webJointContainer)
		{
			WebJoint component = item3.GetComponent<WebJoint>();
			if (component == null)
			{
				continue;
			}
			WebJointSaveData webJointSaveData = default(WebJointSaveData);
			webJointSaveData.position = component.transform.position;
			webJointSaveData.isKinematic = component.Rb.isKinematic;
			webJointSaveData.hasFixedJoint = component.HasFixedJoint;
			webJointSaveData.connectedWebJointIDs = new List<string>();
			webJointSaveData.attachedWebThreadIDs = new List<string>();
			webJointSaveData.springJointSaveDataList = new List<SpringJointSaveData>();
			WebJointSaveData item = webJointSaveData;
			UniqueID component2 = component.GetComponent<UniqueID>();
			if (component2 != null)
			{
				item.id = component2.ID;
			}
			if (component.HasFixedJoint)
			{
				UniqueID component3 = component.GetFixedJoint().connectedBody.GetComponent<UniqueID>();
				if (component3 != null)
				{
					item.fixedJointConnectedBodyID = component3.ID;
				}
			}
			foreach (WebJoint connectedWebJoint in component.connectedWebJoints)
			{
				UniqueID component4 = connectedWebJoint.GetComponent<UniqueID>();
				if (component4 != null)
				{
					item.connectedWebJointIDs.Add(component4.ID);
				}
			}
			foreach (WebThread attachedWebThread in component.attachedWebThreads)
			{
				UniqueID component5 = attachedWebThread.GetComponent<UniqueID>();
				if (component5 != null)
				{
					item.attachedWebThreadIDs.Add(component5.ID);
				}
			}
			foreach (SpringJoint springJoint in component.springJoints)
			{
				SpringJointSaveData springJointSaveData = default(SpringJointSaveData);
				springJointSaveData.damper = springJoint.damper;
				springJointSaveData.spring = springJoint.spring;
				springJointSaveData.minDistance = springJoint.minDistance;
				springJointSaveData.maxDistance = springJoint.maxDistance;
				SpringJointSaveData item2 = springJointSaveData;
				UniqueID component6 = springJoint.connectedBody.GetComponent<UniqueID>();
				if (component6 != null)
				{
					item2.connectedBodyID = component6.ID;
				}
				item.springJointSaveDataList.Add(item2);
			}
			list.Add(item);
		}
		return list;
	}

	public List<WebThreadSaveData> GetWebThreadSaveDataList()
	{
		List<WebThreadSaveData> list = new List<WebThreadSaveData>();
		foreach (Transform item2 in webContainer)
		{
			WebThread component = item2.GetComponent<WebThread>();
			if (!(component == null))
			{
				WebThreadSaveData webThreadSaveData = default(WebThreadSaveData);
				webThreadSaveData.webColor = component.WebColor;
				webThreadSaveData.webThickness = component.WebThickness;
				webThreadSaveData.webIndex = component.WebIndex;
				WebThreadSaveData item = webThreadSaveData;
				UniqueID[] array = new UniqueID[2]
				{
					component.WebJoints[0].GetComponent<UniqueID>(),
					component.WebJoints[1].GetComponent<UniqueID>()
				};
				if (array[0] != null && array[1] != null)
				{
					item.webJoints = new string[2]
					{
						array[0].ID,
						array[1].ID
					};
				}
				UniqueID component2 = component.GetComponent<UniqueID>();
				if (component2 != null)
				{
					item.id = component2.ID;
				}
				list.Add(item);
			}
		}
		return list;
	}

	public void InitializeSavedWebs(List<WebJointSaveData> webJointSaveDataList, List<WebThreadSaveData> webThreadSaveDataList)
	{
		InitializedSavedWebJoints(webJointSaveDataList);
		InitializeWebThreads(webJointSaveDataList, webThreadSaveDataList);
	}

	public bool IsWebAttached(Rigidbody rigidbodyToCheck)
	{
		foreach (WebJoint webJoint in webJointList)
		{
			if (webJoint == null)
			{
				continue;
			}
			foreach (SpringJoint springJoint in webJoint.springJoints)
			{
				_ = springJoint;
				if (this.springJoint.connectedBody == rigidbodyToCheck)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void CatchFly(WebThread webThreadToSplit, WebJoint webJointFly)
	{
		SplitWebThread(webThreadToSplit, webJointFly);
	}

	private void RemoveWebJointFromList(WebJoint webJointToRemove)
	{
		webJointList.Remove(webJointToRemove);
	}

	private void OnShootWeb(InputAction.CallbackContext ctx)
	{
		if (!canShootWebs || Singleton<GameController>.Instance.State != 0 || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			return;
		}
		Singleton<MobileControls>.Instance.ShowButtons(value: false);
		bool flag = ctx.ReadValue<float>() >= 0.5f;
		bool flag2 = ctx.ReadValue<float>() <= 0.2f;
		switch (SettingsController.WebShootMode)
		{
		case HoldOrToggle.Hold:
			if (!webActive && flag && !shootWebPressed)
			{
				if (Singleton<EmoteController>.Instance.OpenEmoteWheel == null && bodyMovement.State != BodyMovement.MovementState.ResetLegs && bodyMovement.State != BodyMovement.MovementState.Emote)
				{
					shootWebPressed = true;
					switch (webMode)
					{
					case WebMode.Default:
						AttachWeb();
						break;
					case WebMode.Building:
						BuildWeb();
						break;
					}
				}
			}
			else if (flag2)
			{
				shootWebPressed = false;
				if (webActive)
				{
					ReleaseWeb();
				}
			}
			break;
		case HoldOrToggle.Toggle:
			if (flag && !shootWebPressed)
			{
				if (!webActive)
				{
					if (Singleton<EmoteController>.Instance.OpenEmoteWheel == null && bodyMovement.State != BodyMovement.MovementState.ResetLegs && bodyMovement.State != BodyMovement.MovementState.Emote)
					{
						shootWebPressed = true;
						switch (webMode)
						{
						case WebMode.Default:
							AttachWeb();
							break;
						case WebMode.Building:
							BuildWeb();
							break;
						default:
							throw new ArgumentOutOfRangeException();
						}
					}
				}
				else if (webActive)
				{
					ReleaseWeb();
				}
			}
			else if (flag2)
			{
				shootWebPressed = false;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void OnAttachFixedAnchor(InputAction.CallbackContext ctx)
	{
		if (canBuildWebs && !webActive && Singleton<GameController>.Instance.State == GameController.GameState.Running && !Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			Singleton<MobileControls>.Instance.ShowButtons(value: false);
			webBuildingMode = WebBuildingMode.FixedAnchor;
			webAnchorGfx.GetComponent<MeshRenderer>().sharedMaterial = webAnchorFixedAnchorMaterial;
			webTargetGfx.GetComponent<MeshRenderer>().sharedMaterial = (WebAnchorActive ? webTargetFixedAnchorMaterial : webTargetDefaultMaterial);
			SetWebTargetColor();
			AttachAnchor();
			Singleton<WebController>.Instance.OnModeChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	private void OnAttachMovingAnchor(InputAction.CallbackContext ctx)
	{
		if (canBuildWebs && !webActive && Singleton<GameController>.Instance.State == GameController.GameState.Running && !Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			Singleton<MobileControls>.Instance.ShowButtons(value: false);
			webBuildingMode = WebBuildingMode.MovingAnchor;
			webAnchorGfx.GetComponent<MeshRenderer>().sharedMaterial = webAnchorMovingAnchorMaterial;
			webTargetGfx.GetComponent<MeshRenderer>().sharedMaterial = (WebAnchorActive ? webTargetMovingAnchorMaterial : webTargetDefaultMaterial);
			SetWebTargetColor();
			AttachAnchor();
			Singleton<WebController>.Instance.OnModeChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	private void OnDeleteWeb(InputAction.CallbackContext ctx)
	{
		if (!canDeleteWebs || Singleton<GameController>.Instance.State != 0 || webActive || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			return;
		}
		Singleton<MobileControls>.Instance.ShowButtons(value: false);
		deleteWebPressed = ctx.ReadValueAsButton();
		if (deleteWebPressed)
		{
			deletePlayerWebsTimer = deletePlayerWebsHoldDuration;
			switch (webMode)
			{
			case WebMode.Default:
				CheckForWebThreadToDestroy();
				DestroyWebThread();
				break;
			case WebMode.Building:
				ActivateDefaultMode();
				break;
			}
		}
	}

	private void OnQuickBuild(InputAction.CallbackContext ctx)
	{
		if (!canQuickBuild || Singleton<GameController>.Instance.State != 0 || Singleton<EmoteController>.Instance.OpenEmoteWheel != null || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			return;
		}
		Singleton<MobileControls>.Instance.ShowButtons(value: false);
		if (webActive && springJoint != null)
		{
			if (canBuildWebs)
			{
				AttachWebToPlayer();
				ReleaseWeb(playAnimation: false);
			}
		}
		else
		{
			if (webActive || !(ctx.ReadValue<float>() > 0.5f))
			{
				return;
			}
			switch (webMode)
			{
			case WebMode.Default:
				if (bodyMovement.State == BodyMovement.MovementState.Walking)
				{
					QuickBuild();
				}
				break;
			case WebMode.Building:
				ActivateDefaultMode();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void AttachWebToPlayer()
	{
		WebJoint webJoint = null;
		WebThread webThread = null;
		if (webTargetObject != null)
		{
			webJoint = webTargetObject.transform.GetComponentInParent<WebJoint>();
			webThread = webTargetObject.transform.GetComponentInParent<WebThread>();
		}
		if (webJoint != null && (webJoint.attachedWebThreads.Count == 0 || webJoint.attachedWebThreads[0].gameObject.layer == LayerMask.NameToLayer("PlayerWeb")))
		{
			return;
		}
		if (webThread != null)
		{
			webJoint = SplitWebThread(webThread, webTarget.position);
			webTargetObject = webJoint.gameObject;
			webTarget.parent = webTargetObject.transform;
			webActive = true;
			webTargetCollider.enabled = true;
		}
		if (webJoint == null)
		{
			webJoint = UnityEngine.Object.Instantiate(webJointPrefab, webJointContainer);
			if (webThread != null)
			{
				webJoint.SetupWebJoint(null, webTarget.position);
			}
			else
			{
				webJoint.SetupWebJoint(webTargetObject, webTarget.position);
			}
			webJointList.Add(webJoint);
		}
		playerWebJoint.AddConnectedWebJoint(webJoint);
		webJoint.AddConnectedWebJoint(playerWebJoint);
		WebThread webThread2 = webThreadPrefab;
		webThread2.SetWebSo(((CosmeticItemWebSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(webColorIndex)).webSo);
		WebThread webThread3 = UnityEngine.Object.Instantiate(webThread2, webContainer);
		webThread3.SetupWebThread(playerWebJoint, webJoint, webThread2, webColorIndex, webColor, webColliderRadius, webThickness, playAnimation: false);
		webThread3.SetLayer(LayerMask.NameToLayer("PlayerWeb"));
		SpringJoint obj = playerWebJoint.gameObject.AddComponent<SpringJoint>();
		obj.connectedBody = webJoint.Rb;
		obj.anchor = Vector3.zero;
		obj.autoConfigureConnectedAnchor = false;
		obj.connectedAnchor = Vector3.zero;
		obj.spring = springMovableObject;
		obj.damper = damperMovableObject;
		obj.enableCollision = true;
		obj.minDistance = springJoint.minDistance;
		obj.maxDistance = springJoint.maxDistance;
		webTarget.parent = base.transform;
		Singleton<MusicController>.Instance.SetKitchenWebCount(webContainer.childCount);
		attachToPlayerSound.Play();
	}

	private void GameController_OnPauseGame(object sender, EventArgs e)
	{
		if (SettingsController.WebShootMode == HoldOrToggle.Hold)
		{
			shootWebPressed = false;
			ReleaseWeb();
		}
	}

	private void AncientPotionController_OnEffectStarted(AncientPotionEffectSo ancientPotionEffectSo)
	{
		ApplyAncientPotionEffect(ancientPotionEffectSo);
	}

	private void AncientPotionController_OnEffectEnded(AncientPotionEffectSo ancientPotionEffectSo)
	{
		RemoveAncientPotionEffect(ancientPotionEffectSo);
	}

	private void OnWebBuild_UpdateHueWebColor()
	{
		UpdateHueWebColor();
	}

	private void OnMainWebActivated_UpdateHueWebColor(object sender, OnMainWebActivatedEventArgs e)
	{
		UpdateHueWebColor();
	}
}
