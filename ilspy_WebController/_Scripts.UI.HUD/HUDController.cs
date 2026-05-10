using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.UI.ContentFitting;

namespace _Scripts.UI.HUD;

public class HUDController : Singleton<HUDController>
{
	[Header("Parent")]
	[SerializeField]
	private GameObject overlay;

	[SerializeField]
	private GameObject controlsHUD;

	[SerializeField]
	private GameObject showControls;

	[SerializeField]
	private GameObject gamepadControls;

	[SerializeField]
	private GameObject keyboardControls;

	[SerializeField]
	private GameObject minimalisticTaskListControls;

	[SerializeField]
	private GameObject minimalisticCheatConsoleControls;

	[Header("Categories")]
	[SerializeField]
	private GameObject[] emoteControls;

	[SerializeField]
	private GameObject[] sprintControls;

	[SerializeField]
	private GameObject[] zoomControls;

	[SerializeField]
	private GameObject[] fixedAnchorControls;

	[SerializeField]
	private GameObject[] movingAnchorControls;

	[SerializeField]
	private GameObject[] shootWebControls;

	[SerializeField]
	private GameObject[] quickBuildControls;

	[SerializeField]
	private GameObject[] jumpControls;

	[SerializeField]
	private GameObject[] interactControls;

	[SerializeField]
	private GameObject[] taskListControls;

	[SerializeField]
	private GameObject[] deleteWebControls;

	[SerializeField]
	private GameObject[] stopBuildingControls;

	private bool controlsAreVisible;

	private ContentFitterRefresh contentFitterRefresh;

	private float refreshTimer;

	private readonly float refreshPeriod = 30f;

	private bool fullControlsAreActive;

	protected override void Awake()
	{
		base.Awake();
		contentFitterRefresh = GetComponent<ContentFitterRefresh>();
	}

	private void OnEnable()
	{
		if (Singleton<GameController>.Instance != null)
		{
			SetControlsType(Singleton<GameController>.Instance.InputIsKeyboardMouse);
			Singleton<GameController>.Instance.OnControlSchemeChanged += GameControllerOnControlSchemeChanged;
			Singleton<GameController>.Instance.OnToggleHUD += GameControllerOnToggleHUD;
			Singleton<GameController>.Instance.OnPauseGame += GameControllerOnPauseGame;
			Singleton<GameController>.Instance.OnContinueGame += GameControllerOnContinueGame;
		}
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		if (Singleton<MobileControls>.Instance != null)
		{
			Singleton<MobileControls>.Instance.OnShowButtons += MobileControls_OnShowButtons;
		}
	}

	private void Start()
	{
		UpdateOverlayActiveState();
		Invoke("RefreshContentFitters", 2f);
		refreshTimer = refreshPeriod;
	}

