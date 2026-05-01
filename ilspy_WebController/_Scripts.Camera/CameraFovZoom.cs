using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.Camera;

public class CameraFovZoom : MonoBehaviour
{
	[SerializeField]
	private CinemachineVirtualCamera virtualCamera;

	[SerializeField]
	private float fovZoomSpeed;

	[SerializeField]
	private float minFovZoom;

	[SerializeField]
	private float maxFovZoom;

	[SerializeField]
	private float smoothness;

	[SerializeField]
	private InputActionReference zoomInputAction;

	private float fovZoom;

	private float[] zoomArray;

	private int zoomIndex;

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
		fovZoom = virtualCamera.m_Lens.FieldOfView;
		zoomArray = new float[4]
		{
			minFovZoom,
			minFovZoom + (maxFovZoom - minFovZoom) / 3f,
			minFovZoom + 2f * (maxFovZoom - minFovZoom) / 3f,
			maxFovZoom
		};
		zoomIndex = SaveController.Load("FovZoomIndex", 0, SaveData.Settings);
		fovZoom = SaveController.Load("FovZoom", virtualCamera.m_Lens.FieldOfView, SaveData.Settings);
		virtualCamera.m_Lens.FieldOfView = fovZoom;
	}

	private void Update()
	{
		virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, fovZoom, 1f / (1f + smoothness));
	}

	private void OnZoom(InputAction.CallbackContext ctx)
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Paused || Singleton<GameController>.Instance.State == GameController.GameState.Cutscene || Singleton<GameController>.Instance.State == GameController.GameState.LevelFinished || (CinemachineVirtualCamera)Singleton<CameraController>.Instance.Brain.ActiveVirtualCamera != virtualCamera)
		{
			return;
		}
		if (Singleton<GameController>.Instance.InputIsKeyboardMouse)
		{
			fovZoom -= ctx.ReadValue<float>() * fovZoomSpeed;
			fovZoom = Mathf.Clamp(fovZoom, minFovZoom, maxFovZoom);
		}
		else if (ctx.ReadValueAsButton())
		{
			zoomIndex++;
			if (zoomIndex > zoomArray.Length - 1)
			{
				zoomIndex = 0;
			}
			fovZoom = zoomArray[zoomIndex];
		}
		SaveController.Save("FovZoom", fovZoom, SaveData.Settings);
		SaveController.Save("FovZoomIndex", zoomIndex, SaveData.Settings);
	}
}
