using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using _Scripts.Singletons;

namespace PhotoMode;

public class PhotoMode : MonoBehaviour
{
	private static PhotoMode instance;

	private EventSystem projectEventSystem;

	private CinemachineBrain projectCinemachineBrain;

	private CinemachineBrain.UpdateMethod project_cm_update;

	private CinemachineBlendDefinition.Style project_cm_blend;

	[Header("Character Reference")]
	[SerializeField]
	private GameObject photoModeCameraOrbit;

	private GameObject playerObject;

	private EventSystem photoModeEventSystem;

	private PhotoModeInputs photoModeInputs;

	private CinemachineFreeLook photoModeCamera;

	private CinemachineCameraOffset photoModeCameraOffset;

	private Transform photoModeUI;

	private GameObject photoModeFrame;

	private Volume photoModeVolume;

	private VolumeProfile photoModeVolumeProfile;

	private CanvasGroup photoModeMenusCanvas;

	private float photoModeCameraXAxis;

	private float photoModeCameraYAxis;

	[Header("UI References")]
	[SerializeField]
	private GameObject photoModeMenus;

	[SerializeField]
	private GameObject photoModeGrid;

	[SerializeField]
	private PhotoModeStickerController stickerController;

	[SerializeField]
	private Image photoModeVignette;

	[Header("Photo Mode Settings")]
	[SerializeField]
	private MinMax viewRoll = new MinMax(-90f, 90f);

	[SerializeField]
	private MinMax camDist = new MinMax(3f, -3f);

	[SerializeField]
	private MinMax focusDistance = new MinMax(0.1f, 20f);

	[SerializeField]
	private MinMax aperture = new MinMax(1f, 32f);

	[SerializeField]
	private MinMax exposure = new MinMax(-3f, 3f);

	[SerializeField]
	private MinMax contrast = new MinMax(-50f, 50f);

	[SerializeField]
	private MinMax saturation = new MinMax(-100f, 100f);

	[SerializeField]
	private MinMax vignette = new MinMax(-1f, 1f);

	[SerializeField]
	private MinMax verticalArm = new MinMax(-1f, 1f);

	[SerializeField]
	private float verticalArmSpeed = 2f;

	[Header("Filter Material")]
	[SerializeField]
	private Material postProcessingMaterial;

	[Header("Debug Behavior")]
	[SerializeField]
	private PhotoModeDebugger photoModeDebugger;

	private Shader unlitShader;

	private bool photoModeOn;

	private Color vignetteColor;

	private ColorAdjustments colorAdj;

	private DepthOfField dof;

	[Space]
	public PhotoModeEvent OnPhotoModeActivation;

	private void Awake()
	{
		instance = this;
		if (UnityEngine.Object.FindObjectOfType<CinemachineBrain>() != null)
		{
			projectCinemachineBrain = UnityEngine.Object.FindObjectOfType<CinemachineBrain>();
			project_cm_blend = projectCinemachineBrain.m_DefaultBlend.m_Style;
			project_cm_update = projectCinemachineBrain.m_UpdateMethod;
		}
		projectEventSystem = EventSystem.current;
		photoModeEventSystem = GetComponentInChildren<EventSystem>();
		photoModeInputs = GetComponent<PhotoModeInputs>();
		photoModeCamera = GetComponentInChildren<CinemachineFreeLook>();
		photoModeCameraOffset = photoModeCamera.GetComponent<CinemachineCameraOffset>();
		photoModeUI = base.transform.Find("PhotoMode_UI");
		photoModeVolume = GetComponentInChildren<Volume>();
		photoModeVolumeProfile = photoModeVolume.profile;
		photoModeVolumeProfile.TryGet<DepthOfField>(out dof);
		photoModeVolumeProfile.TryGet<ColorAdjustments>(out colorAdj);
		photoModeMenusCanvas = photoModeMenus.GetComponent<CanvasGroup>();
		if (projectEventSystem != photoModeEventSystem)
		{
			photoModeEventSystem.enabled = false;
		}
		photoModeInputs.ResetEvent.AddListener(ResetValues);
		photoModeInputs.ToggleGridEvent.AddListener(ToggleGrid);
		photoModeInputs.ToggleInterfaceEvent.AddListener(ToggleUI);
		photoModeInputs.SubmitEvent.AddListener(stickerController.StampSticker);
		photoModeInputs.SwapStickerEvent.AddListener(delegate
		{
			stickerController.ChangeStickerSprite(1);
		});
		photoModeInputs.DeleteStickerEvent.AddListener(stickerController.DeleteSticker);
		photoModeInputs.FlipStickerEvent.AddListener(delegate
		{
			stickerController.FlipSticker(reset: false);
		});
		unlitShader = Shader.Find("Unlit/Texture");
		if (playerObject == null && UnityEngine.Object.FindObjectOfType<PlayerInput>() != null)
		{
			playerObject = UnityEngine.Object.FindObjectOfType<PlayerInput>().gameObject;
		}
		if (playerObject != null)
		{
			photoModeCameraOrbit.transform.position = playerObject.transform.position;
		}
		DebugBehavior();
		if (photoModeUI.gameObject.activeSelf)
		{
			photoModeUI.gameObject.SetActive(value: false);
		}
	}

