using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using _Scripts.Achievements;
using _Scripts.Emotes;
using _Scripts.Environment;
using _Scripts.Game;
using _Scripts.General;
using _Scripts.LivingRoom;
using _Scripts.NPCs;
using _Scripts.Objects;
using _Scripts.Singletons;
using _Scripts.UI.MobileMonetization;
using _Scripts.Utils;

namespace _Scripts.Spider;

[SelectionBase]
public class BodyMovement : MonoBehaviour
{
	public class OnScaleChangedEventArgs : EventArgs
	{
		public float scale;
	}

	public class OnUnderwaterChangedEventArgs : EventArgs
	{
		public bool isUnderwater;
	}

	public enum MovementState
	{
		Walking,
		Jumping,
		ResetLegs,
		Emote
	}

	public enum PlayerInteraction
	{
		None,
		Follow,
		LookAt
	}

	private enum LookAtTarget
	{
		Player,
		TargetTransform
	}

	private enum RenderStyle
	{
		Spider,
		Ball
	}

	private enum CameraAngle
	{
		High,
		Middle,
		Low
	}

	public Action OnJumpInitialized;

	public Action OnLandingPerformed;

	[SerializeField]
	private LayerMask whatIsGroundDefault;

	[SerializeField]
	private LayerMask whatIsGroundWaterWalking;

	[SerializeField]
	private LayerMask climbableLayerMask;

	[SerializeField]
	private RayConfigurationFan[] rayConfigurationFans;

	[SerializeField]
	private RayConfigurationFan[] rayConfigurationFansMobile;

	[SerializeField]
	private bool isPlayer;

	[Header("NPC settings")]
	[SerializeField]
	private bool isWardrobeSpider;

	[Header("NPC settings")]
	[SerializeField]
	private PlayerInteraction playerInteraction;

	[SerializeField]
	private float minFollowDistance;

	[SerializeField]
	private float waitForMeActivateDistance;

	[SerializeField]
	private float waitForMeDeactivateDistance;

	[SerializeField]
	private float lookAtDistance;

	[SerializeField]
	private LookAtTarget lookAtTarget;

	[SerializeField]
	private Transform lookAtTargetTransform;

	[Header("References")]
	[SerializeField]
	private Transform root;

	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private MasterLegController masterLegController;

	[SerializeField]
	private SpiderEmotes emotes;

	[SerializeField]
	private SpiderCustomization spiderCustomization;

	[SerializeField]
	private Transform moveTarget;

	[SerializeField]
	private Transform targetTransform;

	[SerializeField]
	private Transform ballScaler;

	[SerializeField]
	private Transform ball;

	[SerializeField]
	private ParticleSystem underwaterAirBubbleParticles;

	[Header("Movement")]
	[SerializeField]
	private float movementSpeed;

	[SerializeField]
	private float movementBoostFactor;

	[SerializeField]
	private float movementUnderwaterFactor = 0.5f;

	[SerializeField]
	private float movementStopTime;

	[SerializeField]
	private float verticalSpeed;

	[SerializeField]
	private float rootVerticalOffsetDefault;

	[SerializeField]
	private float rootVerticalOffsetWeb;

	[SerializeField]
	private float verticalSmoothness;

	[SerializeField]
	private float idleEmoteTime;

	[SerializeField]
	private float moveTargetDistance = 1f;

	[SerializeField]
	private int inputMappingType = 1;

	[Header("Rotation")]
	[SerializeField]
	private float rotationSpeed;

	[SerializeField]
	private float rotationSmoothness;

	[SerializeField]
	private float angelDiffThreshold;

	[SerializeField]
	private float angleDiffFactor;

	[SerializeField]
	private float lookAtFactor = 2f;

	[SerializeField]
	private float lookAtTolerance = 1f;

	[Header("Jumping")]
	[SerializeField]
	private float forceMagnitude;

	[SerializeField]
	private float jumpAngle;

	[SerializeField]
	private float jumpAngleCharacterBased;

	[SerializeField]
	private float aerialControlSpeedForwardBackwards;

	[SerializeField]
	private float aerialControlSpeedLeftRight;

	[SerializeField]
	private float aerialDecelerationThreshold;

	[SerializeField]
	private float aerialAccelerationThreshold;

	[SerializeField]
	private float jumpDelay;

	[SerializeField]
	private float landingTime;

	[SerializeField]
	private float aimJumpUpwardsFactor;

	[SerializeField]
	private float jumpCheckOffset;

	[SerializeField]
	private float jumpCheckRadius;

	[SerializeField]
	private float jumpCheckDistance;

	[SerializeField]
	private float landingTriggerOffset = 0.5f;

	[SerializeField]
	private float landingTriggerRadius = 1f;

	[SerializeField]
	private float landingTriggerOffsetArachnophobia = 0.5f;

	[SerializeField]
	private float landingTriggerRadiusArachnophobia = 1.25f;

	[SerializeField]
	private float jumpingRotationSmoothness;

	[SerializeField]
	private float landingRotationSmoothness;

	[SerializeField]
	private float pitchAngle;

	[SerializeField]
	private float bounceMinimumVelocity = 10f;

	[SerializeField]
	private float swingBoostForceMagnitude = 1000f;

	[SerializeField]
	private GameObject swingBoostEffectPrefab;

	[Header("Others")]
	[SerializeField]
	private float respawnInterval;

	[SerializeField]
	private float minRespawnHeight;

	[SerializeField]
	private float minScale = 0.5f;

	[SerializeField]
	private float maxScale = 1.666f;

	[SerializeField]
	private float underwaterDrag = 0.5f;

	[SerializeField]
	private float underwaterAngularDrag = 0.5f;

	[SerializeField]
	private float switchToBallDistance = 220f;

	[SerializeField]
	private float switchToSpiderDistance = 180f;

	[Header("Ancient Potion Effects")]
	[SerializeField]
	private float ancientPotionJumpForceMultiplier = 2f;

	[SerializeField]
	private float ancientPotionSpeedMultiplier = 2f;

	[SerializeField]
	private GameObject ancientPotionRubberDuck;

	[Header("Physics")]
	[SerializeField]
	private float landingForceMultiplier = 1f;

	[SerializeField]
	private float opposingJumpingForceMultiplier = 1f;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference moveInputAction;

	[SerializeField]
	private InputActionReference jumpInputAction;

	[SerializeField]
	private InputActionReference sprintInputAction;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter jumpSound;

	[SerializeField]
	private StudioEventEmitter jumpNotPossibleSound;

