using System;
using MoreMountains.Tools;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using _Scripts.Mobile;
using _Scripts.Spider;
using _Scripts.UI.MobileMonetization;

namespace _Scripts.Singletons;

public class MobileControls : Singleton<MobileControls>
{
	public class OnShowButtonsEventArgs : EventArgs
	{
		public bool showButtons;
	}

	[Header("References")]
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private GameObject safeArea;

	[SerializeField]
	private DragArea dragArea;

	[Header("Buttons")]
	[SerializeField]
	private Color buttonBackgroundColor;

	[SerializeField]
	private Sprite buttonBackgroundSprite;

	[SerializeField]
	private Image[] buttonBackgroundImages;

	[SerializeField]
	private CanvasGroup[] buttonCanvasGroups;

	[SerializeField]
	private MMTouchButton[] mmTouchButtons;

	[SerializeField]
	private MMTouchButton pauseButton;

	[SerializeField]
	private MMTouchButton emoteButton;

	[SerializeField]
	private MMTouchButton taskListButton;

	[SerializeField]
	private MMTouchButton toggleWebButton;

	[SerializeField]
	private MMTouchButton interactButton;

	[SerializeField]
	private MMTouchButton jumpButton;

	[SerializeField]
	private MMTouchButton quickBuildButton;

	[SerializeField]
	private MMTouchButton deleteWebButton;

	[SerializeField]
	private MMTouchButton movingAnchorButton;

	[SerializeField]
	private MMTouchButton fixedAnchorButton;

	[SerializeField]
	private MMTouchButton zoomButton;

	[SerializeField]
	private Image mobileJoystickButtonIconImage;

	[SerializeField]
	private Image toggleWebButtonIconImage;

	[SerializeField]
	private Image deleteWebButtonIconImage;

	[SerializeField]
	private Image zoomButtonIconImage;

	[Header("Joysticks")]
	[SerializeField]
	private MMTouchRepositionableJoystick moveJoystick;

	[SerializeField]
	private CanvasGroup moveJoystickKnobCanvasGroup;

	[SerializeField]
	[Range(0f, 1f)]
	private float moveJoystickDeadZone = 0.2f;

	[SerializeField]
	private MMTouchJoystick mobileJoystick;

	[SerializeField]
	private RectTransform mobileJoystickBackground;

	[SerializeField]
	[Range(0f, 1f)]
	private float mobileJoystickDeadZone = 0.2f;

	[Header("Button Activation Indication")]
	[SerializeField]
	private GameObject toggleWebActiveGfx;

	[SerializeField]
	private GameObject shootWebActiveGfx;

	[SerializeField]
	private GameObject movingAnchorActiveIcon;

	[SerializeField]
	private GameObject movingAnchorInactiveIcon;

	[SerializeField]
	private GameObject fixedAnchorActiveIcon;

	[SerializeField]
	private GameObject fixedAnchorInactiveIcon;

	[Header("Sprites")]
	[SerializeField]
	private Sprite shootWebSprite;

	[SerializeField]
	private Sprite buildWebSprite;

	[SerializeField]
	private Sprite deleteWebSprite;

	[SerializeField]
	private Sprite cancelWebSprite;

	[SerializeField]
	private Sprite toggleWebSprite;

	[SerializeField]
	private Sprite zoomInSprite;

	[SerializeField]
	private Sprite zoomOutSprite;

	private GameController gameController;

	private BodyMovement player;

	private WebController webController;

	private CameraController cameraController;

	private SpiderInteraction spiderInteraction;

	private Image pauseButtonImage;

	private Image taskListButtonImage;

	private Image emoteButtonImage;

	private Image interactButtonImage;

	private Image jumpButtonImage;

	private Image quickBuildButtonImage;

	private Image movingAnchorButtonImage;

	private Image fixedAnchorButtonImage;

	private Image toggleWebButtonImage;

	private Image deleteWebButtonImage;

	private Image mobileJoystickButtonImage;

	private bool showInteractButton;

