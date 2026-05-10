using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.Singletons;

namespace _Scripts.Utils;

public class Screenshot : MonoBehaviour
{
	[SerializeField]
	private int superSize = 1;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference screenshotInputAction;

	[SerializeField]
	private InputActionReference screenshotUIInputAction;

	[SerializeField]
	private InputActionReference screenshotPhotoModeInputAction;

	private void OnEnable()
	{
		screenshotInputAction.action.performed += OnScreenshot;
		screenshotUIInputAction.action.performed += OnScreenshot;
		screenshotPhotoModeInputAction.action.performed += OnScreenshot;
	}

	private void OnDisable()
	{
		screenshotInputAction.action.performed -= OnScreenshot;
		screenshotUIInputAction.action.performed -= OnScreenshot;
		screenshotPhotoModeInputAction.action.performed -= OnScreenshot;
	}

	private void TakeScreenshot()
	{
	}

	private void OnScreenshot(InputAction.CallbackContext ctx)
	{
		if ((!(Singleton<GameController>.Instance != null) || Singleton<GameController>.Instance.State != GameController.GameState.Debugging) && ctx.ReadValueAsButton())
		{
			TakeScreenshot();
		}
	}
}
