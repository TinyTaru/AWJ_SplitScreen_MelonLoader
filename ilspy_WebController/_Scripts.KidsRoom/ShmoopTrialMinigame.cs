using System;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Objects;
using _Scripts.Singletons;
using _Scripts.Utils;

namespace _Scripts.KidsRoom;

public class ShmoopTrialMinigame : Singleton<ShmoopTrialMinigame>
{
	public class OnScoreChangedEventArgs : EventArgs
	{
		public int score;
	}

	public class OnTimerChangedEventArgs : EventArgs
	{
		public float timer;
	}

	[Serializable]
	public struct ShmoopColorVariant
	{
		public Material materialDark;

		public Material materialLight;
	}

	[Header("References")]
	[SerializeField]
	private Transform shmoopSpawnPositions;

	[SerializeField]
	private Shmoop shmoopPrefab;

	[SerializeField]
	private ShmoopPenTrigger shmoopPenTrigger;

	[SerializeField]
	private Shmoop[] shmoopsSandboxMode;

	[Header("Dialogue System References")]
	[SerializeField]
	[VariablePopup(false)]
	private string scoreVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string scoreToBeatVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string minigameActiveVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string minigameFinishedVariable;

	[Header("Parameters")]
	[SerializeField]
	private int scoreToBeat = 10;

	[SerializeField]
	private float minigameDurationPC = 60f;

	[SerializeField]
	private float minigameDurationMobile = 90f;

	[SerializeField]
	private ShmoopColorVariant[] shmoopColorVariants;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onMinigameStartedEvent;

	[SerializeField]
	private UnityEvent onMinigameFinishedEvent;

	[SerializeField]
	private UnityEvent onScoreIncreasedEvent;

	[SerializeField]
	private UnityEvent onScoreDecreasedEvent;

	private int score;

	private int highscore;

	private float minigameTimer;

	private bool minigameActive;

	private List<Shmoop> shmoops;

	private bool isInitialized;

	public event EventHandler<OnScoreChangedEventArgs> OnScoreChanged;

	public event EventHandler<OnTimerChangedEventArgs> OnTimerChanged;

	public event EventHandler OnMiniGameStarted;

	public event EventHandler OnMiniGameStopped;

	private void Start()
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel)
		{
			shmoops = shmoopsSandboxMode.ToList();
			SpawnShmoops();
			shmoopPenTrigger.OnPenEnter += ShmoopPenTrigger_OnPenEnter;
			shmoopPenTrigger.OnPenExit += ShmoopPenTrigger_OnPenExit;
		}
	}

	private void Update()
	{
		if (minigameActive)
		{
			UpdateMinigameTimer();
		}
	}

	private void UpdateMinigameTimer()
	{
		minigameTimer -= Time.deltaTime;
		if (minigameTimer <= 0f)
		{
			StopMinigame();
		}
		this.OnTimerChanged?.Invoke(this, new OnTimerChangedEventArgs
		{
			timer = minigameTimer
		});
	}

	public void StartMinigame()
	{
		if (!Singleton<GameController>.Instance.MinigameActive && !minigameActive)
		{
			score = 0;
			minigameTimer = minigameDurationPC;
			SpawnShmoops();
			minigameActive = true;
			DialogueLua.SetVariable(minigameActiveVariable, true);
			DialogueLua.SetVariable(minigameFinishedVariable, false);
			DialogueLua.SetVariable(scoreToBeatVariable, scoreToBeat);
			Singleton<GameController>.Instance.SetMinigameActive(value: true);
			this.OnMiniGameStarted?.Invoke(this, EventArgs.Empty);
			this.OnScoreChanged?.Invoke(this, new OnScoreChangedEventArgs
			{
				score = score
			});
			this.OnTimerChanged?.Invoke(this, new OnTimerChangedEventArgs
			{
				timer = minigameTimer
			});
			onMinigameStartedEvent?.Invoke();
		}
	}

	private void StopMinigame()
	{
		minigameActive = false;
		DialogueLua.SetVariable(minigameActiveVariable, false);
		DialogueLua.SetVariable(minigameFinishedVariable, true);
		DialogueLua.SetVariable(scoreVariable, score);
		Singleton<GameController>.Instance.SetMinigameActive(value: false);
		this.OnMiniGameStopped?.Invoke(this, EventArgs.Empty);
		onMinigameFinishedEvent?.Invoke();
	}

	private void SpawnShmoops()
	{
		foreach (Shmoop item in shmoops.ToList())
		{
			if (!(item == null))
			{
				item.GetComponent<MovableObject>().DestroySafely();
			}
		}
		shmoops = new List<Shmoop>();
		foreach (Transform shmoopSpawnPosition in shmoopSpawnPositions)
		{
			Shmoop shmoop = UnityEngine.Object.Instantiate(shmoopPrefab, shmoopSpawnPosition.position, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
			ShmoopColorVariant shmoopColorVariant = shmoopColorVariants.RandomValue();
			shmoop.ChangeMaterials(shmoopColorVariant.materialDark, shmoopColorVariant.materialLight);
			shmoops.Add(shmoop);
		}
	}

	private void ShmoopPenTrigger_OnPenEnter(object sender, EventArgs e)
	{
		score++;
		onScoreIncreasedEvent?.Invoke();
		this.OnScoreChanged?.Invoke(this, new OnScoreChangedEventArgs
		{
			score = score
		});
	}

	private void ShmoopPenTrigger_OnPenExit(object sender, EventArgs e)
	{
		score--;
		onScoreDecreasedEvent?.Invoke();
		this.OnScoreChanged?.Invoke(this, new OnScoreChangedEventArgs
		{
			score = score
		});
	}
}
