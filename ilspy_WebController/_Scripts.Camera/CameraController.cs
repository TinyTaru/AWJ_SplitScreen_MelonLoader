using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using _Scripts.Camera;
using _Scripts.Spider;
using _Scripts.Utils;
using _Scripts.Wardrobe;

namespace _Scripts.Singletons;

public class CameraController : Singleton<CameraController>
{
	[Header("References")]
	[SerializeField]
	private UnityEngine.Camera mainCamera;

	[SerializeField]
	private UnityEngine.Camera dialogueCamera;

	[SerializeField]
	private UnityEngine.Camera wardrobeCamera;

	[SerializeField]
	private CinemachineVirtualCamera cinemachineFollowCamera;

	[SerializeField]
	private CinemachineVirtualCamera cinemachineFreeLookCamera;

	[SerializeField]
	private FollowTarget followCameraFollowTarget;

	[SerializeField]
	private CameraMouseLook followCameraMouseLook;

	[SerializeField]
	private Transform inputTransform;

	[Header("Follow Camera Settings")]
	[SerializeField]
	private float followCameraDampingDefault = 1f;

	[SerializeField]
	private float followCameraDampingWeb = 1f;

	[SerializeField]
	private float followCameraDampingJumping = 0.1f;

	[SerializeField]
	private float disableHatDistance = 2f;

	[SerializeField]
	private float enableHatDistance = 2.5f;

	[Header("Screen Shake")]
	[Space(5f)]
	[SerializeField]
	private NoiseSettings screenShakeNoiseSettings;

	[SerializeField]
	private float screenShakeMaxAmplitude;

	[SerializeField]
	private float screenShakeFrequency;

	[SerializeField]
	private float screenShakeDuration;

	[SerializeField]
	private float screenShakeMinVelocity;

	[SerializeField]
	private float screenShakeMaxVelocity;

	[Header("Free Look Camera Settings")]
	[SerializeField]
	private bool showPlayerInFreeLookCamera;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference freeLookCameraInputAction;

	private CinemachineBrain cinemachineBrain;

	private CinemachineBasicMultiChannelPerlin followCameraPerlin;

	private float screenShakeTimer;

	private bool followCameraResetActive;

	private bool canMoveCamera;

	private bool hatEnabled;

	private bool instantCamera;

	private CameraZoom cameraZoom;

	private Vector3 shoulderOffset;

	private int dialogueCameraUsers;

	public UnityEngine.Camera MainCamera => Singleton<CameraController>.Instance.mainCamera;

	public CameraMouseLook FollowCameraMouseLook => followCameraMouseLook;

	public CinemachineBrain Brain => cinemachineBrain;

	public bool CanMoveCamera => canMoveCamera;

	public Transform InputTransform => inputTransform;

	public FollowTarget FollowCameraFollowTarget => followCameraFollowTarget;

	public bool InstantCamera => instantCamera;

	protected override void Awake()
	{
		base.Awake();
		if (Singleton<CameraController>.Instance == this)
		{
			cinemachineBrain = Singleton<CameraController>.Instance.mainCamera.GetComponent<CinemachineBrain>();
		}
	}