	private float moveJoystickMaxRange;

	private float mobileJoystickMaxRange;

	private bool multiTouch;

	private bool showButtons;

	private bool showButtonsOld;

	private float safeAreaFactor;

	private bool mobileJoystickActive;

	private bool advancedBuildingModeUnlocked;

	private bool quickBuildUnlocked;

	private Vector2 mobileJoystickDefaultPosition;

	private static bool dialogueActive;

	public event EventHandler<OnShowButtonsEventArgs> OnShowButtons;

	protected override void Awake()
	{
		base.Awake();
		Singleton<MobileControls>.Instance.moveJoystickMaxRange = Singleton<MobileControls>.Instance.moveJoystick.MaxRange;
		Singleton<MobileControls>.Instance.mobileJoystickMaxRange = Singleton<MobileControls>.Instance.mobileJoystick.MaxRange;
		Singleton<MobileControls>.Instance.safeAreaFactor = Screen.safeArea.height / (float)Screen.height;
		Singleton<MobileControls>.Instance.mobileJoystickDefaultPosition = Singleton<MobileControls>.Instance.mobileJoystickBackground.anchoredPosition;
		Singleton<MobileControls>.Instance.mobileJoystickActive = true;
	}

	private void Start()
	{
		Singleton<MobileControls>.Instance.canvas.gameObject.SetActive(value: false);
		if (Singleton<GameController>.Instance == null)
		{
			Debug.LogError("No GameController in the scene!");
		}
		else
		{
			Singleton<MobileControls>.Instance.gameController = Singleton<GameController>.Instance;
			Singleton<MobileControls>.Instance.gameController.OnPauseGame += GameController_OnPauseGame;
			Singleton<MobileControls>.Instance.gameController.OnContinueGame += GameController_OnContinueGame;
			Singleton<MobileControls>.Instance.gameController.OnOpenWardrobeDirectly += GameController_OnOpenWardrobeDirectly;
			Singleton<MobileControls>.Instance.player = Singleton<MobileControls>.Instance.gameController.Player;
			Singleton<MobileControls>.Instance.spiderInteraction = Singleton<MobileControls>.Instance.player.GetComponentInChildren<SpiderInteraction>();
		}
		if (Singleton<WebController>.Instance == null)
		{
			Debug.LogError("No WebController in the scene!");
		}
		else
		{
			Singleton<MobileControls>.Instance.webController = Singleton<WebController>.Instance;
			Singleton<MobileControls>.Instance.webController.OnModeChanged += WebController_OnModeChanged;
		}
		if (Singleton<CameraController>.Instance == null)
		{
			Debug.LogError("No CameraController in the scene!");
		}
		else
		{
			Singleton<MobileControls>.Instance.cameraController = Singleton<CameraController>.Instance;
		}
		Singleton<MobileControls>.Instance.pauseButtonImage = pauseButton.GetComponent<Image>();
		Singleton<MobileControls>.Instance.emoteButtonImage = emoteButton.GetComponent<Image>();
		Singleton<MobileControls>.Instance.taskListButtonImage = taskListButton.GetComponent<Image>();
		Singleton<MobileControls>.Instance.interactButtonImage = interactButton.GetComponent<Image>();
		Singleton<MobileControls>.Instance.jumpButtonImage = jumpButton.GetComponent<Image>();
		Singleton<MobileControls>.Instance.quickBuildButtonImage = quickBuildButton.GetComponent<Image>();
		Singleton<MobileControls>.Instance.movingAnchorButtonImage = movingAnchorButton.GetComponent<Image>();
		Singleton<MobileControls>.Instance.fixedAnchorButtonImage = fixedAnchorButton.GetComponent<Image>();
		Singleton<MobileControls>.Instance.toggleWebButtonImage = toggleWebButton.GetComponent<Image>();
		Singleton<MobileControls>.Instance.deleteWebButtonImage = deleteWebButton.GetComponent<Image>();
		Singleton<MobileControls>.Instance.mobileJoystickButtonImage = mobileJoystick.GetComponent<Image>();
		Singleton<MobileControls>.Instance.toggleWebButton.ReturnToInitialSpriteAutomatically = false;
		Singleton<MobileControls>.Instance.quickBuildButton.ReturnToInitialSpriteAutomatically = false;
		Singleton<MobileControls>.Instance.deleteWebButton.ReturnToInitialSpriteAutomatically = false;
		Singleton<MobileControls>.Instance.showInteractButton = false;
		Singleton<MobileControls>.Instance.showButtons = true;
		Singleton<MobileControls>.Instance.advancedBuildingModeUnlocked = SceneManager.GetActiveScene().name != "Level_Tutorial";
		Singleton<MobileControls>.Instance.quickBuildUnlocked = SceneManager.GetActiveScene().name != "Level_Tutorial";
		if (!Singleton<SceneController>.Instance.IsStoryLevel)
		{
			SetTaskListButtonActive(value: false);
		}
		if (Singleton<PlayTimeCanvas>.Instance != null)
		{
			Singleton<PlayTimeCanvas>.Instance.Opened += PlayTimeCanvas_OnOpened;
			Singleton<PlayTimeCanvas>.Instance.Closed += PlayTimeCanvas_OnClosed;
		}
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		UpdateUI();
		UpdateButtonTransparency();
	}

