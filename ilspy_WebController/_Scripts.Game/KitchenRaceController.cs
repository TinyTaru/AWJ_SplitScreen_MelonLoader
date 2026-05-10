using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;
using _Scripts.Utils;

namespace _Scripts.Game;

public class KitchenRaceController : Singleton<KitchenRaceController>
{
	public enum RaceState
	{
		Inactive,
		Active,
		Running
	}

	[Header("References")]
	[SerializeField]
	private Transform raceCheckpointContainer;

	[SerializeField]
	private TextMeshProUGUI raceTimerText;

	[SerializeField]
	private TextMeshProUGUI timeToBeatText;

	[SerializeField]
	private GameObject raceUI;

	[SerializeField]
	private Transform playerResetPosition;

	[SerializeField]
	private GameObject raceSettings;

	[Header("Parameters")]
	[SerializeField]
	private float timeToBeatPC = 55f;

	[SerializeField]
	private float timeToBeatMobile = 55f;

	[SerializeField]
	private int visibleCheckpointsAhead = 2;

	[SerializeField]
	private Material currentCheckpointMaterial;

	[SerializeField]
	private Material futureCheckpointMaterial;

	[SerializeField]
	[VariablePopup(false)]
	private string raceTimerVariableName = "Office.RaceTime";

	[SerializeField]
	[VariablePopup(false)]
	private string raceActiveVariableName;

	[SerializeField]
	[VariablePopup(false)]
	private string raceFinishedVariableName;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onRaceStarted;

	[SerializeField]
	private UnityEvent onRaceCancelled;

	[SerializeField]
	private UnityEvent onRaceFinished;

	private List<RaceCheckpoint> checkpoints;

	private float raceTimer;

	private bool raceActive;

	private RaceState state;

	private int checkpointCounter;

	private float timeToBeat;

	public RaceState State => state;

