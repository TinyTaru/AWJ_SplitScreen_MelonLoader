using System;
using TMPro;
using UnityEngine;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.UI.Scene_Loading;
using _Scripts.UnityGamingServices;

namespace _Scripts.UI.MobileMonetization;

public class RemainingPlayTimeWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject widget;

	[SerializeField]
	private TextMeshProUGUI remainingPlayTimeText;

	[Header("Level Data")]
	[SerializeField]
	private LevelData officeLevelData;

	[SerializeField]
	private LevelData kidsRoomLevelData;

	private LevelMusic currentLevel;

	private bool levelUnlocked;

	private float backupTimer;

	private const float backupTimerDuration = 10f;

	private void Awake()
	{
		widget.SetActive(value: false);
	}

	private void OnEnable()
	{
		SceneController.OnSceneLoaded += SceneController_OnSceneLoaded;
		if (Singleton<IAPStoreManager>.Instance != null)
		{
			Singleton<IAPStoreManager>.Instance.SuccessfullyPurchased += IapStoreManager_OnSuccessfullyPurchased;
		}
		if (Singleton<PlaySessionManager>.Instance != null)
		{
			Singleton<PlaySessionManager>.Instance.OnPlaySessionTimeAdded += PLaySessionManager_OnPlaySessionTimeAdded;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionTimerUpdated += PlaySessionManager_OnPlaySessionTimerUpdated;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionExpired += PlaySessionManager_OnPlaySessionExpired;
		}
		Singleton<GameController>.Instance.OnPauseGame += GameController_OnPauseGame;
		Singleton<GameController>.Instance.OnContinueGame += GameController_OnContinueGame;
	}

	private void OnDisable()
	{
		SceneController.OnSceneLoaded -= SceneController_OnSceneLoaded;
		if (Singleton<IAPStoreManager>.Instance != null)
		{
			Singleton<IAPStoreManager>.Instance.SuccessfullyPurchased -= IapStoreManager_OnSuccessfullyPurchased;
		}
		if (Singleton<PlaySessionManager>.Instance != null)
		{
			Singleton<PlaySessionManager>.Instance.OnPlaySessionTimeAdded -= PLaySessionManager_OnPlaySessionTimeAdded;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionTimerUpdated -= PlaySessionManager_OnPlaySessionTimerUpdated;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionExpired -= PlaySessionManager_OnPlaySessionExpired;
		}
		Singleton<GameController>.Instance.OnPauseGame -= GameController_OnPauseGame;
		Singleton<GameController>.Instance.OnContinueGame -= GameController_OnContinueGame;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnApplicationFocus(bool hasFocus)
	{
	}

	private static string FormatRemainingPlayTime(TimeSpan timeSpan)
	{
		if (!(timeSpan.TotalMinutes >= 1.0))
		{
			return $"{timeSpan.Seconds} sec";
		}
		return $"{Mathf.CeilToInt((float)timeSpan.Minutes + (float)timeSpan.Seconds / 60f)} min";
	}

	public void OpenPlayTimeCanvas()
	{
		if (!levelUnlocked && (!(Singleton<SceneController>.Instance.CurrentScene != officeLevelData.sceneName) || !(Singleton<SceneController>.Instance.CurrentScene != kidsRoomLevelData.sceneName)) && Singleton<GameController>.Instance.State == GameController.GameState.Running)
		{
			Singleton<PlayTimeCanvas>.Instance.Open(currentLevel);
		}
	}

	private void SceneController_OnSceneLoaded(object sender, EventArgs e)
	{
		widget.SetActive(value: false);
	}

	private void IapStoreManager_OnSuccessfullyPurchased(string message)
	{
	}

	private void PlaySessionManager_OnPlaySessionTimerUpdated(TimeSpan timeSpan)
	{
	}

	private void PLaySessionManager_OnPlaySessionTimeAdded()
	{
	}

	private void PlaySessionManager_OnPlaySessionExpired()
	{
	}

	private void GameController_OnPauseGame(object sender, EventArgs e)
	{
		widget.SetActive(value: false);
	}

	private void GameController_OnContinueGame(object sender, EventArgs e)
	{
	}
}