	private void Update()
	{
		bool active = Singleton<MobileControls>.Instance.showInteractButton || DialogueManager.instance.activeConversation != null;
		Singleton<MobileControls>.Instance.interactButton.gameObject.SetActive(active);
		bool flag = Singleton<MobileControls>.Instance.mobileJoystickActive;
		if (Singleton<MobileControls>.Instance.mobileJoystick.gameObject.activeSelf != flag)
		{
			Singleton<MobileControls>.Instance.cameraController.FollowCameraMouseLook.MobileJoystickLook(Vector2.zero);
			Singleton<MobileControls>.Instance.mobileJoystick.ResetJoystick();
		}
		Singleton<MobileControls>.Instance.mobileJoystick.gameObject.SetActive(flag);
		Singleton<MobileControls>.Instance.mobileJoystickBackground.gameObject.SetActive(flag);
		Singleton<MobileControls>.Instance.toggleWebActiveGfx.SetActive(Singleton<MobileControls>.Instance.webController.WebActive);
		Singleton<MobileControls>.Instance.shootWebActiveGfx.SetActive(Singleton<MobileControls>.Instance.webController.WebActive);
		Singleton<MobileControls>.Instance.movingAnchorActiveIcon.SetActive(Singleton<MobileControls>.Instance.webController.MovingAnchorActive);
		Singleton<MobileControls>.Instance.movingAnchorInactiveIcon.SetActive(!Singleton<MobileControls>.Instance.webController.MovingAnchorActive);
		Singleton<MobileControls>.Instance.fixedAnchorActiveIcon.SetActive(Singleton<MobileControls>.Instance.webController.FixedAnchorActive);
		Singleton<MobileControls>.Instance.fixedAnchorInactiveIcon.SetActive(!Singleton<MobileControls>.Instance.webController.FixedAnchorActive);
	}

	private void OnDestroy()
	{
		if (Singleton<PlayTimeCanvas>.Instance != null)
		{
			Singleton<PlayTimeCanvas>.Instance.Opened -= PlayTimeCanvas_OnOpened;
			Singleton<PlayTimeCanvas>.Instance.Closed -= PlayTimeCanvas_OnClosed;
		}
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
	}

	private void UpdateButtonBackground()
	{
		Image[] array = buttonBackgroundImages;
		foreach (Image obj in array)
		{
			obj.sprite = buttonBackgroundSprite;
			obj.color = buttonBackgroundColor;
		}
	}