	private void OnEnable()
	{
		freeLookCameraInputAction.action.performed += OnFreeLookCamera;
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		}
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnContinueGame += GameController_OnContinueGame;
		}
	}

	private void OnDisable()
	{
		freeLookCameraInputAction.action.performed -= OnFreeLookCamera;
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		}
	}

	private void Start()
	{
		followCameraPerlin = Singleton<CameraController>.Instance.cinemachineFollowCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
		cameraZoom = Singleton<CameraController>.Instance.cinemachineFollowCamera.GetComponent<CameraZoom>();
		StopScreenShake();
		ResetFollowCamera();
		DisableWardrobeCamera();
		UpdateSMAA();
		UpdateFieldOfView();
		hatEnabled = true;
		canMoveCamera = true;
		Singleton<CameraController>.Instance.dialogueCameraUsers = 0;
		Singleton<CameraController>.Instance.dialogueCamera.gameObject.SetActive(value: false);
		OnRespawn();
		SpiderInteraction[] array = UnityEngine.Object.FindObjectsByType<SpiderInteraction>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (SpiderInteraction obj in array)
		{
			obj.OnInteractableAdded += SpiderInteraction_OnInteractableAdded;
			obj.OnInteractableRemoved += SpiderInteraction_OnInteractableRemoved;
		}
	}

	private void Update()
	{
		if (screenShakeTimer > 0f)
		{
			screenShakeTimer -= Time.deltaTime;
		}
		else
		{
			StopScreenShake();
		}
		Vector3 vector = Vector3.Cross(Singleton<CameraController>.Instance.mainCamera.transform.right, Vector3.up);
		inputTransform.LookAt(inputTransform.position + vector, Vector3.up);
		if (hatEnabled && GetCameraDistance() < disableHatDistance)
		{
			hatEnabled = false;
			Singleton<GameController>.Instance.Player.Customization.EnableHat(hatEnabled);
		}
		else if (!hatEnabled && GetCameraDistance() > enableHatDistance)
		{
			hatEnabled = true;
			Singleton<GameController>.Instance.Player.Customization.EnableHat(hatEnabled);
		}
	}

	private void FixedUpdate()
	{
		Singleton<CameraController>.Instance.cinemachineFollowCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().ShoulderOffset = shoulderOffset;
	}

	private IEnumerator ResetFollowCameraCoroutine()
	{
		SetDampingMode(CameraDamping.Instant);
		followCameraResetActive = true;
		yield return new WaitForSeconds(0.1f);
		followCameraResetActive = false;
		SetDampingMode(CameraDamping.Default);
	}

	private IEnumerator PreventCameraMovementCoroutine(float duration)
	{
		canMoveCamera = false;
		yield return new WaitForSeconds(duration);
		canMoveCamera = true;
	}

	private IEnumerator OnRespawnCoroutine()
	{
		instantCamera = true;
		yield return new WaitForSeconds(0.1f);
		instantCamera = false;
		canMoveCamera = true;
	}

	private void StopScreenShake()
	{
		followCameraPerlin.m_AmplitudeGain = 0f;
		followCameraPerlin.m_FrequencyGain = 0f;
	}

	private void UpdateSMAA()
	{
		if (!(Singleton<CameraController>.Instance.mainCamera == null))
		{
			Singleton<CameraController>.Instance.mainCamera.GetComponent<UniversalAdditionalCameraData>().antialiasing = (SettingsController.SMAA ? AntialiasingMode.SubpixelMorphologicalAntiAliasing : AntialiasingMode.None);
		}
	}

	private void UpdateFieldOfView()
	{
		float horizontalFOVDeg = Mathf.Lerp(SettingsController.MinFieldOfView, SettingsController.MaxFieldOfView, SettingsController.FieldOfView);
		float resolutionRatio = SettingsController.GetResolutionRatio();
		float cinemachineFollowCameraFOV = _Scripts.Utils.Utils.ConvertHorizontalToVerticalFOV(horizontalFOVDeg, resolutionRatio);
		SetCinemachineFollowCameraFOV(cinemachineFollowCameraFOV);
	}

	public static void AddDialogueCameraUser()
	{
		Singleton<CameraController>.Instance.dialogueCameraUsers++;
		Singleton<CameraController>.Instance.dialogueCamera.gameObject.SetActive(value: true);
	}

	public static void RemoveDialogueCameraUser()
	{
		Singleton<CameraController>.Instance.dialogueCameraUsers--;
		Singleton<CameraController>.Instance.dialogueCameraUsers = Mathf.Max(0, Singleton<CameraController>.Instance.dialogueCameraUsers);
		if (Singleton<CameraController>.Instance.dialogueCameraUsers == 0)
		{
			Singleton<CameraController>.Instance.dialogueCamera.gameObject.SetActive(value: false);
		}
	}

	public void SetDampingMode(CameraDamping dampingMode)
	{
		if (!followCameraResetActive)
		{
			float num = 0f;
			switch (dampingMode)
			{
			default:
				num = followCameraDampingDefault;
				cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
				break;
			case CameraDamping.Web:
				num = followCameraDampingWeb;
				cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
				break;
			case CameraDamping.Jumping:
				num = followCameraDampingJumping;
				cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
				break;
			case CameraDamping.Instant:
				num = 0f;
				cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
				break;
			case CameraDamping.Cutscene:
				num = 0f;
				cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
				break;
			}
			Singleton<CameraController>.Instance.cinemachineFollowCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().Damping = Vector3.one * num;
		}
	}

	public void PerformScreenShake()
	{
		float num = Mathf.Clamp01((Singleton<GameController>.Instance.Player.Rb.linearVelocity.magnitude - screenShakeMinVelocity) / (screenShakeMaxVelocity - screenShakeMinVelocity));
		if (num > 0f)
		{
			followCameraPerlin.m_NoiseProfile = screenShakeNoiseSettings;
			followCameraPerlin.m_AmplitudeGain = screenShakeMaxAmplitude * num;
			followCameraPerlin.m_FrequencyGain = screenShakeFrequency;
			screenShakeTimer = screenShakeDuration;
		}
	}

	public void ResetFollowCamera()
	{
		StartCoroutine(ResetFollowCameraCoroutine());
	}

	public static void StartDialogue()
	{
		Singleton<CameraController>.Instance.FollowCameraFollowTarget.SetFollowType(FollowTarget.FollowType.Dialogue);
		AddDialogueCameraUser();
	}

	public static void EndDialogue()
	{
		Singleton<CameraController>.Instance.FollowCameraFollowTarget.SetFollowType(FollowTarget.FollowType.Spider);
		RemoveDialogueCameraUser();
	}

	public void PreventCameraMovement(float duration)
	{
		StartCoroutine(PreventCameraMovementCoroutine(duration));
	}

	public void EnableWardrobeCamera()
	{
		Singleton<CameraController>.Instance.mainCamera.gameObject.SetActive(value: false);
		wardrobeCamera.gameObject.SetActive(value: true);
		wardrobeCamera.transform.position = Singleton<WardrobeIsland>.Instance.WardrobeCameraPosition.position;
		wardrobeCamera.transform.rotation = Singleton<WardrobeIsland>.Instance.WardrobeCameraPosition.rotation;
	}

	public void DisableWardrobeCamera()
	{
		Singleton<CameraController>.Instance.mainCamera.gameObject.SetActive(value: true);
		wardrobeCamera.gameObject.SetActive(value: false);
	}

	public float GetCameraDistance()
	{
		return Vector3.Distance(followCameraMouseLook.transform.position, Singleton<CameraController>.Instance.mainCamera.transform.position);
	}

	public void OnRespawn()
	{
		StartCoroutine(OnRespawnCoroutine());
	}

	public void ToggleFreeLookCamera()
	{
		if (cinemachineFreeLookCamera.gameObject.activeSelf)
		{
			cinemachineFreeLookCamera.gameObject.SetActive(value: false);
			Singleton<GameController>.Instance.Player.gameObject.SetActive(value: true);
			Singleton<GameController>.Instance.State = GameController.GameState.Running;
			Singleton<WebController>.Instance.ShowWebTarget(value: true);
		}
		else
		{
			cinemachineFreeLookCamera.gameObject.SetActive(value: true);
			Singleton<GameController>.Instance.Player.gameObject.SetActive(showPlayerInFreeLookCamera);
			Singleton<GameController>.Instance.State = GameController.GameState.FreeLook;
			Singleton<WebController>.Instance.ShowWebTarget(value: false);
		}
	}

	public void SetCinemachineFollowCameraFOV(float value)
	{
		if (Singleton<CameraController>.Instance.cinemachineFollowCamera != null)
		{
			Singleton<CameraController>.Instance.cinemachineFollowCamera.m_Lens.FieldOfView = value;
			if (cinemachineBrain != null && Time.timeScale == 0f)
			{
				CinemachineBrain.UpdateMethod updateMethod = cinemachineBrain.m_UpdateMethod;
				cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.ManualUpdate;
				cinemachineBrain.ManualUpdate();
				cinemachineBrain.m_UpdateMethod = updateMethod;
			}
		}
		if (Singleton<CameraController>.Instance.dialogueCamera != null)
		{
			Singleton<CameraController>.Instance.dialogueCamera.fieldOfView = value;
		}
	}

	public void SetCinemachineFreeLookCameraFOV(float value)
	{
		cinemachineFreeLookCamera.m_Lens.FieldOfView = value;
	}

	public void SetCameraZoom(float value)
	{
		cameraZoom.SetZoom(value);
	}

	public void SetMinCameraZoom(float value)
	{
		cameraZoom.SetMinZoom(value);
	}

	public void SetMaxCameraZoom(float value)
	{
		cameraZoom.SetMaxZoom(value);
	}

	public int MobileZoom()
	{
		return cameraZoom.MobileZoom();
	}

	public int GetZoomArrayLength()
	{
		return cameraZoom.zoomArrayLength;
	}

	public void SetShoulderOffset(Vector3 offset)
	{
		shoulderOffset = offset;
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		UpdateSMAA();
		UpdateFieldOfView();
	}

	private void GameController_OnContinueGame(object sender, EventArgs e)
	{
		canMoveCamera = true;
	}

	private static void SpiderInteraction_OnInteractableAdded(object sender, EventArgs e)
	{
		AddDialogueCameraUser();
	}

	private static void SpiderInteraction_OnInteractableRemoved(object sender, EventArgs e)
	{
		RemoveDialogueCameraUser();
	}

	private void OnFreeLookCamera(InputAction.CallbackContext ctx)
	{
	}
}
