using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.MobileMonetization;
using _Scripts.Utils;

namespace _Scripts.Camera;

public class CameraZoom : MonoBehaviour
{
	[SerializeField]
	private CinemachineVirtualCamera virtualCamera;

	[SerializeField]
	private float zoomSpeed;

	[SerializeField]
	private float minZoom;

	[SerializeField]
	private float maxZoom;

	[SerializeField]
	private float smoothness;

	[SerializeField]
	private float mobileSmoothness;

	[SerializeField]
	private float mobileMaxZoom = 40f;

	[SerializeField]
	private int zoomSteps = 6;

	[SerializeField]
	private bool zoomInWhenLookingUp;

	[SerializeField]
	private AnimatorUpdateMode updateMode;

	[Header("Auto Zoom Settings")]
	[SerializeField]
	private float minSpeed;

	[SerializeField]
	private float maxSpeed;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference zoomInputAction;

	private float zoom = 10f;

	private Cinemachine3rdPersonFollow follow;

	private float[] zoomArray;

	private int zoomIndex;

	private static readonly int ShaderCameraZoom = Shader.PropertyToID("CameraZoom");

	private float cameraDistance;

	private float finalCameraDistance;

	public int zoomArrayLength => zoomArray.Length;

	private void OnEnable()
	{
		zoomInputAction.action.performed += OnZoom;
	}

	private void OnDisable()
	{
		zoomInputAction.action.performed -= OnZoom;
	}

	private void Start()
	{
		follow = (Cinemachine3rdPersonFollow)virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
		zoomArray = new float[zoomSteps];
		for (int i = 0; i < zoomSteps; i++)
		{
			zoomArray[i] = minZoom + (float)i * (maxZoom - minZoom) / (float)(zoomSteps - 1);
		}
		zoomIndex = SaveController.Load("FollowCameraZoomIndex", 0, SaveData.Settings);
		zoom = SaveController.Load("FollowCameraZoom", follow.CameraDistance, SaveData.Settings);
		follow.CameraDistance = zoom;
	}

	private void Update()
	{
		if (updateMode == AnimatorUpdateMode.Normal)
		{
			HandleCameraZoom(Time.deltaTime);
		}
	}

	private void FixedUpdate()
	{
		if (updateMode == AnimatorUpdateMode.Fixed)
		{
			HandleCameraZoom(Time.fixedDeltaTime);
		}
	}

	private void HandleCameraZoom(float deltaTime)
	{
		BodyMovement player = Singleton<GameController>.Instance.Player;
		if (SettingsController.AutoZoom)
		{
			zoom = minZoom + player.Rb.linearVelocity.magnitude * 1f;
			zoom = Mathf.Min(zoom, maxZoom);
		}
		cameraDistance = _Scripts.Utils.Utils.ExponentialDecay(cameraDistance, zoom, 5f, deltaTime);
		if (zoomInWhenLookingUp && player.State == BodyMovement.MovementState.Walking && !player.WebTouched)
		{
			float num = Mathf.Max(Vector3.Dot(player.transform.up, base.transform.forward), 0f);
			finalCameraDistance = Mathf.Lerp(cameraDistance, 0f, num * 1.5f);
		}
		else
		{
			finalCameraDistance = cameraDistance;
		}
		follow.CameraDistance = finalCameraDistance;
		Shader.SetGlobalFloat(ShaderCameraZoom, finalCameraDistance);
	}

	public void SetZoom(float value)
	{
		zoom = value;
	}

	public void SetMinZoom(float value)
	{
		minZoom = value;
	}

	public void SetMaxZoom(float value)
	{
		maxZoom = value;
	}

	public int MobileZoom()
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Paused || Singleton<GameController>.Instance.State == GameController.GameState.Cutscene || Singleton<GameController>.Instance.State == GameController.GameState.LevelFinished || Singleton<GameController>.Instance.State == GameController.GameState.Debugging)
		{
			return -1;
		}
		if (Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			return -1;
		}
		if ((CinemachineVirtualCamera)Singleton<CameraController>.Instance.Brain.ActiveVirtualCamera != virtualCamera)
		{
			return -1;
		}
		zoomIndex++;
		if (zoomIndex > zoomArray.Length - 1)
		{
			zoomIndex = 0;
		}
		zoom = zoomArray[zoomIndex];
		SaveController.Save("FollowCameraZoom", zoom, SaveData.Settings);
		SaveController.Save("FollowCameraZoomIndex", zoomIndex, SaveData.Settings);
		return zoomIndex;
	}

	private void OnZoom(InputAction.CallbackContext ctx)
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Paused || Singleton<GameController>.Instance.State == GameController.GameState.Cutscene || Singleton<GameController>.Instance.State == GameController.GameState.LevelFinished || Singleton<GameController>.Instance.State == GameController.GameState.Debugging || Singleton<PlayTimeCanvas>.Instance.IsOpen || (CinemachineVirtualCamera)Singleton<CameraController>.Instance.Brain.ActiveVirtualCamera != virtualCamera)
		{
			return;
		}
		if (Singleton<GameController>.Instance.InputIsKeyboardMouse)
		{
			zoom -= ctx.ReadValue<Vector2>().y * zoomSpeed;
			zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
		}
		else if (ctx.ReadValueAsButton())
		{
			zoomIndex++;
			if (zoomIndex > zoomArray.Length - 1)
			{
				zoomIndex = 0;
			}
			zoom = zoomArray[zoomIndex];
		}
		SaveController.Save("FollowCameraZoom", zoom, SaveData.Settings);
		SaveController.Save("FollowCameraZoomIndex", zoomIndex, SaveData.Settings);
	}
}
