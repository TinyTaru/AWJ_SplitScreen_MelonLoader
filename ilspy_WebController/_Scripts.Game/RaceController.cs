using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;
using _Scripts.Utils;

namespace _Scripts.Game;

public class RaceController : MonoBehaviour
{
	private enum RaceState
	{
		Inactive,
		Active,
		Running
	}

	public static RaceController Instance;

	[SerializeField]
	private Transform raceCheckpointContainer;

	[SerializeField]
	private Transform raceRespawnPoint;

	[SerializeField]
	private TextMeshProUGUI raceTimerText;

	[Header("Quest Variables")]
	[VariablePopup(false)]
	[SerializeField]
	private string raceFinishedVariable;

	[VariablePopup(false)]
	[SerializeField]
	private string playerTimeVariable;

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

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		checkpoints = new List<RaceCheckpoint>();
		foreach (Transform item in raceCheckpointContainer)
		{
			RaceCheckpoint component = item.GetComponent<RaceCheckpoint>();
			if (component != null)
			{
				checkpoints.Add(component);
			}
		}
		DisableAllCheckpoints();
		state = RaceState.Inactive;
		raceTimerText.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (state == RaceState.Running)
		{
			raceTimer += Time.deltaTime;
			raceTimerText.text = _Scripts.Utils.Utils.FormatTime(raceTimer);
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
			checkpoints[i].gameObject.SetActive(i == 0);
		}
	}

	public void ActivateRace()
	{
		Debug.Log("Activate Race");
		if (state == RaceState.Inactive)
		{
			state = RaceState.Active;
		}
		ResetCheckpoints();
	}

	public void DeactivateRace()
	{
		if (state == RaceState.Active)
		{
			state = RaceState.Inactive;
		}
		DisableAllCheckpoints();
	}

	private void StartRace()
	{
		state = RaceState.Running;
		raceTimer = 0f;
		checkpointCounter = 1;
		onRaceStarted.Invoke();
		raceTimerText.gameObject.SetActive(value: true);
		for (int i = 1; i < checkpoints.Count; i++)
		{
			checkpoints[i].gameObject.SetActive(value: true);
		}
	}

	public void CancelRace()
	{
		if (state == RaceState.Running)
		{
			Singleton<GameController>.Instance.RespawnPlayer(raceRespawnPoint, playSound: false);
			state = RaceState.Active;
			DialogueLua.SetVariable(raceFinishedVariable, false);
			Singleton<MusicController>.Instance.PlaySound("event:/game/general/quest_progress_decrement");
			DialogueManager.SendUpdateTracker();
			raceTimerText.gameObject.SetActive(value: false);
			ResetCheckpoints();
			onRaceCancelled.Invoke();
		}
	}

	private void FinishRace()
	{
		if (state == RaceState.Running)
		{
			state = RaceState.Active;
			DialogueLua.SetVariable(raceFinishedVariable, true);
			DialogueLua.SetVariable(playerTimeVariable, raceTimer);
			Singleton<MusicController>.Instance.PlaySound("event:/game/general/quest_complete");
			DialogueManager.SendUpdateTracker();
			raceTimerText.gameObject.SetActive(value: false);
			ResetCheckpoints();
			onRaceFinished.Invoke();
		}
	}

	public void CheckpointReached()
	{
		if (state == RaceState.Active)
		{
			StartRace();
			return;
		}
		checkpointCounter++;
		if (checkpointCounter >= raceCheckpointContainer.childCount)
		{
			FinishRace();
		}
	}
}
