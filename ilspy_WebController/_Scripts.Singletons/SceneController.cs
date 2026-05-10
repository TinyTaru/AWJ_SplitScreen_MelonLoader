using System;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using InputIcons;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using _Scripts.General;
using _Scripts.UI.Animations;
using _Scripts.UI.Scene_Loading;
using _Scripts.Utils;

namespace _Scripts.Singletons;

public class SceneController : Singleton<SceneController>
{
	[Header("Loading Animation")]
	[SerializeField]
	private GameObject loaderCanvas;

	[SerializeField]
	private Image progressBar;

	[SerializeField]
	private Image loaderBackground;

	[SerializeField]
	private TextMeshProUGUI levelName;

	[SerializeField]
	private CanvasGroup parentCanvasGroup;

	[Header("Release")]
	[SerializeField]
	private LevelData[] levelDataArrayRelease;

	[SerializeField]
	private LevelData fallBackLevelDataRelease;

	[Header("Mobile")]
	[SerializeField]
	private LevelData[] levelDataArrayMobile;

	[SerializeField]
	private LevelData fallBackLevelDataMobile;

	[Header("Demo")]
	[SerializeField]
	private LevelData[] levelDataArrayDemo;

	[SerializeField]
	private LevelData fallBackLevelDataDemo;

	[Header("Event")]
	[SerializeField]
	private LevelData[] levelDataArrayEvent;

	[SerializeField]
	private LevelData fallBackLevelDataEvent;

	private float target;

	private string currentScene;

	private string sceneToLoad;

	private string lastScene;

	private bool sceneIsLoading;

	private bool isStoryLevel;

	private static bool lastLevelWasStoryLevel;

	private LevelData[] levelDataArray;

	public string CurrentScene => Singleton<SceneController>.Instance.currentScene;

	public string LastScene => Singleton<SceneController>.Instance.lastScene;

	public bool IsStoryLevel => Singleton<SceneController>.Instance.isStoryLevel;

	public static bool LastLevelWasStoryLevel => lastLevelWasStoryLevel;

	public static event EventHandler OnSceneLoadingStarted;

	public static event EventHandler OnSceneLoaded;

	protected override void Awake()
	{
		base.Awake();
		currentScene = SceneManager.GetActiveScene().name;
		levelDataArray = levelDataArrayRelease.Concat(levelDataArrayMobile).Concat(levelDataArrayDemo).Concat(levelDataArrayEvent)
			.ToArray();
		if (Singleton<SceneController>.Instance == this)
		{
			InputIconsManagerSO.Instance.Initialize();
			Singleton<SceneController>.Instance.lastScene = "";
		}
		DebugManager.instance.enableRuntimeUI = false;
	}

	private void Update()
	{
		if (Singleton<SceneController>.Instance.sceneIsLoading)
		{
			Singleton<SceneController>.Instance.progressBar.fillAmount = _Scripts.Utils.Utils.ExponentialDecay(Singleton<SceneController>.Instance.progressBar.fillAmount, Singleton<SceneController>.Instance.target, 5f, Time.unscaledDeltaTime);
		}
	}

	private void SetIsStoryLevel(bool value)
	{
		isStoryLevel = value;
		lastLevelWasStoryLevel = value;
	}