	private void Start()
	{
		playerObject = Singleton<GameController>.Instance.Player.gameObject;
	}

	private void Update()
	{
		if (stickerController.IsActive())
		{
			stickerController.MoveStickers(photoModeInputs.moveAxis);
			stickerController.RotateStickers(photoModeInputs.modifier1_value);
			stickerController.ScaleStickers(photoModeInputs.modifier2_value);
		}
		else if (photoModeOn)
		{
			CraneCamera(photoModeInputs.modifier2_value);
		}
	}

	public void Activate(bool active)
	{
		OnPhotoModeActivation.Invoke(active);
		if (active)
		{
			SetPhotoModeActive();
		}
		else
		{
			SetMainCameraActive();
		}
		if (photoModeCameraOrbit != null && playerObject != null)
		{
			photoModeCameraOrbit.transform.position = playerObject.transform.position;
		}
		photoModeUI.gameObject.SetActive(active);
		if (active)
		{
			projectEventSystem = EventSystem.current;
		}
		if (projectEventSystem != null && projectEventSystem != photoModeEventSystem)
		{
			projectEventSystem.enabled = !active;
		}
		photoModeEventSystem.enabled = active;
		ProjectCinemachineConfig(active);
		photoModeCameraXAxis = photoModeCamera.m_XAxis.Value;
		photoModeCameraYAxis = photoModeCamera.m_YAxis.Value;
		ResetValues();
		photoModeOn = active;
		photoModeCamera.Priority = ((!active) ? 1 : 99);
		if (active)
		{
			SetEventSystemSelectedObj(GetComponentInChildren<Slider>().gameObject);
		}
	}

	public void CraneCamera(float value)
	{
		if (value < 0f && photoModeCameraOffset.m_Offset.y > verticalArm.min)
		{
			photoModeCameraOffset.m_Offset.y -= verticalArmSpeed * Mathf.Abs(value) * Time.unscaledDeltaTime;
		}
		if (value > 0f && photoModeCameraOffset.m_Offset.y < verticalArm.max)
		{
			photoModeCameraOffset.m_Offset.y += verticalArmSpeed * Mathf.Abs(value) * Time.unscaledDeltaTime;
		}
	}