	[SerializeField]
	private StudioEventEmitter waterSplashSound;

	[SerializeField]
	private StudioEventEmitter landingSound;

	[SerializeField]
	private StudioEventEmitter waitForMeSound;

	[SerializeField]
	private StudioEventEmitter stepSound;

	[SerializeField]
	private StudioEventEmitter rollSound;

	[SerializeField]
	private StudioEventEmitter yeetSound;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onRespawn;

	private LayerMask whatIsGround;

	private Transform followTarget;

	private Quaternion lastRotation;

	private Vector3 respawnPosition;

	private Quaternion respawnRotation;

	private float jumpTimer;

	private MovementState state;

	private float movementTimer;

	private Vector3 rayHitPointSum;

	private Vector3 rayHitNormalSum;

	private int hitAmount;

	private Vector2 moveInput;

	private Vector2 moveVector;

	private bool jumpInput;

	private bool sprintInput;

	private bool isSprinting;

	private float respawnTimer;

	private bool lookAtCoroutineActive;

	private bool isUnderwater;

	private bool isMovingForward;

	private float idleEmoteTimer;

	private bool canSayWaitForMe;

	private Transform cameraTransform;

	private Vector3 jumpStartPosition;

	private Quaternion jumpStartRotation;

	private float angleDifference;

	private Transform oldTargetTransformParent;

	private Vector3 oldTargetPosition;

	private Rigidbody targetRigidbody;

	private MovableObject targetMovableObject;

	private Vector3 defaultScale;

	private Vector3 targetVelocity;

	private CameraAngle cameraAngle;

	private FollowerManager followerManager;

	private Vector2 oldMoveInput;

	private bool webTouched;

	private static readonly int PlayerPosition = Shader.PropertyToID("_PlayerPosition");

	private float stepTimer;

	[SerializeField]
	private float stepDuration;

	private EnvironmentMaterial targetMaterial;

	private EnvironmentMaterial oldTargetMaterial;

	private float oldScale;

	private float defaultDrag;

	private float defaultAngularDrag;

	private Vector3 relativeVelocity;

	private bool isMoving;

	private RenderStyle renderStyle;

	private float sqrDistanceToPlayer;

	private float currentAncientPotionJumpForceMultiplier = 1f;

	private float currentAncientPotionSpeedMultiplier = 1f;

	private bool ancientPotionNoGravity;

	private int underWaterCounter;

	private bool canPlayLandingSound;

	public float MovementBoostFactor => movementBoostFactor;