	private void UpdateButtonTransparency()
	{
		float num = 1f - SettingsController.ButtonTransparency;
		CanvasGroup[] array = Singleton<MobileControls>.Instance.buttonCanvasGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].alpha = num;
		}
		MMTouchButton[] array2 = mmTouchButtons;
		foreach (MMTouchButton obj in array2)
		{
			obj.IdleOpacity = num;
			obj.PressedOpacity = num * 0.5f;
		}
	}

	private void UpdateUI()
	{
		if (!(Singleton<MobileControls>.Instance == null) && !(Singleton<MobileControls>.Instance.canvas == null))
		{
			Singleton<MobileControls>.Instance.canvas.gameObject.SetActive(value: false);
		}
	}

	private void ShowMobileUI()
	{
		Singleton<MobileControls>.Instance.canvas.gameObject.SetActive(value: true);
		Singleton<MobileControls>.Instance.dragArea.Reset();
		UpdateUI();
	}

	private static void HideMobileUI()
	{
		Singleton<MobileControls>.Instance.canvas.gameObject.SetActive(value: false);
		Singleton<MobileControls>.Instance.dragArea.Reset();
	}

	public static void StartDialogue()
	{
		dialogueActive = true;
		Singleton<MobileControls>.Instance.UpdateUI();
	}

	public static void EndDialogue()
	{
		dialogueActive = false;
		Singleton<MobileControls>.Instance.UpdateUI();
	}

	public void DisableButtonsForTutorialStart()
	{
		SetTaskListButtonActive(value: false);
		SetAdvancedWebBuildingButtonsActive(value: false);
		SetToggleWebButtonActive(value: false);
		SetQuickBuildButtonActive(value: false);
		SetMobileJoystickActive(value: false);
		SetDeleteWebButtonActive(value: false);
		Singleton<MobileControls>.Instance.advancedBuildingModeUnlocked = false;
		Singleton<MobileControls>.Instance.quickBuildUnlocked = false;
	}

	public void MobileMove(Vector2 value)
	{
	}

	public void ResetMobileMove()
	{
	}

	public void MobileLook(Vector2 value)
	{
	}

	public void MobileShootLook(Vector2 value)
	{
	}

	public void ResetMobileLook()
	{
	}

	public void Jump(bool value)
	{
	}

	public void ShootWeb(bool value)
	{
	}

	public void ToggleWeb()
	{
	}

	public void QuickBuild()
	{
	}

	public void FixedAnchor()
	{
	}

	public void MovingAnchor()
	{
	}

	public void DeleteWeb()
	{
	}

	public void DeleteWebButtonReleased()
	{
	}

	public void Interact()
	{
	}

	public void Pause()
	{
	}

	public void Emote(bool value)
	{
	}

	public void WebColor(bool value)
	{
	}

	public void TaskList(bool value)
	{
	}

	public void Sprint(bool value)
	{
	}

	public void Zoom()
	{
	}

	public void OnMoveJoystickDown(bool value)
	{
		bool raycastTarget = !value || Singleton<MobileControls>.Instance.multiTouch;
		Singleton<MobileControls>.Instance.emoteButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.taskListButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.toggleWebButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.pauseButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.movingAnchorButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.fixedAnchorButtonImage.raycastTarget = raycastTarget;
	}

	public void OnMobileJoystickDown(bool value)
	{
		Singleton<MobileControls>.Instance.mobileJoystick.SetNeutralPosition(Singleton<MobileControls>.Instance.mobileJoystickBackground.position);
		bool raycastTarget = !value || Singleton<MobileControls>.Instance.multiTouch;
		Singleton<MobileControls>.Instance.jumpButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.quickBuildButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.deleteWebButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.interactButtonImage.raycastTarget = raycastTarget;
	}

	public void OnDragAreaDown(bool value)
	{
		bool raycastTarget = !value || Singleton<MobileControls>.Instance.multiTouch;
		Singleton<MobileControls>.Instance.mobileJoystickButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.jumpButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.quickBuildButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.deleteWebButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.interactButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.fixedAnchorButtonImage.raycastTarget = raycastTarget;
		Singleton<MobileControls>.Instance.movingAnchorButtonImage.raycastTarget = raycastTarget;
		if (!value)
		{
			Singleton<MobileControls>.Instance.gameController.ContinueDialogue();
		}
	}

	public void SetEmoteButtonActive(bool value)
	{
		Singleton<MobileControls>.Instance.emoteButton.gameObject.SetActive(value);
	}

	public void SetTaskListButtonActive(bool value)
	{
		Singleton<MobileControls>.Instance.taskListButton.gameObject.SetActive(value);
	}

	public void SetToggleWebButtonActive(bool value)
	{
		Singleton<MobileControls>.Instance.toggleWebButton.gameObject.SetActive(value);
	}

	public void SetQuickBuildButtonActive(bool value)
	{
		if (value)
		{
			Singleton<MobileControls>.Instance.quickBuildUnlocked = true;
		}
		if (Singleton<SettingsController>.Instance != null)
		{
			bool alternativeQuickBuildButton = SettingsController.AlternativeQuickBuildButton;
			Singleton<MobileControls>.Instance.quickBuildButton.gameObject.SetActive(value && !alternativeQuickBuildButton);
		}
	}

	public void SetDeleteWebButtonActive(bool value)
	{
		Singleton<MobileControls>.Instance.deleteWebButton.gameObject.SetActive(value);
	}

	public void SetMobileJoystickActive(bool value)
	{
		Singleton<MobileControls>.Instance.mobileJoystickActive = value;
	}

	public void SetAdvancedWebBuildingButtonsActive(bool value)
	{
		if (value)
		{
			Singleton<MobileControls>.Instance.advancedBuildingModeUnlocked = true;
		}
		Singleton<MobileControls>.Instance.fixedAnchorButton.gameObject.SetActive(value);
		Singleton<MobileControls>.Instance.movingAnchorButton.gameObject.SetActive(value);
	}

	public void ShowInteractButton(bool value)
	{
		Singleton<MobileControls>.Instance.showInteractButton = value;
	}

	public void ShowButtons(bool value)
	{
		Singleton<MobileControls>.Instance.showButtons = value;
		UpdateUI();
		Singleton<MobileControls>.Instance.showButtonsOld = value;
		Singleton<MobileControls>.Instance.OnShowButtons?.Invoke(this, new OnShowButtonsEventArgs
		{
			showButtons = value
		});
	}

	private void WebController_OnModeChanged(object sender, EventArgs e)
	{
		if (Singleton<MobileControls>.Instance.webController.WebAnchorActive)
		{
			Singleton<MobileControls>.Instance.toggleWebButtonIconImage.sprite = buildWebSprite;
			Singleton<MobileControls>.Instance.mobileJoystickButtonIconImage.sprite = buildWebSprite;
			Singleton<MobileControls>.Instance.deleteWebButtonIconImage.sprite = cancelWebSprite;
		}
		else
		{
			Singleton<MobileControls>.Instance.toggleWebButtonIconImage.sprite = toggleWebSprite;
			Singleton<MobileControls>.Instance.mobileJoystickButtonIconImage.sprite = shootWebSprite;
			Singleton<MobileControls>.Instance.deleteWebButtonIconImage.sprite = deleteWebSprite;
		}
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		UpdateUI();
		UpdateButtonTransparency();
	}

	private void GameController_OnContinueGame(object sender, EventArgs e)
	{
		ShowMobileUI();
	}

	private void GameController_OnPauseGame(object sender, EventArgs e)
	{
		HideMobileUI();
	}

	private void GameController_OnOpenWardrobeDirectly(object sender, EventArgs e)
	{
		Singleton<MobileControls>.Instance.canvas.gameObject.SetActive(value: false);
		Singleton<MobileControls>.Instance.dragArea.Reset();
	}

	private void PlayTimeCanvas_OnOpened()
	{
		HideMobileUI();
	}

	private void PlayTimeCanvas_OnClosed()
	{
		ShowMobileUI();
	}
}
