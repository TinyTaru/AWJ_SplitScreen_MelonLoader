using UnityEngine;

namespace PhotoMode;

public class InfoBarController : MonoBehaviour
{
	[SerializeField]
	private GameObject photoModeMenus;

	[SerializeField]
	private CanvasGroup photoModeMenusCanvas;

	[SerializeField]
	private GameObject defaultInfoBarKeyboard;

	[SerializeField]
	private GameObject stickerInfoBarKeyboard;

	[SerializeField]
	private GameObject defaultInfoBarGamepad;

	[SerializeField]
	private GameObject stickerInfoBarGamepad;

	[SerializeField]
	private CustomInputProvider photoModeCameraInputProvider;

	private bool stickerModeActive;

	private bool keyboardModeActive = true;

	private void Awake()
	{
		photoModeMenusCanvas = photoModeMenus.GetComponent<CanvasGroup>();
	}

	public void StickerModeActivation(bool active)
	{
		stickerModeActive = active;
		photoModeMenusCanvas.interactable = !active;
		photoModeCameraInputProvider.active = !active;
		if (keyboardModeActive)
		{
			defaultInfoBarKeyboard.SetActive(!active);
			stickerInfoBarKeyboard.SetActive(active);
			defaultInfoBarGamepad.SetActive(value: false);
			stickerInfoBarGamepad.SetActive(value: false);
		}
		else
		{
			defaultInfoBarGamepad.SetActive(!active);
			stickerInfoBarGamepad.SetActive(active);
			defaultInfoBarKeyboard.SetActive(value: false);
			stickerInfoBarKeyboard.SetActive(value: false);
		}
	}

	public void SetKeyboardModeActive(bool active)
	{
		keyboardModeActive = active;
		StickerModeActivation(stickerModeActive);
	}
}