	public MovementState State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
		}
	}

	public float LandingTime => landingTime;

	public Transform FollowTarget
	{
		get
		{
			return followTarget;
		}
		set
		{
			followTarget = value;
		}
	}

	public bool IsPlayer => isPlayer;

	public Rigidbody Rb => rb;

	public SpiderEmotes Emotes => emotes;

	public Transform Root => root;

	public SpiderCustomization Customization => spiderCustomization;

	public Vector2 MoveVector => moveVector;

	public LayerMask WhatIsGround => whatIsGround;

	public MasterLegController LegController => masterLegController;

	public bool IsWardrobeSpider => isWardrobeSpider;

	public float JumpTimer => jumpTimer;

	public Rigidbody TargetRigidbody => targetRigidbody;

	public Transform TargetTransform => targetTransform;

	public bool IsSprinting => isSprinting;

	public Transform Ball => ball;

	public bool CanFollow => playerInteraction == PlayerInteraction.Follow;

	public PlayerInteraction GetPlayerInteraction => playerInteraction;

	public MovableObject TargetMovableObject => targetMovableObject;

	public bool WebTouched => webTouched;

	public event EventHandler<OnScaleChangedEventArgs> OnScaleChanged;

	public event EventHandler<OnUnderwaterChangedEventArgs> OnUnderwaterChanged;

	private void Awake()
	{
		defaultDrag = rb.linearDamping;
		defaultAngularDrag = rb.angularDamping;
	}

	private void OnEnable()
	{
		moveInputAction.action.performed += OnMove;
		jumpInputAction.action.performed += OnJump;
		sprintInputAction.action.performed += OnSprint;
		if (isPlayer && Singleton<AncientPotionController>.Instance != null)
		{
			Singleton<AncientPotionController>.Instance.OnEffectStarted += AncientPotionController_OnEffectStarted;
			Singleton<AncientPotionController>.Instance.OnEffectEnded += AncientPotionController_OnEffectEnded;
		}
		SceneController.OnSceneLoadingStarted -= SceneControllerOnOnSceneLoadingStarted;
		SceneController.OnSceneLoadingStarted += SceneControllerOnOnSceneLoadingStarted;
		SceneController.OnSceneLoaded -= SceneControllerOnOnSceneLoaded;
		SceneController.OnSceneLoaded += SceneControllerOnOnSceneLoaded;
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		OnSettingsUpdated();
	}

	private void OnDisable()
	{
		moveInputAction.action.performed -= OnMove;
		jumpInputAction.action.performed -= OnJump;
		sprintInputAction.action.performed -= OnSprint;
		if (isPlayer && Singleton<AncientPotionController>.Instance != null)
		{
			Singleton<AncientPotionController>.Instance.OnEffectStarted -= AncientPotionController_OnEffectStarted;
			Singleton<AncientPotionController>.Instance.OnEffectEnded -= AncientPotionController_OnEffectEnded;
		}
		SceneController.OnSceneLoadingStarted -= SceneControllerOnOnSceneLoadingStarted;
		SceneController.OnSceneLoaded -= SceneControllerOnOnSceneLoaded;
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
	}

	private void Start()
	{
		rb.useGravity = false;
		rb.isKinematic = false;
		rb.linearVelocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		defaultScale = base.transform.lossyScale;
		webTouched = false;
		underWaterCounter = 0;
		state = MovementState.Walking;
		lastRotation = base.transform.rotation;
		SetRespawnPositionToCurrentPosition();
		SetIsUnderwater(value: false);
		movementTimer = movementStopTime;
		respawnTimer = respawnInterval;
		idleEmoteTimer = idleEmoteTime;
		followerManager = Singleton<GameController>.Instance.Player.GetComponent<FollowerManager>();
		OnSettingsUpdated();
	}

	private void Update()
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Paused)
		{
			moveInput = Vector2.zero;
			return;
		}
		if (isPlayer)
		{
			HandleScale();
			HandleStepAndRollSounds();
			CalculateMoveVector();
			HandleRespawn();
		}
		else if (isUnderwater)
		{
			StartCoroutine(WaterSplashCoroutine());
			Respawn();
		}
		HandleRenderStyle();
		if (ball != null && moveVector.sqrMagnitude > 0f)
		{
			Vector3 vector = base.transform.InverseTransformVector(relativeVelocity) * 57.29578f * Time.deltaTime;
			ball.RotateAround(ball.position, base.transform.right, vector.z);
			ball.RotateAround(ball.position, base.transform.forward, 0f - vector.x);
		}
	}

	private void FixedUpdate()
	{
		if (Singleton<GameController>.Instance.State != GameController.GameState.Paused)
		{
			if (isPlayer)
			{
				Shader.SetGlobalVector(PlayerPosition, base.transform.position);
			}
			if (state == MovementState.Walking || state == MovementState.Emote)
			{
				PerformWalking();
			}
			else if (state == MovementState.Jumping)
			{
				PerformJumping();
			}
		}
	}

	public IEnumerator ResetLegs(MovementState movementStateAfter)
	{
		state = MovementState.ResetLegs;
		masterLegController.ResetAllLegs();
		yield return new WaitForSeconds(0.05f);
		state = movementStateAfter;
	}

	public void ResetAllLegs()
	{
		masterLegController.ResetAllLegs();
	}

	private IEnumerator LookAtCoroutine(Vector3 lookAtPosition)
	{
		Vector3 vector = lookAtPosition - base.transform.position;
		Vector3 inverseDiff2 = base.transform.InverseTransformDirection(vector);
		float dotProduct2 = Vector3.Dot(vector, base.transform.forward);
		if (dotProduct2 > 0f && Mathf.Abs(inverseDiff2.x) < lookAtTolerance)
		{
			moveVector = Vector2.zero;
			yield break;
		}
		lookAtCoroutineActive = true;
		ball.DOLocalRotate(Vector3.zero, 1f);
		do
		{
			vector = lookAtPosition - base.transform.position;
			inverseDiff2 = base.transform.InverseTransformDirection(vector);
			dotProduct2 = Vector3.Dot(vector, base.transform.forward);
			moveVector = Vector2.ClampMagnitude(new Vector2(inverseDiff2.x, 0f) * lookAtFactor, 1f);
			yield return null;
		}
		while ((dotProduct2 < 0f || Mathf.Abs(inverseDiff2.x) > lookAtTolerance) && Singleton<GameController>.Instance.State == GameController.GameState.Dialogue);
		moveVector = Vector2.zero;
		lookAtCoroutineActive = false;
		masterLegController.ResetAllLegs();
	}

	private IEnumerator WaterSplashCoroutine()
	{
		Transform parent = waterSplashSound.transform.parent;
		waterSplashSound.transform.parent = null;
		waterSplashSound.Play();
		yield return new WaitForSeconds(2f);
		waterSplashSound.transform.parent = parent;
	}

	private void CalculateMoveVector()
	{
		cameraTransform = Singleton<CameraController>.Instance.MainCamera.transform;
		if (!lookAtCoroutineActive)
		{
			Vector3 up = base.transform.up;
			Vector3 vector = Vector3.zero;
			Vector3 vector2 = Vector3.zero;
			if (inputMappingType == 0)
			{
				vector = Vector3.Lerp(vector, cameraTransform.up, Vector3.Dot(cameraTransform.forward, -up));
				vector = Vector3.Lerp(cameraTransform.up, cameraTransform.forward, Vector3.Dot(up, Vector3.up));
				vector = Vector3.Lerp(vector, cameraTransform.up, Vector3.Dot(cameraTransform.forward, Vector3.down));
				float num = (1f - Vector3.Dot(cameraTransform.up, Vector3.up)) * (1f - Mathf.Abs(Vector3.Dot(up, cameraTransform.forward)));
				vector = Vector3.Lerp(vector, Vector3.up, num);
				vector = Vector3.Lerp(vector, cameraTransform.forward, Vector3.Dot(cameraTransform.up, up));
				Debug.Log($"{num:F2}");
				vector2 = cameraTransform.right;
			}
			else if (inputMappingType == 1)
			{
				vector = cameraTransform.forward;
				vector = Vector3.Lerp(vector, cameraTransform.up, Vector3.Dot(cameraTransform.forward, -up));
				vector = Vector3.Lerp(vector, cameraTransform.up, Vector3.Dot(cameraTransform.forward, up) * (1f - Mathf.Clamp01(Vector3.Dot(Vector3.up, up))));
				vector2 = cameraTransform.right;
				vector2 = Vector3.Lerp(Vector3.Cross(up, cameraTransform.forward) * ((Vector3.Dot(Vector3.up, up) > -0.1f) ? 1f : (-1f)), vector2, Mathf.Abs(Vector3.Dot(cameraTransform.forward, up)));
				vector2 = Vector3.Lerp(vector2, cameraTransform.right, Mathf.Abs(Vector3.Dot(cameraTransform.forward, -up)));
			}
			Vector3 vector3 = Vector3.ProjectOnPlane(vector, up).normalized * moveInput.y;
			Vector3 vector4 = Vector3.ProjectOnPlane(vector2, up).normalized * moveInput.x;
			Vector3 vector5 = (vector3 + vector4) * moveTargetDistance;
			Vector3 position = base.transform.position + vector5;
			if (moveTarget != null)
			{
				moveTarget.position = position;
			}
			Vector3 vector6 = base.transform.InverseTransformDirection(vector5);
			moveVector = Vector2.ClampMagnitude(new Vector2(vector6.x, vector6.z), 1f);
		}
	}

	private void PerformWalking()
	{
		if (isPlayer)
		{
			if (jumpInput)
			{
				InitializeJump();
				return;
			}
		}
		else
		{
			NpcWalk();
		}
		if (SettingsController.SprintMode == HoldOrToggle.Hold && Singleton<GameController>.Instance.InputIsKeyboardMouse)
		{
			isSprinting = sprintInput;
		}
		else if (sprintInput)
		{
			isSprinting = !isSprinting;
			sprintInput = false;
		}
		float num = (isSprinting ? (movementSpeed * movementBoostFactor) : movementSpeed);
		if (isUnderwater)
		{
			num *= movementUnderwaterFactor;
		}
		num *= currentAncientPotionSpeedMultiplier;
		Vector3 vector = base.transform.forward * MoveVector.y;
		_ = base.transform.position + vector * num * Time.fixedDeltaTime;
		if (moveVector.sqrMagnitude > 0f)
		{
			movementTimer = movementStopTime;
			idleEmoteTimer = idleEmoteTime;
		}
		else
		{
			movementTimer -= Time.fixedDeltaTime;
			if (isPlayer && Singleton<GameController>.Instance.State == GameController.GameState.Running)
			{
				idleEmoteTimer -= Time.fixedDeltaTime;
				if (idleEmoteTimer <= 0f)
				{
					idleEmoteTimer = idleEmoteTime;
				}
			}
			else
			{
				idleEmoteTimer = idleEmoteTime;
			}
		}
		if (!isMovingForward && moveVector.y > 0f)
		{
			isMovingForward = true;
		}
		else if (isMovingForward && moveVector.y < 0f)
		{
			isMovingForward = false;
		}
		bool flag = targetRigidbody != null && !targetRigidbody.isKinematic;
		if (isPlayer || flag || moveVector != Vector2.zero || webTouched)
		{
			CalculateRayCasts();
			webTouched = PerformRayCasts();
		}
		InstantKillSurface componentInParent = targetTransform.GetComponentInParent<InstantKillSurface>();
		if (isPlayer && componentInParent != null)
		{
			componentInParent.KillPlayer();
			Singleton<GameController>.Instance.RespawnPlayer(componentInParent.RespawnPosition);
			return;
		}
		if (hitAmount > 0)
		{
			if (targetTransform.parent != oldTargetTransformParent || moveVector.sqrMagnitude > 0f || movementTimer > 0f)
			{
				CalculateTargetPosition();
				CalculateTargetRotation();
			}
			if (isPlayer && Singleton<GameController>.Instance.State != GameController.GameState.Cutscene)
			{
				Singleton<CameraController>.Instance.SetDampingMode(webTouched ? CameraDamping.Web : CameraDamping.Default);
			}
			Vector3 b = (webTouched ? (Vector3.up * rootVerticalOffsetWeb) : (Vector3.up * rootVerticalOffsetDefault));
			root.transform.localPosition = Vector3.Lerp(root.transform.localPosition, b, 1f / (verticalSmoothness + 1f));
			MoveToTargetTransform(num);
			if (targetRigidbody != null)
			{
				float num2 = ((targetMovableObject != null) ? targetMovableObject.GravityMultiplier : 0f);
				Vector3 force = Physics.gravity * (num2 * rb.mass);
				targetRigidbody.AddForceAtPosition(force, root.position, ForceMode.Force);
			}
		}
		else
		{
			state = MovementState.Jumping;
			masterLegController.State = MasterLegController.LegState.Jumping;
			if (!ancientPotionNoGravity)
			{
				rb.useGravity = true;
			}
			OnJumpInitialized?.Invoke();
		}
		lastRotation = base.transform.rotation;
	}

	private void NpcWalk()
	{
		switch (playerInteraction)
		{
		case PlayerInteraction.None:
			moveVector = Vector2.zero;
			break;
		case PlayerInteraction.Follow:
			if (followTarget != null)
			{
				Vector3 direction = followTarget.transform.position - base.transform.position;
				if (direction.magnitude > minFollowDistance)
				{
					Vector3 normalized = base.transform.InverseTransformDirection(direction).normalized;
					moveVector = new Vector2(normalized.x, normalized.z).normalized;
				}
				else
				{
					moveVector = Vector2.zero;
				}
				if (direction.magnitude > waitForMeActivateDistance && canSayWaitForMe)
				{
					canSayWaitForMe = false;
					waitForMeSound.Play();
				}
				else if (direction.magnitude < waitForMeDeactivateDistance)
				{
					canSayWaitForMe = true;
				}
			}
			else
			{
				moveVector = Vector2.zero;
			}
			break;
		case PlayerInteraction.LookAt:
		{
			if (Singleton<GameController>.Instance.State == GameController.GameState.Dialogue)
			{
				break;
			}
			Vector3 vector = lookAtTarget switch
			{
				LookAtTarget.Player => Singleton<GameController>.Instance.Player.transform.position, 
				LookAtTarget.TargetTransform => (lookAtTargetTransform == null) ? (base.transform.position + base.transform.forward) : lookAtTargetTransform.position, 
				_ => throw new ArgumentOutOfRangeException(), 
			} - base.transform.position;
			if (vector.magnitude < lookAtDistance)
			{
				Vector3 vector2 = base.transform.InverseTransformDirection(vector);
				moveVector = Vector2.ClampMagnitude(new Vector2(vector2.x, 0f) * lookAtFactor, 1f);
				ball.localRotation = Quaternion.identity;
				if (Vector3.Dot(vector, base.transform.forward) > 0f && Mathf.Abs(vector2.x) < lookAtTolerance)
				{
					moveVector.x = 0f;
				}
			}
			else
			{
				moveVector = Vector2.zero;
				ball.localRotation = Quaternion.identity;
			}
			break;
		}
		}
	}

	public void YeetPlayer(float forceMagnitude, Vector3 direction)
	{
		if (state != MovementState.Jumping)
		{
			jumpInput = false;
			state = MovementState.Jumping;
			masterLegController.State = MasterLegController.LegState.Jumping;
			if (!ancientPotionNoGravity)
			{
				rb.useGravity = true;
			}
			jumpTimer = jumpDelay;
			if (TargetRigidbody != null)
			{
				Vector3 force = -targetTransform.up * rb.mass * opposingJumpingForceMultiplier;
				TargetRigidbody.AddForceAtPosition(force, rb.position, ForceMode.Impulse);
			}
			base.transform.parent = null;
			targetTransform.parent = base.transform;
			targetRigidbody = null;
			targetMaterial = null;
			Vector3 force2 = direction * forceMagnitude;
			rb.AddForce(force2);
			Singleton<WebController>.Instance.ActivateDefaultMode();
			Singleton<WebController>.Instance.RecalculateSpringJoint();
			ballScaler.DOKill();
			ballScaler.localScale = Vector3.one;
			ballScaler.DOPunchScale(new Vector3(-0.25f, 0.5f, -0.25f), 0.5f, 5);
			yeetSound.Play();
			OnJumpInitialized?.Invoke();
		}
	}

	private void InitializeJump()
	{
		jumpInput = false;
		Vector3 zero = Vector3.zero;
		float num = ((Vector3.Dot(base.transform.up, Vector3.up) >= -0.8f) ? 1f : (-0.5f));
		zero = Vector3.Lerp(Vector3.up * (num * Mathf.Sin(jumpAngle * (MathF.PI / 180f))), base.transform.up * Mathf.Sin(jumpAngle * (MathF.PI / 180f)), aimJumpUpwardsFactor).normalized * (forceMagnitude * currentAncientPotionJumpForceMultiplier);
		if (!Physics.SphereCast(new Ray(base.transform.position + base.transform.up * jumpCheckOffset, zero.normalized), jumpCheckRadius, jumpCheckDistance, WhatIsGround))
		{
			state = MovementState.Jumping;
			masterLegController.State = MasterLegController.LegState.Jumping;
			if (!ancientPotionNoGravity)
			{
				rb.useGravity = true;
			}
			jumpTimer = jumpDelay;
			if (TargetRigidbody != null)
			{
				Vector3 force = -targetTransform.up * rb.mass * opposingJumpingForceMultiplier;
				TargetRigidbody.AddForceAtPosition(force, rb.position, ForceMode.Impulse);
			}
			base.transform.parent = null;
			targetTransform.parent = base.transform;
			targetRigidbody = null;
			targetMaterial = null;
			rb.AddForce(zero);
			jumpStartPosition = base.transform.position;
			jumpStartRotation = base.transform.rotation;
			Singleton<WebController>.Instance.RecalculateSpringJoint();
			rollSound.Stop();
			isMoving = false;
			jumpSound.Play();
			ballScaler.DOKill();
			ballScaler.localScale = Vector3.one;
			ballScaler.DOPunchScale(new Vector3(-0.25f, 0.5f, -0.25f), 0.5f, 5);
			OnJumpInitialized?.Invoke();
		}
		else
		{
			jumpNotPossibleSound.Play();
		}
	}

	private void CalculateTargetPosition()
	{
		Vector3 position = rayHitPointSum / hitAmount;
		targetTransform.position = position;
	}

	private void CalculateTargetRotation()
	{
		Vector3 vector = rayHitNormalSum / hitAmount;
		angleDifference = MoveVector.x * (1f + rotationSmoothness) * rotationSpeed * Time.fixedDeltaTime;
		Quaternion rotation = Quaternion.LookRotation(Vector3.Cross(Quaternion.AngleAxis(angleDifference, vector) * base.transform.right, vector), vector);
		targetTransform.rotation = rotation;
	}

	private void MoveToTargetTransform(float speed)
	{
		Quaternion rotation = targetTransform.rotation;
		float f = Quaternion.Angle(lastRotation, rotation);
		float num = rotationSmoothness;
		float t = 1f / (1f + num);
		float num2 = Mathf.Abs(f) - Mathf.Abs(angleDifference);
		if (Mathf.Abs(num2) > angelDiffThreshold)
		{
			t = (1f + num2 * angleDiffFactor) / (1f + num);
		}
		base.transform.rotation = Quaternion.Slerp(lastRotation, rotation, t);
		Vector3 position = targetTransform.position;
		Vector3 vector = base.transform.InverseTransformPoint(position);
		float num3 = verticalSpeed;
		relativeVelocity = (base.transform.forward * MoveVector.y + (base.transform.up * vector.y + base.transform.forward * vector.z + base.transform.right * vector.x) * num3) * speed;
		Vector3 linearVelocity = Vector3.ClampMagnitude(relativeVelocity, speed);
		_ = Vector3.zero;
		if (targetRigidbody != null)
		{
			Vector3 rhs = position - targetRigidbody.worldCenterOfMass;
			Vector3 vector2 = Vector3.Cross(targetRigidbody.angularVelocity, rhs);
			linearVelocity += vector2 + targetRigidbody.linearVelocity;
		}
		rb.angularVelocity = Vector3.zero;
		rb.linearVelocity = linearVelocity;
		oldTargetPosition = position;
		oldTargetTransformParent = targetTransform.parent;
	}

	private void CalculateRayCasts()
	{
		RayConfigurationFan[] array = rayConfigurationFans;
		foreach (RayConfigurationFan rayConfigurationFan in array)
		{
			rayConfigurationFan.rays = new Ray[rayConfigurationFan.numberOfRays];
			for (int j = 0; j < rayConfigurationFan.numberOfRays; j++)
			{
				float num = ((rayConfigurationFan.numberOfRays != 1) ? (90f + rayConfigurationFan.directionAngle - rayConfigurationFan.fanAngle / 2f + rayConfigurationFan.fanAngle / (float)(rayConfigurationFan.numberOfRays - 1) * (float)j) : (90f + rayConfigurationFan.directionAngle));
				if (rayConfigurationFan.followDirection && !isMovingForward)
				{
					num += 180f;
				}
				num *= MathF.PI / 180f;
				Vector2 vector = new Vector2(Mathf.Cos(num), Mathf.Sin(num));
				Transform transform = base.transform;
				Vector3 origin = transform.position + (transform.up * rayConfigurationFan.verticalOffset + transform.right * (rayConfigurationFan.radius * vector.x) + transform.forward * (rayConfigurationFan.radius * vector.y)) * base.transform.lossyScale.x;
				Vector3 normalized = (transform.right * (vector.x * Mathf.Cos(rayConfigurationFan.verticalAngle * (MathF.PI / 180f))) + transform.forward * (vector.y * Mathf.Cos(rayConfigurationFan.verticalAngle * (MathF.PI / 180f))) + transform.up * Mathf.Sin(rayConfigurationFan.verticalAngle * (MathF.PI / 180f))).normalized;
				rayConfigurationFan.rays[j] = new Ray(origin, normalized);
				Debug.DrawLine(rayConfigurationFan.rays[j].origin, rayConfigurationFan.rays[j].origin + rayConfigurationFan.rays[j].direction * rayConfigurationFan.rayLength, rayConfigurationFan.color);
			}
		}
	}

	private bool PerformRayCasts(bool isLanding = false)
	{
		rayHitPointSum = Vector3.zero;
		rayHitNormalSum = Vector3.zero;
		hitAmount = 0;
		List<Transform> list = new List<Transform>();
		List<Collider> list2 = new List<Collider>();
		webTouched = false;
		RayConfigurationFan[] array = rayConfigurationFans;
		foreach (RayConfigurationFan rayConfigurationFan in array)
		{
			Ray[] rays = rayConfigurationFan.rays;
			foreach (Ray ray in rays)
			{
				LayerMask layerMask = whatIsGround;
				if (isLanding)
				{
					layerMask = (int)layerMask | LayerMask.GetMask("Movable");
				}
				if (Physics.SphereCast(ray, rayConfigurationFan.sphereRadius * base.transform.lossyScale.x, out var hitInfo, rayConfigurationFan.rayLength * base.transform.lossyScale.x, layerMask))
				{
					Vector3 vector = ((hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Web")) ? (hitInfo.point - hitInfo.normal * (Singleton<WebController>.Instance.WebColliderRadius - 0.1f)) : hitInfo.point);
					rayHitPointSum += vector;
					rayHitNormalSum += hitInfo.normal;
					hitAmount++;
					if (isPlayer && (hitInfo.transform.CompareTag("Web") || hitInfo.transform.CompareTag("WebbedObject") || _Scripts.Utils.Utils.IsLayerInLayerMask(hitInfo.transform.gameObject.layer, climbableLayerMask)))
					{
						webTouched = true;
					}
					list.Add(hitInfo.transform);
					list2.Add(hitInfo.collider);
				}
			}
		}
		if (hitAmount > 0)
		{
			targetTransform.parent = (from i in list
				group i by i into grp
				orderby grp.Count() descending
				select grp.Key).First();
			targetRigidbody = targetTransform.GetComponentInParent<Rigidbody>();
			Collider collider = (from i in list2
				group i by i into grp
				orderby grp.Count() descending
				select grp.Key).First();
			targetMaterial = collider.GetComponentInParent<EnvironmentMaterial>();
			MovableObject movableObject = targetMovableObject;
			targetMovableObject = ((targetRigidbody != null) ? targetRigidbody.GetComponent<MovableObject>() : null);
			if (movableObject != targetMovableObject)
			{
				if (movableObject != null)
				{
					movableObject.ResetLayer();
					movableObject.ResetCollisionExclusions();
				}
				if (targetMovableObject != null)
				{
					targetMovableObject.ExcludePlayerCollision();
				}
			}
			if (isLanding && targetMovableObject != null)
			{
				targetMovableObject.SetLayerToWebbedObject();
			}
			base.transform.SetParent(targetTransform.parent, worldPositionStays: true);
			if (targetTransform.parent != oldTargetTransformParent)
			{
				Singleton<WebController>.Instance.RecalculateSpringJoint();
			}
		}
		else
		{
			targetTransform.parent = base.transform;
			base.transform.parent = null;
			base.transform.localScale = defaultScale;
			targetMaterial = null;
		}
		return webTouched;
	}

	private void PerformJumping()
	{
		jumpTimer -= Time.fixedDeltaTime;
		movementTimer = movementStopTime;
		bool flag = false;
		flag = Physics.SphereCast(new Ray(base.transform.position, rb.linearVelocity.normalized), 0.5f, out var hitInfo, 5f, WhatIsGround);
		Vector3 normalized = Vector3.Cross(Vector3.up, rb.linearVelocity.normalized).normalized;
		_ = Vector3.Cross(normalized, Vector3.up).normalized;
		Vector3 zero = Vector3.zero;
		if (!flag)
		{
			Vector3 vector = Singleton<CameraController>.Instance.InputTransform.InverseTransformDirection(rb.linearVelocity);
			vector.y = 0f;
			if (Mathf.Abs(vector.x) < aerialAccelerationThreshold || (vector.x < 0f - aerialAccelerationThreshold && moveInput.x > 0f) || (vector.x > aerialAccelerationThreshold && moveInput.x < 0f))
			{
				zero += Singleton<CameraController>.Instance.InputTransform.right * moveInput.x * aerialControlSpeedLeftRight;
			}
			if (Mathf.Abs(vector.z) < aerialAccelerationThreshold || (vector.z < 0f - aerialAccelerationThreshold && moveInput.y > 0f) || (vector.z > aerialAccelerationThreshold && moveInput.y < 0f))
			{
				zero += Singleton<CameraController>.Instance.InputTransform.forward * moveInput.y * aerialControlSpeedForwardBackwards;
			}
			rb.linearVelocity += Vector3.ClampMagnitude(zero, aerialControlSpeedForwardBackwards) * Time.fixedDeltaTime;
		}
		Vector3 vector2 = ((!SettingsController.ArachnophobiaMode) ? (Quaternion.AngleAxis(0f - pitchAngle, normalized) * rb.linearVelocity.normalized) : (Quaternion.AngleAxis(-90f, normalized) * rb.linearVelocity.normalized));
		Vector3 upwards = Vector3.Cross(normalized, -vector2);
		if (jumpTimer <= 0f && flag && (!Singleton<WebController>.Instance.WebActive || rb.linearVelocity.magnitude < bounceMinimumVelocity))
		{
			upwards = hitInfo.normal;
			vector2 = Vector3.Cross(normalized, hitInfo.normal);
			if (isPlayer && Singleton<GameController>.Instance.State != GameController.GameState.Cutscene)
			{
				Singleton<CameraController>.Instance.SetDampingMode(CameraDamping.Default);
			}
		}
		else if (isPlayer && Singleton<GameController>.Instance.State != GameController.GameState.Cutscene)
		{
			Singleton<CameraController>.Instance.SetDampingMode(CameraDamping.Jumping);
		}
		if (rb.linearVelocity.magnitude > 0f)
		{
			Quaternion b = Quaternion.LookRotation(vector2, upwards);
			float num = ((flag && (!Singleton<WebController>.Instance.WebActive || rb.linearVelocity.magnitude > bounceMinimumVelocity)) ? landingRotationSmoothness : jumpingRotationSmoothness);
			base.transform.rotation = Quaternion.Slerp(lastRotation, b, 1f / (1f + num));
			ballScaler.rotation = jumpStartRotation;
			Vector3 vector3 = rb.linearVelocity / 10f * 57.29578f * Time.fixedDeltaTime;
			ball.RotateAround(ball.position, Vector3.right, vector3.z);
			ball.RotateAround(ball.position, Vector3.forward, 0f - vector3.x);
		}
		lastRotation = base.transform.rotation;
		if ((isPlayer && jumpTimer > 0f) || (Singleton<WebController>.Instance.WebActive && rb.linearVelocity.magnitude > bounceMinimumVelocity))
		{
			return;
		}
		LayerMask layerMask = (int)whatIsGround | LayerMask.GetMask("Movable");
		if (SettingsController.ArachnophobiaMode)
		{
			if (Physics.CheckSphere(base.transform.position + base.transform.up * landingTriggerOffsetArachnophobia, landingTriggerRadiusArachnophobia, layerMask))
			{
				PerformLanding();
			}
		}
		else if (Physics.CheckSphere(base.transform.position + base.transform.up * landingTriggerOffset, landingTriggerRadius, layerMask))
		{
			PerformLanding();
		}
	}

	private void HandleRespawn()
	{
		if (base.transform.position.y <= -50f)
		{
			Singleton<GameController>.Instance.RespawnPlayer(null);
			AchievementEvents.JumpIntoVoid();
		}
		respawnTimer -= Time.deltaTime;
		if (respawnTimer <= 0f)
		{
			respawnTimer += respawnInterval;
			if (state == MovementState.Walking && base.transform.position.y > minRespawnHeight && targetRigidbody == null && !targetTransform.parent.CompareTag("Web") && !isUnderwater)
			{
				SetRespawnPositionToCurrentPosition();
			}
		}
	}

	private void HandleScale()
	{
		float x = base.transform.lossyScale.x;
		if (x < minScale)
		{
			if (base.transform.parent == null)
			{
				base.transform.localScale = minScale * Vector3.one;
			}
			else
			{
				float x2 = base.transform.parent.lossyScale.x;
				base.transform.localScale = minScale / x2 * Vector3.one;
			}
		}
		else if (x > maxScale)
		{
			if (base.transform.parent == null)
			{
				base.transform.localScale = maxScale * Vector3.one;
			}
			else
			{
				float x3 = base.transform.parent.lossyScale.x;
				base.transform.localScale = maxScale / x3 * Vector3.one;
			}
		}
		if (x != oldScale)
		{
			defaultScale = base.transform.lossyScale;
			this.OnScaleChanged?.Invoke(this, new OnScaleChangedEventArgs
			{
				scale = x
			});
		}
		oldScale = x;
	}

	private void HandleStepAndRollSounds()
	{
		if (!(targetMaterial != null))
		{
			return;
		}
		if (SettingsController.ArachnophobiaMode)
		{
			if (!isMoving && moveInput.sqrMagnitude > 0.1f)
			{
				isMoving = true;
				rollSound.Play();
			}
			else if (isMoving && moveInput.sqrMagnitude <= 0.1f)
			{
				isMoving = false;
				rollSound.Stop();
			}
			if (targetMaterial != oldTargetMaterial)
			{
				rollSound.SetParameter("step_material", (float)targetMaterial.Mat);
			}
			oldTargetMaterial = targetMaterial;
		}
		else
		{
			stepTimer -= Time.deltaTime * relativeVelocity.magnitude;
			if (moveInput.sqrMagnitude > 0.1f && stepTimer <= 0f)
			{
				stepTimer = stepDuration;
				stepSound.Play();
				stepSound.SetParameter("step_material", (float)targetMaterial.Mat);
			}
		}
	}

	private void HandleRenderStyle()
	{
	}

	private void ApplyRenderStyle()
	{
		switch (renderStyle)
		{
		case RenderStyle.Spider:
			root.gameObject.SetActive(!SettingsController.ArachnophobiaMode);
			ball.gameObject.SetActive(SettingsController.ArachnophobiaMode);
			break;
		case RenderStyle.Ball:
			root.gameObject.SetActive(value: false);
			ball.gameObject.SetActive(value: true);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void ApplyAncientPotionEffect(AncientPotionEffectSo ancientPotionEffectSo)
	{
		switch (ancientPotionEffectSo.effectType)
		{
		case AncientPotionEffectType.SuperJump:
			currentAncientPotionJumpForceMultiplier = ancientPotionJumpForceMultiplier;
			break;
		case AncientPotionEffectType.SuperSpeed:
			currentAncientPotionSpeedMultiplier = ancientPotionSpeedMultiplier;
			break;
		case AncientPotionEffectType.NoGravity:
			rb.useGravity = false;
			ancientPotionNoGravity = true;
			break;
		case AncientPotionEffectType.RubberDuck:
			root.gameObject.SetActive(value: false);
			ball.gameObject.SetActive(value: false);
			ancientPotionRubberDuck.SetActive(value: true);
			break;
		case AncientPotionEffectType.LongWeb:
		case AncientPotionEffectType.HueWeb:
		case AncientPotionEffectType.ColorFilter:
			break;
		}
	}

	private void RemoveAncientPotionEffect(AncientPotionEffectSo ancientPotionEffectSo)
	{
		switch (ancientPotionEffectSo.effectType)
		{
		case AncientPotionEffectType.SuperJump:
			currentAncientPotionJumpForceMultiplier = 1f;
			break;
		case AncientPotionEffectType.SuperSpeed:
			currentAncientPotionSpeedMultiplier = 1f;
			break;
		case AncientPotionEffectType.NoGravity:
			if (state == MovementState.Jumping)
			{
				rb.useGravity = true;
			}
			ancientPotionNoGravity = false;
			break;
		case AncientPotionEffectType.RubberDuck:
			ancientPotionRubberDuck.SetActive(value: false);
			ApplyRenderStyle();
			break;
		case AncientPotionEffectType.LongWeb:
		case AncientPotionEffectType.HueWeb:
		case AncientPotionEffectType.ColorFilter:
			break;
		}
	}

	public void Respawn(Vector3 position, Quaternion rotation)
	{
		followTarget = null;
		if (isPlayer)
		{
			followerManager.ForceRemoveAllFollowers();
			Singleton<CameraController>.Instance.ResetFollowCamera();
		}
		else if (followerManager != null)
		{
			followerManager.RemoveFollower(base.gameObject);
			followTarget = null;
		}
		emotes.StopEmote();
		state = MovementState.Walking;
		rb.isKinematic = true;
		rb.useGravity = false;
		targetRigidbody = null;
		targetTransform.parent = base.transform;
		base.transform.position = position;
		base.transform.rotation = rotation;
		lastRotation = respawnRotation;
		ballScaler.rotation = Quaternion.identity;
		masterLegController.LandingTimer = landingTime;
		masterLegController.State = MasterLegController.LegState.Landing;
		masterLegController.ResetAllLegs(instant: true);
		movementTimer = movementStopTime;
		rb.isKinematic = false;
		rb.linearVelocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		jumpInput = false;
		underWaterCounter = 0;
		CalculateRayCasts();
		PerformRayCasts();
		SetIsUnderwater(value: false);
		if (hitAmount > 0)
		{
			CalculateTargetPosition();
			CalculateTargetRotation();
		}
		onRespawn.Invoke();
	}

	public void Respawn(Transform respawnTransform = null)
	{
		if (respawnTransform != null)
		{
			respawnPosition = respawnTransform.position;
			respawnRotation = respawnTransform.rotation;
		}
		Respawn(respawnPosition, respawnRotation);
	}

	public void PerformLanding()
	{
		if (isPlayer)
		{
			Singleton<CameraController>.Instance.PerformScreenShake();
		}
		Vector3 linearVelocity = rb.linearVelocity;
		masterLegController.State = MasterLegController.LegState.Landing;
		state = MovementState.Walking;
		rb.useGravity = false;
		rb.linearVelocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		if (moveInput.magnitude == 0f)
		{
			masterLegController.ResetAllLegs();
		}
		yeetSound.Stop();
		if (canPlayLandingSound)
		{
			landingSound.Play();
		}
		CalculateRayCasts();
		PerformRayCasts(isLanding: true);
		InstantKillSurface component = targetTransform.parent.GetComponent<InstantKillSurface>();
		if (isPlayer && component != null)
		{
			component.KillPlayer();
			Singleton<GameController>.Instance.RespawnPlayer(component.RespawnPosition);
			return;
		}
		if (hitAmount > 0)
		{
			CalculateTargetPosition();
			CalculateTargetRotation();
			if (targetRigidbody != null && !targetRigidbody.isKinematic)
			{
				Vector3 force = linearVelocity * landingForceMultiplier;
				targetRigidbody.AddForceAtPosition(force, rb.position, ForceMode.Impulse);
			}
			else if (base.transform.position.y > minRespawnHeight && !targetTransform.parent.CompareTag("Web") && !isUnderwater)
			{
				respawnTimer = respawnInterval;
				SetRespawnPositionToCurrentPosition();
			}
			if (RaceController.Instance != null)
			{
				RaceController.Instance.CancelRace();
			}
		}
		ballScaler.localRotation = Quaternion.identity;
		ballScaler.DOKill();
		ballScaler.localScale = Vector3.one;
		ballScaler.DOPunchScale(new Vector3(0.25f, -0.5f, 0.25f), 0.5f, 5);
		OnLandingPerformed?.Invoke();
	}

	public void OnStartDialogue(Vector3 lookAtPosition)
	{
		StartCoroutine(LookAtCoroutine(lookAtPosition));
	}

	public void SetRespawnPositionToCurrentPosition()
	{
		respawnPosition = targetTransform.position;
		respawnRotation = targetTransform.rotation;
	}

	public void SetRespawnPosition(Transform newRespawnTransform)
	{
		respawnPosition = newRespawnTransform.position;
		respawnRotation = newRespawnTransform.rotation;
	}

	public void SetPlayerInteraction(PlayerInteraction newPlayerInteraction)
	{
		playerInteraction = newPlayerInteraction;
	}

	public void SetPlayerInteractionToLookAt()
	{
		playerInteraction = PlayerInteraction.LookAt;
	}

	public void SetIsUnderwater(bool value)
	{
		if (!isPlayer)
		{
			return;
		}
		underWaterCounter += (value ? 1 : (-1));
		underWaterCounter = Mathf.Max(0, underWaterCounter);
		if (!isUnderwater && underWaterCounter > 0)
		{
			isUnderwater = true;
			waterSplashSound.Play();
			rb.linearDamping = underwaterDrag;
			rb.angularDamping = underwaterAngularDrag;
			this.OnUnderwaterChanged?.Invoke(this, new OnUnderwaterChangedEventArgs
			{
				isUnderwater = true
			});
			if (underwaterAirBubbleParticles != null)
			{
				underwaterAirBubbleParticles.Play();
			}
			Singleton<MusicController>.Instance.StartUnderwater();
		}
		else if (isUnderwater && underWaterCounter == 0)
		{
			isUnderwater = false;
			rb.linearDamping = defaultDrag;
			rb.angularDamping = defaultAngularDrag;
			this.OnUnderwaterChanged?.Invoke(this, new OnUnderwaterChangedEventArgs
			{
				isUnderwater = false
			});
			if (underwaterAirBubbleParticles != null)
			{
				underwaterAirBubbleParticles.Stop();
			}
			Singleton<MusicController>.Instance.StopUnderwater();
		}
	}

	public void ResetInput()
	{
		moveInput = Vector2.zero;
		moveVector = Vector2.zero;
		jumpInput = false;
		sprintInput = false;
	}

	public void MobileMove(Vector2 value)
	{
	}

	public void MobileJump(bool value)
	{
	}

	public void MobileSprint(bool value)
	{
	}

	public void SetLookAtTargetTransform(Transform newLookAtTransform)
	{
		lookAtTargetTransform = newLookAtTransform;
	}

	private void OnMove(InputAction.CallbackContext ctx)
	{
		if (Singleton<GameController>.Instance.State != 0 || !isPlayer || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			moveInput = Vector2.zero;
		}
		else
		{
			moveInput = ctx.ReadValue<Vector2>();
		}
	}

	private void OnJump(InputAction.CallbackContext ctx)
	{
		if (Singleton<GameController>.Instance.State != 0 || !isPlayer || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			jumpInput = false;
		}
		else if (state == MovementState.Jumping)
		{
			if (Singleton<WebController>.Instance.WebActive)
			{
				jumpInput = false;
			}
		}
		else
		{
			Singleton<MobileControls>.Instance.ShowButtons(value: false);
			jumpInput = ctx.ReadValueAsButton();
		}
	}

	private void OnSprint(InputAction.CallbackContext ctx)
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Running && isPlayer && !Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			Singleton<MobileControls>.Instance.ShowButtons(value: false);
			sprintInput = ctx.ReadValueAsButton();
		}
	}

	private void OnSettingsUpdated()
	{
		ApplyRenderStyle();
		whatIsGround = (SettingsController.WaterWalking ? whatIsGroundWaterWalking : whatIsGroundDefault);
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		OnSettingsUpdated();
	}

	private void AncientPotionController_OnEffectStarted(AncientPotionEffectSo ancientPotionEffectSo)
	{
		ApplyAncientPotionEffect(ancientPotionEffectSo);
	}

	private void AncientPotionController_OnEffectEnded(AncientPotionEffectSo ancientPotionEffectSo)
	{
		RemoveAncientPotionEffect(ancientPotionEffectSo);
	}

	private void SceneControllerOnOnSceneLoadingStarted(object sender, EventArgs e)
	{
		rb.isKinematic = true;
	}

	private void SceneControllerOnOnSceneLoaded(object sender, EventArgs e)
	{
		canPlayLandingSound = true;
	}
}