	public void ViewRoll(Slider slider)
	{
		photoModeCamera.m_Lens.Dutch = (slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (viewRoll.max - viewRoll.min) + viewRoll.min;
	}

	public void CameraDistance(Slider slider)
	{
		photoModeCameraOffset.m_Offset.z = (slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (camDist.max - camDist.min) + camDist.min;
	}

	public void FocusDistance(Slider slider)
	{
		dof.focusDistance.Override((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (focusDistance.max - focusDistance.min) + focusDistance.min);
	}

	public void Aperture(Slider slider)
	{
		dof.aperture.Override((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (aperture.max - aperture.min) + aperture.min);
	}

	public void Exposure(Slider slider)
	{
		colorAdj.postExposure.Override((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (exposure.max - exposure.min) + exposure.min);
	}

	public void Contrast(Slider slider)
	{
		colorAdj.contrast.Override((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (contrast.max - contrast.min) + contrast.min);
	}

	public void Saturation(Slider slider)
	{
		colorAdj.saturation.Override((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (saturation.max - saturation.min) + saturation.min);
	}

	public void Vignette(Slider slider)
	{
		vignetteColor = ((slider.value > 0f) ? Color.white : Color.black);
		vignetteColor.a = Mathf.Abs((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (vignette.max - vignette.min) + vignette.min);
		photoModeVignette.color = vignetteColor;
	}

	public void ChangeFilter(Shader shader)
	{
		postProcessingMaterial.shader = shader;
	}

	public void ChangeFrame(GameObject frame)
	{
		ResetFrame();
		photoModeFrame = frame;
		frame.gameObject.SetActive(value: true);
	}

	public void ResetFrame()
	{
		if (photoModeFrame != null)
		{
			photoModeFrame.SetActive(value: false);
		}
	}

	public void ToggleUI()
	{
		if (photoModeOn && !stickerController.IsActive())
		{
			photoModeMenusCanvas.alpha = ((photoModeMenusCanvas.alpha == 0f) ? 1 : 0);
		}
	}

	public void ToggleGrid()
	{
		if (photoModeOn && !stickerController.IsActive())
		{
			photoModeGrid.SetActive(!photoModeGrid.activeSelf);
		}
	}

	public void ResetValues()
	{
		if (!stickerController.IsActive())
		{
			photoModeCameraOffset.m_Offset = new Vector3(0f, 0f, Mathf.Ceil(camDist.max - MathF.Abs(camDist.min)) / 2f);
			photoModeCamera.m_XAxis.Value = photoModeCameraXAxis;
			photoModeCamera.m_YAxis.Value = photoModeCameraYAxis;
			Slider[] componentsInChildren = photoModeMenus.GetComponentsInChildren<Slider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].value = 0f;
			}
			if (photoModeMenusCanvas.alpha == 0f)
			{
				ToggleUI();
			}
			ChangeFilter(unlitShader);
			stickerController.ResetStickers();
			stickerController.ToggleStickerMode(status: false);
			if (EventSystem.current != null)
			{
				SetEventSystemSelectedObj(EventSystem.current.currentSelectedGameObject);
			}
		}
	}

	private void SetPhotoModeActive()
	{
		projectCinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
		projectCinemachineBrain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;
	}

	private void SetMainCameraActive()
	{
		projectCinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
		projectCinemachineBrain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.FixedUpdate;
	}

	private void SetEventSystemSelectedObj(GameObject obj)
	{
		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(obj);
	}

	private void OnApplicationQuit()
	{
		ResetValues();
	}

	private void DebugBehavior()
	{
		if (playerObject == null)
		{
			photoModeDebugger.PlayerAvailability(available: false);
		}
		if (photoModeVolume == null)
		{
			photoModeDebugger.VolumeAvailability(available: false);
		}
		if (photoModeCameraOffset == null)
		{
			photoModeDebugger.CameraOffsetAvailability(available: false);
		}
	}

	private void ProjectCinemachineConfig(bool active)
	{
		if (projectCinemachineBrain != null)
		{
			projectCinemachineBrain.m_IgnoreTimeScale = active;
			projectCinemachineBrain.m_UpdateMethod = (active ? CinemachineBrain.UpdateMethod.SmartUpdate : project_cm_update);
			StartCoroutine(BlendTimeReset());
		}
		IEnumerator BlendTimeReset()
		{
			float blendTime = projectCinemachineBrain.m_DefaultBlend.m_Time;
			projectCinemachineBrain.m_DefaultBlend.m_Time = 0f;
			projectCinemachineBrain.m_DefaultBlend.m_Style = ((!active) ? project_cm_blend : CinemachineBlendDefinition.Style.Cut);
			yield return new WaitForEndOfFrame();
			projectCinemachineBrain.m_DefaultBlend.m_Time = blendTime;
		}
	}

	public static void SetPlayerObject(GameObject player)
	{
		instance.playerObject = player;
		instance.photoModeCameraOrbit.transform.position = player.transform.position;
	}
}
