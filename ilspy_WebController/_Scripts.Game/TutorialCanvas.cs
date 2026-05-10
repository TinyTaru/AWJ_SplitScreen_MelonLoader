using System;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Game;

public class TutorialCanvas : MonoBehaviour
{
	[SerializeField]
	private Canvas tutorialCanvas;

	[SerializeField]
	private GameObject[] englishInstructions;

	[SerializeField]
	private GameObject[] germanInstructions;

	[SerializeField]
	private GameObject[] tutorialInstructions;

	private GameObject currentInstruction;

	private void OnEnable()
	{
		SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		Singleton<GameController>.Instance.OnGameStateChanged += OnGameStateChanged;
		UpdateCurrentInstruction();
		UpdateInstructionLanguage();
	}

	private void OnDisable()
	{
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		Singleton<GameController>.Instance.OnGameStateChanged -= OnGameStateChanged;
	}

	private void Start()
	{
		if (tutorialCanvas != null)
		{
			tutorialCanvas.worldCamera = Singleton<CameraController>.Instance.MainCamera;
			tutorialCanvas.sortingLayerName = "UI";
		}
		UpdateInstructionLanguage();
	}

	private void UpdateInstructionLanguage()
	{
		GameObject[] array = englishInstructions;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(SettingsController.Language == SystemLanguage.English);
		}
		array = germanInstructions;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(SettingsController.Language == SystemLanguage.German);
		}
	}

	private void OnGameStateChanged(object sender, GameController.OnGameStateChangedEventArgs onGameStateChangedEventArgs)
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Cutscene)
		{
			UpdateCurrentInstruction();
			DisableAllInstructions();
		}
		else if (Singleton<GameController>.Instance.LastState == GameController.GameState.Cutscene)
		{
			EnableCurrentInstruction();
		}
	}

	private void UpdateCurrentInstruction()
	{
		GameObject[] array = tutorialInstructions;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.activeSelf)
			{
				currentInstruction = gameObject;
				break;
			}
		}
	}

	private void EnableCurrentInstruction()
	{
		if (!(currentInstruction == null))
		{
			currentInstruction.SetActive(value: true);
		}
	}

	public void DisableAllInstructions()
	{
		GameObject[] array = tutorialInstructions;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		UpdateInstructionLanguage();
	}
}
