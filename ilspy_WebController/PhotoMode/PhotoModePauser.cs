using UnityEngine;
using UnityEngine.InputSystem;

namespace PhotoMode;

public class PhotoModePauser : MonoBehaviour
{
	[SerializeField]
	private bool pauseActionActivation;

	private PlayerInput playerInput;

	private PhotoModeInputs photoModeInputs;

	private PhotoMode photoModeBehaviors;

	private PhotoModeStickerController stickerController;

	private bool gamePaused;

	private void Awake()
	{
		if (Object.FindObjectOfType<PlayerInput>() != null)
		{
			playerInput = Object.FindObjectOfType<PlayerInput>();
		}
		photoModeInputs = GetComponent<PhotoModeInputs>();
		photoModeBehaviors = GetComponent<PhotoMode>();
		stickerController = GetComponentInChildren<PhotoModeStickerController>();
		if (pauseActionActivation)
		{
			photoModeInputs.PauseEvent.AddListener(PauseAction);
		}
	}

	public void PauseAction()
	{
		PauseGame(!gamePaused);
	}

	private void PauseGame(bool pause)
	{
		if (stickerController.IsActive())
		{
			stickerController.ToggleStickerMode(status: false);
			return;
		}
		float num = 1f;
		if (pause)
		{
			num = Time.deltaTime;
		}
		gamePaused = pause;
		Time.timeScale = (gamePaused ? 0f : num);
		photoModeBehaviors.Activate(pause);
		if ((bool)playerInput)
		{
			playerInput.enabled = !pause;
		}
	}
}
