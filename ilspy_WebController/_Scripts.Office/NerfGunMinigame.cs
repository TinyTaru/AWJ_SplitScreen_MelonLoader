using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;
using _Scripts.Utils;

namespace _Scripts.Office;

public class NerfGunMinigame : MonoBehaviour
{
	public class OnScoreChangedEventArgs : EventArgs
	{
		public int score;
	}

	public class OnTimerChangedEventArgs : EventArgs
	{
		public float timer;
	}

	[Header("References")]
	[SerializeField]
	private Transform targetContainer;

	[Header("Dialogue System References")]
	[SerializeField]
	[VariablePopup(false)]
	private string scoreVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string scoreToBeatVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string highscoreVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string minigameDurationVariable;

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
	private int targetStartAmount = 3;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onMinigameStartedEvent;

	[SerializeField]
	private UnityEvent onMinigameFinishedEvent;

	[SerializeField]
	private UnityEvent onNewHighscoreEvent;

	[SerializeField]
	private UnityEvent onNoNewHighscoreEvent;

	[SerializeField]
	private UnityEvent onTargetHitEvent;

	private int score;

	private int highscore;

	private float minigameTimer;

	private bool minigameActive;

	private List<CatapultTarget> activeTargets;

	private List<CatapultTarget> inactiveTargets;

	public event EventHandler<OnScoreChangedEventArgs> OnScoreChanged;

	public event EventHandler<OnTimerChangedEventArgs> OnTimerChanged;

	public event EventHandler OnMiniGameStarted;

	public event EventHandler OnMiniGameStopped;

	private void Awake()
	{
		activeTargets = new List<CatapultTarget>();
		inactiveTargets = new List<CatapultTarget>();
		CatapultTarget[] componentsInChildren = targetContainer.GetComponentsInChildren<CatapultTarget>();
		foreach (CatapultTarget obj in componentsInChildren)
		{
			obj.OnTargetHit += NerfGunTarget_OnTargetHit;
			obj.OnDeactivate += NerfGunTarget_OnDeactivate;
			obj.Deactivate();
		}
		DialogueLua.SetVariable(scoreToBeatVariable, scoreToBeat);
		DialogueLua.SetVariable(minigameDurationVariable, minigameDurationPC);
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

	private void ActivateRandomTarget()
	{
		CatapultTarget catapultTarget = inactiveTargets.RandomValue();
		inactiveTargets.Remove(catapultTarget);
		activeTargets.Add(catapultTarget);
		catapultTarget.Activate();
	}

	public void StartMinigame()
	{
		if (!Singleton<GameController>.Instance.MinigameActive && !minigameActive)
		{
			score = 0;
			minigameTimer = minigameDurationPC;
			for (int i = 0; i < targetStartAmount; i++)
			{
				ActivateRandomTarget();
			}
			minigameActive = true;
			DialogueLua.SetVariable(minigameActiveVariable, true);
			DialogueLua.SetVariable(minigameFinishedVariable, false);
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
		for (int num = activeTargets.Count - 1; num >= 0; num--)
		{
			activeTargets[num].Deactivate();
		}
		minigameActive = false;
		DialogueLua.SetVariable(minigameActiveVariable, false);
		DialogueLua.SetVariable(minigameFinishedVariable, true);
		DialogueLua.SetVariable(scoreVariable, score);
		Singleton<GameController>.Instance.SetMinigameActive(value: false);
		this.OnMiniGameStopped?.Invoke(this, EventArgs.Empty);
		onMinigameFinishedEvent?.Invoke();
		highscore = DialogueLua.GetVariable(highscoreVariable, 0);
		if (score > Mathf.Max(scoreToBeat, highscore))
		{
			onNewHighscoreEvent?.Invoke();
		}
		else
		{
			onNoNewHighscoreEvent?.Invoke();
		}
	}

	private void NerfGunTarget_OnTargetHit(object sender, EventArgs e)
	{
		score++;
		ActivateRandomTarget();
		this.OnScoreChanged?.Invoke(this, new OnScoreChangedEventArgs
		{
			score = score
		});
		onTargetHitEvent?.Invoke();
	}

	private void NerfGunTarget_OnDeactivate(object sender, EventArgs e)
	{
		if (activeTargets.Contains(sender as CatapultTarget))
		{
			activeTargets.Remove(sender as CatapultTarget);
		}
		if (!inactiveTargets.Contains(sender as CatapultTarget))
		{
			inactiveTargets.Add(sender as CatapultTarget);
		}
	}
}
