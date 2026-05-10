using System;
using UnityEngine;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UI.HUD;

public class TaskListHUD : MonoBehaviour
{
	[SerializeField]
	private GameObject overlay;

	private bool isEnabled;

	private void Awake()
	{
		isEnabled = true;
	}

	private void Start()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnPauseGame += GameController_OnPauseGame;
			Singleton<GameController>.Instance.OnContinueGame += GameController_OnContinueGame;
		}
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		}
		UpdateOverlayActiveState();
	}

	private void OnDestroy()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnPauseGame -= GameController_OnPauseGame;
			Singleton<GameController>.Instance.OnContinueGame -= GameController_OnContinueGame;
		}
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		}
	}

	private void ShowOverlay()
	{
		overlay.SetActive(isEnabled);
	}

	private void UpdateOverlayActiveState()
	{
		if (SettingsController.ShowControls != ControlOverlay.TaskList)
		{
			HideOverlay();
		}
	}

	public void HideOverlay()
	{
		overlay.SetActive(value: false);
	}

	public void Disable()
	{
		isEnabled = false;
		HideOverlay();
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs eventArgs)
	{
		UpdateOverlayActiveState();
	}

	private void GameController_OnContinueGame(object sender, EventArgs e)
	{
		ShowOverlay();
	}

	private void GameController_OnPauseGame(object sender, EventArgs e)
	{
		HideOverlay();
	}
}