	private void OnDisable()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnControlSchemeChanged -= GameControllerOnControlSchemeChanged;
			Singleton<GameController>.Instance.OnToggleHUD -= GameControllerOnToggleHUD;
			Singleton<GameController>.Instance.OnPauseGame -= GameControllerOnPauseGame;
			Singleton<GameController>.Instance.OnContinueGame -= GameControllerOnContinueGame;
		}
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
	}

	private void Update()
	{
		refreshTimer -= Time.deltaTime;
		if (refreshTimer <= 0f)
		{
			refreshTimer = refreshPeriod;
			RefreshContentFitters();
		}
	}

	private void HideAllControls()
	{
		fullControlsAreActive = false;
		Singleton<HUDController>.Instance.controlsHUD.SetActive(value: false);
		Singleton<HUDController>.Instance.showControls.SetActive(value: false);
		Singleton<HUDController>.Instance.minimalisticTaskListControls.SetActive(value: false);
		Singleton<HUDController>.Instance.minimalisticCheatConsoleControls.SetActive(value: false);
	}

	private void ShowFullControls()
	{
		fullControlsAreActive = true;
		Singleton<HUDController>.Instance.controlsHUD.SetActive(Singleton<HUDController>.Instance.controlsAreVisible);
		Singleton<HUDController>.Instance.showControls.SetActive(!Singleton<HUDController>.Instance.controlsAreVisible);
		Singleton<HUDController>.Instance.minimalisticTaskListControls.SetActive(value: false);
		Singleton<HUDController>.Instance.minimalisticCheatConsoleControls.SetActive(value: false);
		RefreshContentFitters();
	}

	private void ShowMinimalisticTaskListControls()
	{
		fullControlsAreActive = false;
		Singleton<HUDController>.Instance.controlsHUD.SetActive(value: false);
		Singleton<HUDController>.Instance.showControls.SetActive(value: false);
		Singleton<HUDController>.Instance.minimalisticTaskListControls.SetActive(value: true);
		Singleton<HUDController>.Instance.minimalisticCheatConsoleControls.SetActive(value: false);
		RefreshContentFitters();
	}

	private void ShowMinimalisticCheatConsoleControls()
	{
		fullControlsAreActive = false;
		Singleton<HUDController>.Instance.controlsHUD.SetActive(value: false);
		Singleton<HUDController>.Instance.showControls.SetActive(value: false);
		Singleton<HUDController>.Instance.minimalisticTaskListControls.SetActive(value: false);
		Singleton<HUDController>.Instance.minimalisticCheatConsoleControls.SetActive(value: true);
		RefreshContentFitters();
	}

	private void ToggleControlsHUD()
	{
		Singleton<HUDController>.Instance.controlsAreVisible = !Singleton<HUDController>.Instance.controlsAreVisible;
		Singleton<HUDController>.Instance.controlsHUD.SetActive(Singleton<HUDController>.Instance.controlsAreVisible);
		Singleton<HUDController>.Instance.showControls.SetActive(!Singleton<HUDController>.Instance.controlsAreVisible);
		RefreshContentFitters();
	}

	private void SetControlsType(bool isKeyboardMouse)
	{
		Singleton<HUDController>.Instance.keyboardControls.SetActive(isKeyboardMouse);
		Singleton<HUDController>.Instance.gamepadControls.SetActive(!isKeyboardMouse);
	}

	private void SetControlsActive(GameObject[] controls, bool value)
	{
		foreach (GameObject gameObject in controls)
		{
			if ((bool)gameObject && gameObject.activeSelf != value)
			{
				gameObject.SetActive(value);
				if (value)
				{
					float duration = 1f;
					float strength = 1f;
					int vibrato = 10;
					gameObject.transform.DOShakeScale(duration, strength, vibrato).OnComplete(Singleton<HUDController>.Instance.RefreshContentFitters);
				}
			}
		}
	}

	private void RefreshContentFitters()
	{
		if (!(contentFitterRefresh == null))
		{
			contentFitterRefresh.RefreshContentFitters();
		}
	}

	private void UpdateOverlayActiveState()
	{
		bool eventMode = SettingsController.EventMode;
		bool flag = SceneManager.GetActiveScene().name.ToUpper().Contains("TUTORIAL");
		if (eventMode)
		{
			Singleton<HUDController>.Instance.controlsAreVisible = true;
			ShowFullControls();
			return;
		}
		if (flag)
		{
			switch (SettingsController.ShowControls)
			{
			case ControlOverlay.None:
				HideAllControls();
				break;
			case ControlOverlay.TaskList:
			case ControlOverlay.Full:
				ShowFullControls();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return;
		}
		switch (SettingsController.ShowControls)
		{
		case ControlOverlay.None:
			HideAllControls();
			break;
		case ControlOverlay.TaskList:
			if (Singleton<SceneController>.Instance.IsStoryLevel)
			{
				ShowMinimalisticTaskListControls();
			}
			else
			{
				ShowMinimalisticCheatConsoleControls();
			}
			break;
		case ControlOverlay.Full:
			ShowFullControls();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void TutorialStart()
	{
		Singleton<HUDController>.Instance.ShowOverlay();
	}

	public void ShowOverlay()
	{
		Singleton<HUDController>.Instance.overlay.SetActive(value: true);
	}

	public void HideOverlay()
	{
		Singleton<HUDController>.Instance.overlay.SetActive(value: false);
	}

	public void ResetForTutorial()
	{
		SetEmoteControlsActive(value: true);
		SetSprintControlsActive(value: true);
		SetZoomControlsActive(value: true);
		SetJumpControlsActive(value: true);
		SetInteractControlsActive(value: true);
		SetFixedAnchorControlsActive(value: false);
		SetMovingAnchorControlsActive(value: false);
		SetShootWebControlsActive(value: false);
		SetQuickBuildControlsActive(value: false);
		SetTaskListControlsActive(value: false);
		SetDeleteWebControlsActive(value: false);
		SetStopBuildingControlsActive(value: false);
	}

	public void SetEmoteControlsActive(bool value)
	{
		SetControlsActive(Singleton<HUDController>.Instance.emoteControls, value);
	}

	public void SetSprintControlsActive(bool value)
	{
		SetControlsActive(Singleton<HUDController>.Instance.sprintControls, value);
	}

	public void SetZoomControlsActive(bool value)
	{
		SetControlsActive(Singleton<HUDController>.Instance.zoomControls, value);
	}

	public void SetFixedAnchorControlsActive(bool value)
	{
		SetControlsActive(Singleton<HUDController>.Instance.fixedAnchorControls, value);
	}

	public void SetMovingAnchorControlsActive(bool value)
	{
		SetControlsActive(Singleton<HUDController>.Instance.movingAnchorControls, value);
	}

	public void SetShootWebControlsActive(bool value)
	{
		SetControlsActive(Singleton<HUDController>.Instance.shootWebControls, value);
	}

	public void SetQuickBuildControlsActive(bool value)
	{
		SetControlsActive(Singleton<HUDController>.Instance.quickBuildControls, value);
	}

	public void SetJumpControlsActive(bool value)
	{
		SetControlsActive(Singleton<HUDController>.Instance.jumpControls, value);
	}

	public void SetInteractControlsActive(bool value)
	{
		SetControlsActive(Singleton<HUDController>.Instance.interactControls, value);
	}

	public void SetTaskListControlsActive(bool value)
	{
		SetControlsActive(Singleton<HUDController>.Instance.taskListControls, value);
	}

	public void SetDeleteWebControlsActive(bool value)
	{
		SetControlsActive(Singleton<HUDController>.Instance.deleteWebControls, value);
	}

	public void SetStopBuildingControlsActive(bool value)
	{
		SetControlsActive(Singleton<HUDController>.Instance.stopBuildingControls, value);
	}

	private void GameControllerOnControlSchemeChanged(object sender, GameController.OnControlSchemeChangedEventArgs e)
	{
		Singleton<HUDController>.Instance.SetControlsType(e.InputIsKeyboardMouse);
	}

	private void GameControllerOnToggleHUD(object sender, EventArgs e)
	{
		if (fullControlsAreActive)
		{
			Singleton<HUDController>.Instance.ToggleControlsHUD();
		}
	}

	private void GameControllerOnPauseGame(object sender, EventArgs e)
	{
		HideOverlay();
	}

	private void GameControllerOnContinueGame(object sender, EventArgs e)
	{
		ShowOverlay();
		RefreshContentFitters();
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs eventArgs)
	{
		UpdateOverlayActiveState();
	}

	private void MobileControls_OnShowButtons(object sender, MobileControls.OnShowButtonsEventArgs e)
	{
	}
}