	private string GetValidSceneName(string sceneName)
	{
		LevelData[] array = Singleton<SceneController>.Instance.levelDataArrayRelease;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].sceneName.ToUpper() == sceneName.ToUpper())
			{
				return sceneName;
			}
		}
		sceneName = fallBackLevelDataRelease.sceneName;
		return sceneName;
	}

	public void OverwriteIsStoryLevel(bool value)
	{
		isStoryLevel = value;
	}

	public void LoadScene(string sceneName)
	{
		if (!Singleton<SceneController>.Instance.sceneIsLoading)
		{
			Singleton<SceneController>.Instance.sceneIsLoading = true;
			Time.timeScale = 1f;
			Singleton<SceneController>.Instance.lastScene = SceneManager.GetActiveScene().name;
			Debug.Log("Load Scene: " + sceneName);
			SceneController.OnSceneLoadingStarted?.Invoke(this, EventArgs.Empty);
			SceneManager.LoadScene(sceneName);
			Singleton<SceneController>.Instance.sceneIsLoading = false;
			Singleton<SceneController>.Instance.currentScene = sceneName;
			SceneController.OnSceneLoaded?.Invoke(this, EventArgs.Empty);
		}
	}

	public void ExitLevel()
	{
		if (Singleton<SceneController>.Instance.isStoryLevel)
		{
			LoadSpecificStoryLevel("EA_Level_Hub");
		}
		else
		{
			LoadCreativeModeLevel("EA_MainMenu");
		}
	}

	public void LoadStoryLevel()
	{
		lastLevelWasStoryLevel = Singleton<SceneController>.Instance.isStoryLevel;
		Singleton<SceneController>.Instance.isStoryLevel = true;
		string sceneName = SaveController.LoadString("StoryLevel", "EA_Level_Tutorial", SaveData.Game);
		sceneName = GetValidSceneName(sceneName);
		PerformAsyncSceneLoading(sceneName);
	}

	public void LoadSpecificStoryLevel(string sceneName)
	{
		lastLevelWasStoryLevel = Singleton<SceneController>.Instance.isStoryLevel;
		Singleton<SceneController>.Instance.isStoryLevel = true;
		PerformAsyncSceneLoading(sceneName);
	}

	public void LoadCreativeModeLevel(string sceneName)
	{
		Debug.Log("LoadCreativeModeLevel " + sceneName);
		lastLevelWasStoryLevel = Singleton<SceneController>.Instance.isStoryLevel;
		Singleton<SceneController>.Instance.isStoryLevel = false;
		SaveController.ResetDialogueSystemData();
		PerformAsyncSceneLoading(sceneName);
	}

	private async void PerformAsyncSceneLoading(string sceneName)
	{
		if (Singleton<SceneController>.Instance.loaderCanvas == null)
		{
			Debug.LogError("Loader canvas is not assigned!");
		}
		else
		{
			if (Singleton<SceneController>.Instance.sceneIsLoading)
			{
				return;
			}
			if (Singleton<SceneController>.Instance.isStoryLevel)
			{
				SaveController.LoadDialogueSystemData();
			}
			Singleton<SceneController>.Instance.sceneIsLoading = true;
			Time.timeScale = 1f;
			Singleton<SceneController>.Instance.lastScene = SceneManager.GetActiveScene().name;
			Singleton<SceneController>.Instance.target = 0f;
			Singleton<SceneController>.Instance.progressBar.fillAmount = 0f;
			Singleton<SceneController>.Instance.parentCanvasGroup.alpha = 0f;
			Debug.Log("Scene: " + sceneName);
			LevelData[] array = Singleton<SceneController>.Instance.levelDataArray;
			foreach (LevelData levelData in array)
			{
				if (levelData.sceneName.ToUpper() == sceneName.ToUpper())
				{
					Singleton<SceneController>.Instance.loaderBackground.sprite = (SettingsController.ArachnophobiaMode ? levelData.levelImageArachnophobia : levelData.levelImageNormal);
					Singleton<SceneController>.Instance.levelName.text = DialogueManager.GetLocalizedText(levelData.levelName);
					break;
				}
			}
			SceneController.OnSceneLoadingStarted?.Invoke(this, EventArgs.Empty);
			Singleton<SceneController>.Instance.loaderCanvas.SetActive(value: true);
			await AnimateCanvasGroupFade(Singleton<SceneController>.Instance.parentCanvasGroup, 1f, 1f);
			AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
			do
			{
				await Task.Yield();
				Singleton<SceneController>.Instance.target = scene.progress;
			}
			while (scene.progress < 1f);
			await Task.Delay(1000);
			Singleton<SceneController>.Instance.sceneIsLoading = false;
			Singleton<SceneController>.Instance.currentScene = sceneName;
			if (Singleton<SceneController>.Instance.isStoryLevel && sceneName.ToUpper().Contains("LEVEL"))
			{
				SaveController.Save("StoryLevel", sceneName, SaveData.Game);
				SaveController.LoadDialogueSystemData();
			}
			SceneController.OnSceneLoaded?.Invoke(this, EventArgs.Empty);
			await AnimateCanvasGroupFade(Singleton<SceneController>.Instance.parentCanvasGroup, 0f, 1f);
			Singleton<SceneController>.Instance.loaderCanvas.SetActive(value: false);
		}
	}

	private Task AnimateCanvasGroupFade(CanvasGroup canvasGroup, float targetAlpha, float duration)
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		UIAnimation.AnimateFade(canvasGroup, targetAlpha, duration).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			tcs.SetResult(result: true);
		});
		return tcs.Task;
	}

	public void ExitGame()
	{
		Singleton<MusicController>.Instance.StopMusic();
		Singleton<MusicController>.Instance.Pause();
		Application.Quit();
	}

	public string GetLevelName(string sceneName)
	{
		LevelData[] array = levelDataArray;
		foreach (LevelData levelData in array)
		{
			if (levelData.sceneName.ToUpper() == sceneName.ToUpper())
			{
				return levelData.levelName;
			}
		}
		return "";
	}

	public string GetCurrentLevelName()
	{
		return GetLevelName(SceneManager.GetActiveScene().name);
	}

	public string GetCurrentLevelDisplayName()
	{
		return DialogueManager.GetLocalizedText(GetLevelName(SceneManager.GetActiveScene().name.ToUpper()));
	}

	public LevelData GetLevelData(string sceneName)
	{
		sceneName = GetValidSceneName(sceneName);
		return Singleton<SceneController>.Instance.levelDataArray.FirstOrDefault((LevelData levelData) => levelData.sceneName.ToUpper() == sceneName.ToUpper());
	}

	public void SetStoryLevelName(string sceneName)
	{
		SaveController.Save("StoryLevel", sceneName, SaveData.Game);
	}
}