	private void Start()
	{
		checkpoints = new List<RaceCheckpoint>();
		foreach (Transform item in raceCheckpointContainer)
		{
			RaceCheckpoint component = item.GetComponent<RaceCheckpoint>();
			if (component != null)
			{
				component.AssignRaceController(this);
				checkpoints.Add(component);
			}
		}
		DisableAllCheckpoints();
		state = RaceState.Inactive;
		raceUI.SetActive(value: false);
		timeToBeat = timeToBeatPC;
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnPauseGame += GameController_OnPauseGame;
			Singleton<GameController>.Instance.OnContinueGame += GameController_OnContinueGame;
		}
	}

	private void Update()
	{
		if (state == RaceState.Running)
		{
			raceTimer += Time.deltaTime;
			raceTimerText.text = _Scripts.Utils.Utils.FormatTime(raceTimer);
		}
	}

	private void OnDestroy()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnPauseGame -= GameController_OnPauseGame;
			Singleton<GameController>.Instance.OnContinueGame -= GameController_OnContinueGame;
		}
	}

	private void DisableAllCheckpoints()
	{
		foreach (RaceCheckpoint checkpoint in checkpoints)
		{
			checkpoint.gameObject.SetActive(value: false);
		}
	}

	private void ResetCheckpoints()
	{
		for (int i = 0; i < checkpoints.Count; i++)
		{
			if (i == 0)
			{
				checkpoints[i].gameObject.SetActive(value: true);
				checkpoints[i].MakeCurrentCheckpoint(currentCheckpointMaterial);
			}
			else
			{
				checkpoints[i].gameObject.SetActive(value: false);
			}
		}
	}

	private void HideRaceSettings()
	{
		raceSettings.SetActive(value: false);
	}

	public void ActivateRace()
	{
		state = RaceState.Active;
		DialogueLua.SetVariable(raceActiveVariableName, true);
		ResetCheckpoints();
	}

	private void StartRace()
	{
		if (Singleton<GameController>.Instance.MinigameActive)
		{
			DialogueLua.SetVariable(raceActiveVariableName, false);
			return;
		}
		state = RaceState.Running;
		raceTimer = 0f;
		checkpointCounter = 1;
		onRaceStarted.Invoke();
		raceUI.SetActive(value: true);
		timeToBeatText.text = DialogueManager.GetLocalizedText("Misc_Time to beat") + _Scripts.Utils.Utils.FormatTime(timeToBeat);
		DialogueLua.SetVariable(raceFinishedVariableName, false);
		Singleton<GameController>.Instance.SetMinigameActive(value: true);
		ShowNextCheckpoints();
	}

	private void ShowNextCheckpoints()
	{
		int num = checkpointCounter + visibleCheckpointsAhead;
		for (int i = 1; i < checkpoints.Count; i++)
		{
			if (i == checkpointCounter)
			{
				checkpoints[i].gameObject.SetActive(value: true);
				checkpoints[i].MakeCurrentCheckpoint(currentCheckpointMaterial);
			}
			else if (i > checkpointCounter && i < num)
			{
				checkpoints[i].gameObject.SetActive(value: true);
				checkpoints[i].DisplayAsFutureCheckpoint(futureCheckpointMaterial);
			}
			else
			{
				checkpoints[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void ContinueRace()
	{
		if (state == RaceState.Running)
		{
			Singleton<GameController>.Instance.ContinueGame();
			HideRaceSettings();
		}
	}

	public void RestartRace()
	{
		if (state == RaceState.Running)
		{
			state = RaceState.Active;
			DialogueLua.SetVariable(raceActiveVariableName, true);
			DialogueLua.SetVariable(raceFinishedVariableName, false);
			Singleton<GameController>.Instance.SetMinigameActive(value: false);
			raceUI.SetActive(value: false);
			ResetCheckpoints();
			Singleton<GameController>.Instance.ContinueGame();
			Singleton<GameController>.Instance.RespawnPlayer(playerResetPosition, playSound: false);
			HideRaceSettings();
		}
	}

	public void CancelRace()
	{
		if (state == RaceState.Running)
		{
			state = RaceState.Inactive;
			DialogueLua.SetVariable(raceActiveVariableName, false);
			DialogueLua.SetVariable(raceFinishedVariableName, false);
			Singleton<GameController>.Instance.SetMinigameActive(value: false);
			raceUI.SetActive(value: false);
			DisableAllCheckpoints();
			Singleton<GameController>.Instance.ContinueGame();
			HideRaceSettings();
			onRaceCancelled.Invoke();
		}
	}

	private void FinishRace()
	{
		if (state == RaceState.Running)
		{
			state = RaceState.Inactive;
			DialogueLua.SetVariable(raceActiveVariableName, false);
			DialogueLua.SetVariable(raceFinishedVariableName, true);
			Singleton<GameController>.Instance.SetMinigameActive(value: false);
			Invoke("DisableRaceUI", 2f);
			DisableAllCheckpoints();
			DialogueLua.SetVariable(raceTimerVariableName, raceTimer);
			onRaceFinished.Invoke();
		}
	}

	private void DisableRaceUI()
	{
		raceUI.SetActive(value: false);
	}

	public void CheckpointReached()
	{
		if (state == RaceState.Active)
		{
			StartRace();
			return;
		}
		checkpointCounter++;
		ShowNextCheckpoints();
		if (checkpointCounter >= raceCheckpointContainer.childCount)
		{
			if (raceTimer <= timeToBeat)
			{
				FinishRace();
				return;
			}
			state = RaceState.Active;
			DialogueLua.SetVariable(raceActiveVariableName, false);
			Singleton<GameController>.Instance.SetMinigameActive(value: false);
			raceUI.SetActive(value: false);
			ResetCheckpoints();
			onRaceCancelled.Invoke();
		}
	}

	public void ShowRaceSettings()
	{
		raceSettings.SetActive(value: true);
	}

	private void GameController_OnPauseGame(object sender, EventArgs e)
	{
		raceUI.SetActive(value: false);
	}

	private void GameController_OnContinueGame(object sender, EventArgs e)
	{
		raceUI.SetActive(state == RaceState.Running);
	}
}
