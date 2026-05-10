using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.LivingRoom;

public class PianoMinigame : Singleton<PianoMinigame>
{
	private enum PianoMinigameState
	{
		Uninitialized,
		SettingUp,
		Running,
		WaitingForEnd,
		Ended
	}

	private enum HitQuality
	{
		Miss,
		Ok,
		Good,
		Perfect
	}

	private readonly struct BeatInfo
	{
		public readonly int bar;

		public readonly int beat;

		public readonly float tempo;

		public readonly int timeSigUpper;

		public readonly int timeSigLower;

		public readonly int timelineMs;

		public BeatInfo(int bar, int beat, float tempo, int timeSigUpper, int timeSigLower, int timelineMs)
		{
			this.bar = bar;
			this.beat = beat;
			this.tempo = tempo;
			this.timeSigUpper = timeSigUpper;
			this.timeSigLower = timeSigLower;
			this.timelineMs = timelineMs;
		}
	}

	[Header("Asset References")]
	[SerializeField]
	private PianoMinigameShmoop minigameShmoopPrefab;

	[SerializeField]
	private PianoMinigameFeedback pianoMinigameFeedbackPrefab;

	[Tooltip("The judgement line object in the scene (visible or invisible)")]
	[SerializeField]
	private Transform judgmentLine;

	[Tooltip("Make sure these are in order of string 0 to string 4")]
	[SerializeField]
	private List<PianoString> pianoStrings;

	[SerializeField]
	private TextAsset beatChartCsv;

	[SerializeField]
	private PianoQuestUI pianoQuestUI;

	[SerializeField]
	[VariablePopup(false)]
	private string minigameActiveVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string minigameFinishedVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string scoreVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string scoreToBeatVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string maxPointsVariable;

	[Header("Sound References")]
	private EventInstance backingTrackLoopInstance;

	[SerializeField]
	private EventReference backingTrackLoopReference;

	[SerializeField]
	private StudioEventEmitter multiplierUpSound;

	[SerializeField]
	private StudioEventEmitter multiplierDownSound;

	[SerializeField]
	private StudioEventEmitter noteSound;

	[Header("Parameters")]
	[Tooltip("A delay before the game really starts, to let the player adjust and get ready.")]
	[SerializeField]
	private float startupDelay;

	[Tooltip("Shmoop speed per second; adjust for higher/lower difficulty.")]
	[SerializeField]
	private float shmoopSpeed = 10f;

	[Tooltip("Percentage of total points player has to get to \"win\" the game.")]
	[SerializeField]
	private float winningThreshold = 0.5f;

	[SerializeField]
	private int scoreToBeat = 6000;

	[Header("Scoring")]
	[SerializeField]
	private int pointsOkHit = 80;

	[SerializeField]
	private int pointsGoodHit = 100;

	[SerializeField]
	private int pointsPerfectHit = 120;

	private int currentMultiplier = 1;

	[SerializeField]
	private int maxMultiplier = 5;

	[SerializeField]
	private int hitsToIncreaseMultiplier;

	private const float hitThresholdPerfect = 0.75f;

	private const float hitThresholdGood = 1.125f;

	private const float hitThresholdOk = 1.5f;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onMinigameStarted;

	[SerializeField]
	private UnityEvent onMinigameCancelled;

	[SerializeField]
	private UnityEvent onMinigameFinished;

	private float currentBpm;

	private float shmoopInitialSpawnDelay;

	private string[] beatChartLines;

	private int currentBeat;

	private int totalShmoopsSpawned;

	private int currentPlayerPoints;

	private float minigameStartupDelayCounter;

	private float shmoopInitialSpawnDelayCounter;

	private bool canSpawnShmoops;

	private bool spawnShmoopCoroutineRunning;

	private int multiplierCounter;

	private bool wasMusicMuted;

	private bool wasMoviesMuted;

	private bool wasRecordPlayerMuted;

	private Dictionary<int, List<PianoMinigameShmoop>> spawnedShmoops = new Dictionary<int, List<PianoMinigameShmoop>>();

	[SerializeField]
	private bool isGodMode;

	private PianoMinigameState state;

	private static readonly ConcurrentQueue<BeatInfo> BeatQueue = new ConcurrentQueue<BeatInfo>();

	private EVENT_CALLBACK beatCallback;

	private void Start()
	{
		state = PianoMinigameState.Uninitialized;
		pianoQuestUI.gameObject.SetActive(value: false);
		DialogueLua.SetVariable(scoreToBeatVariable, scoreToBeat);
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnPauseGame += GameController_OnPauseGame;
			Singleton<GameController>.Instance.OnContinueGame += GameController_OnContinueGame;
		}
		backingTrackLoopInstance = RuntimeManager.CreateInstance(backingTrackLoopReference);
		beatCallback = OnFmodEventCallback;
		RESULT rESULT = backingTrackLoopInstance.setCallback(beatCallback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT);
		if (rESULT != 0)
		{
			UnityEngine.Debug.LogError("FMOD setCallback failed: " + rESULT);
		}
	}

	private void Update()
	{
		switch (state)
		{
		case PianoMinigameState.SettingUp:
			minigameStartupDelayCounter += Time.deltaTime;
			pianoQuestUI.SetGameInfoStartingText(Mathf.CeilToInt(3f - minigameStartupDelayCounter));
			break;
		case PianoMinigameState.Running:
		{
			BeatInfo result;
			while (BeatQueue.TryDequeue(out result))
			{
				SpawnShmoop();
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case PianoMinigameState.Uninitialized:
		case PianoMinigameState.WaitingForEnd:
		case PianoMinigameState.Ended:
			break;
		}
	}

	private void OnDestroy()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnPauseGame -= GameController_OnPauseGame;
			Singleton<GameController>.Instance.OnContinueGame -= GameController_OnContinueGame;
		}
		Singleton<MusicController>.Instance.SetLivingRoomMiniGameOnState(isOn: false);
	}

	private IEnumerator MinigameStartWaitingCoroutine()
	{
		UnityEngine.Debug.Log("[PianoMinigame] Waiting for minigame to start...");
		yield return new WaitUntil(() => minigameStartupDelayCounter >= startupDelay);
		UnityEngine.Debug.Log("[PianoMinigame] Starting game!");
		StartMinigame();
	}

	private IEnumerator InitialShmoopSpawnDelayCoroutine()
	{
		UnityEngine.Debug.Log("[PianoMinigame] Waiting to spawn first shmoop...");
		yield return new WaitForSeconds(shmoopInitialSpawnDelay);
		canSpawnShmoops = true;
	}

	private IEnumerator HandleGameEndSequence()
	{
		yield return new WaitForSeconds(2f);
		state = PianoMinigameState.Uninitialized;
		yield return new WaitForSeconds(3f);
		pianoQuestUI.HideInfoText();
	}

	private void ReadCsvIntoReader()
	{
		if (!(Singleton<PianoMinigame>.Instance.beatChartCsv == null))
		{
			beatChartLines = Singleton<PianoMinigame>.Instance.beatChartCsv.text.Split("\n");
			string[] array = beatChartLines[currentBeat].Split(",");
			currentBpm = float.Parse(array[0]);
			shmoopInitialSpawnDelay = float.Parse(array[1]);
			UnityEngine.Debug.Log("[PianoMinigame] Read BPM as " + currentBpm + " and initial startup delay as " + shmoopInitialSpawnDelay);
			currentBeat++;
		}
	}

	private void SetupGame(bool isRestart)
	{
		if (!Singleton<GameController>.Instance.MinigameActive && state == PianoMinigameState.Uninitialized)
		{
			Singleton<MusicController>.Instance.SetLivingRoomMiniGameOnState(isOn: true);
			CleanupMinigameData();
			ReadCsvIntoReader();
			state = PianoMinigameState.SettingUp;
			minigameStartupDelayCounter = 0f;
			pianoQuestUI.PrepareForStartup();
			pianoQuestUI.gameObject.SetActive(value: true);
			StartCoroutine(MinigameStartWaitingCoroutine());
		}
	}

	private void StartMinigame()
	{
		state = PianoMinigameState.Running;
		BeatQueue.Clear();
		multiplierCounter = 0;
		Singleton<GameController>.Instance.SetMinigameActive(value: true);
		backingTrackLoopInstance.start();
		pianoQuestUI.StartMinigame();
		StartCoroutine(InitialShmoopSpawnDelayCoroutine());
		ActivateMinigameCollidersForPianoStrings(value: true);
		DialogueLua.SetVariable(minigameActiveVariable, true);
		onMinigameStarted?.Invoke();
	}

	private void ActivateMinigameCollidersForPianoStrings(bool value)
	{
		foreach (PianoString pianoString in pianoStrings)
		{
			pianoString.SwitchToMinigameCollider(value);
		}
	}

	private void SpawnShmoop()
	{
		if (currentBeat >= beatChartLines.Length)
		{
			state = PianoMinigameState.WaitingForEnd;
			CheckGameEnd();
			return;
		}
		int[] shmoopsCountForCurrentBeat = GetShmoopsCountForCurrentBeat();
		for (int i = 0; i < shmoopsCountForCurrentBeat.Length; i++)
		{
			for (int j = 0; j < shmoopsCountForCurrentBeat[i]; j++)
			{
				CreateShmoopAtString(i);
			}
		}
	}

	private void CreateShmoopAtString(int stringIndex)
	{
		PianoString pianoString = pianoStrings[stringIndex];
		Vector3 position = pianoString.GetShmoopSpawnPoint().position;
		Vector3 forward = pianoString.GetShmoopEndPoint().position - position;
		PianoMinigameShmoop pianoMinigameShmoop = UnityEngine.Object.Instantiate(minigameShmoopPrefab, position, Quaternion.LookRotation(forward, Vector3.up), pianoString.transform);
		pianoMinigameShmoop.SetPianoStringEndpoint(pianoString.GetShmoopEndPoint());
		pianoMinigameShmoop.SetShmoopSpeed(shmoopSpeed);
		pianoMinigameShmoop.SetShmoopStringId(stringIndex);
		totalShmoopsSpawned++;
		if (!spawnedShmoops.ContainsKey(stringIndex))
		{
			spawnedShmoops.Add(stringIndex, new List<PianoMinigameShmoop>());
		}
		spawnedShmoops[stringIndex].Add(pianoMinigameShmoop);
	}

	private int[] GetShmoopsCountForCurrentBeat()
	{
		int[] array = new int[5];
		string[] array2 = beatChartLines[currentBeat].Split(",");
		for (int i = 0; i < array2.Length && i < array.Length; i++)
		{
			int.TryParse(array2[i], out array[i]);
		}
		currentBeat++;
		return array;
	}

	private void AddPoints(int points)
	{
		currentPlayerPoints += points * currentMultiplier;
		pianoQuestUI.UpdatePoints(currentPlayerPoints);
	}

	private void UpdateMultiplier(bool isIncrease)
	{
		multiplierCounter++;
		if (multiplierCounter >= hitsToIncreaseMultiplier && isIncrease && currentMultiplier < maxMultiplier)
		{
			currentMultiplier++;
			multiplierCounter = 0;
			pianoQuestUI.UpdateMultiplier(currentMultiplier);
			PlayMultiplierSound(multiplierUpSound);
		}
		else if (!isIncrease && currentMultiplier > 1)
		{
			currentMultiplier--;
			multiplierCounter = 0;
			pianoQuestUI.UpdateMultiplier(currentMultiplier);
			PlayMultiplierSound(multiplierDownSound);
		}
	}

	private void PlayMultiplierSound(StudioEventEmitter emitter)
	{
		emitter.SetParameter("multiplier_value", currentMultiplier);
		emitter.SetParameter("volume", (float)currentMultiplier / (float)maxMultiplier);
		emitter.Play();
	}

	private void EndGame()
	{
		state = PianoMinigameState.Ended;
		Singleton<GameController>.Instance.SetMinigameActive(value: false);
		backingTrackLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		Singleton<MusicController>.Instance.SetLivingRoomMiniGameOnState(isOn: false);
		int maximumPossiblePoints = GetMaximumPossiblePoints();
		pianoQuestUI.EndMinigame(currentPlayerPoints, maximumPossiblePoints);
		ActivateMinigameCollidersForPianoStrings(value: false);
		StartCoroutine(HandleGameEndSequence());
		DialogueLua.SetVariable(maxPointsVariable, maximumPossiblePoints);
		DialogueLua.SetVariable(scoreVariable, currentPlayerPoints);
		DialogueLua.SetVariable(minigameActiveVariable, false);
		DialogueLua.SetVariable(minigameFinishedVariable, true);
		onMinigameFinished?.Invoke();
	}

	private int GetMaximumPossiblePoints()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 1; i < maxMultiplier; i++)
		{
			num += pointsPerfectHit * hitsToIncreaseMultiplier * i;
			num2 += hitsToIncreaseMultiplier;
		}
		return num + (totalShmoopsSpawned - num2) * pointsPerfectHit * maxMultiplier;
	}

	private void CheckGameEnd()
	{
		foreach (List<PianoMinigameShmoop> value in spawnedShmoops.Values)
		{
			if (value.Count > 0)
			{
				return;
			}
		}
		EndGame();
	}

	private void PlayPauseBackingTrack(bool pauseTrack)
	{
		if ((state == PianoMinigameState.Running || state == PianoMinigameState.WaitingForEnd) && !backingTrackLoopReference.IsNull && backingTrackLoopInstance.isValid())
		{
			backingTrackLoopInstance.setPaused(pauseTrack);
		}
	}

	private void CleanupMinigameData()
	{
		spawnShmoopCoroutineRunning = false;
		canSpawnShmoops = false;
		StopAllCoroutines();
		foreach (List<PianoMinigameShmoop> value in spawnedShmoops.Values)
		{
			foreach (PianoMinigameShmoop item in value)
			{
				if (item != null)
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
			}
		}
		spawnedShmoops.Clear();
		currentBeat = 0;
		totalShmoopsSpawned = 0;
		currentPlayerPoints = 0;
		currentMultiplier = 1;
		pianoQuestUI.UpdatePoints(0);
		pianoQuestUI.UpdateMultiplier(1);
		pianoQuestUI.HideGameplayTexts();
		ActivateMinigameCollidersForPianoStrings(value: false);
	}

	private HitQuality EvaluateShmoopHitQuality(float posDiff)
	{
		if (posDiff <= 1.125f)
		{
			if (posDiff <= 0.75f)
			{
				AddPoints(pointsPerfectHit);
				UpdateMultiplier(isIncrease: true);
				return HitQuality.Perfect;
			}
			AddPoints(pointsGoodHit);
			UpdateMultiplier(isIncrease: true);
			return HitQuality.Good;
		}
		if (posDiff <= 1.5f)
		{
			AddPoints(pointsOkHit);
			UpdateMultiplier(isIncrease: true);
			return HitQuality.Ok;
		}
		UpdateMultiplier(isIncrease: false);
		return HitQuality.Miss;
	}

	private static RESULT OnFmodEventCallback(EVENT_CALLBACK_TYPE type, IntPtr eventInstancePtr, IntPtr parameterPtr)
	{
		if (type == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
		{
			TIMELINE_BEAT_PROPERTIES tIMELINE_BEAT_PROPERTIES = Marshal.PtrToStructure<TIMELINE_BEAT_PROPERTIES>(parameterPtr);
			BeatInfo item = new BeatInfo(tIMELINE_BEAT_PROPERTIES.bar, tIMELINE_BEAT_PROPERTIES.beat, tIMELINE_BEAT_PROPERTIES.tempo, tIMELINE_BEAT_PROPERTIES.timesignatureupper, tIMELINE_BEAT_PROPERTIES.timesignaturelower, tIMELINE_BEAT_PROPERTIES.position);
			BeatQueue.Enqueue(item);
		}
		return RESULT.OK;
	}

	public void SetupGame()
	{
		SetupGame(isRestart: false);
	}

	public void OnShmoopEndReached(PianoMinigameShmoop shmoop, int shmoopStringId)
	{
		if (spawnedShmoops.ContainsKey(shmoopStringId))
		{
			spawnedShmoops[shmoopStringId].Remove(shmoop);
		}
		shmoop.FallDown();
		UpdateMultiplier(isIncrease: false);
		UnityEngine.Object.Instantiate(pianoMinigameFeedbackPrefab, shmoop.transform.position, Quaternion.identity, null).SetFeedbackText(HitQuality.Miss.ToString());
		if (state == PianoMinigameState.WaitingForEnd)
		{
			CheckGameEnd();
		}
	}

	public void ShowMinigameSettings()
	{
		pianoQuestUI.ShowSettingsUI();
		PlayPauseBackingTrack(pauseTrack: true);
	}

	public bool IsMinigameActive()
	{
		if (state != PianoMinigameState.Running && state != PianoMinigameState.WaitingForEnd)
		{
			return state == PianoMinigameState.SettingUp;
		}
		return true;
	}

	public void ContinueMinigame()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.ContinueGame();
		}
		pianoQuestUI.HideSettingsUI();
		PlayPauseBackingTrack(pauseTrack: false);
	}

	public void RestartMinigame()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.ContinueGame();
			Singleton<GameController>.Instance.SetMinigameActive(value: false);
		}
		pianoQuestUI.HideSettingsUI();
		backingTrackLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		CleanupMinigameData();
		state = PianoMinigameState.Uninitialized;
		SetupGame(isRestart: true);
	}

	public void CancelMinigame()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.ContinueGame();
			Singleton<GameController>.Instance.SetMinigameActive(value: false);
		}
		pianoQuestUI.HideSettingsUI();
		backingTrackLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		CleanupMinigameData();
		Singleton<MusicController>.Instance.SetLivingRoomMiniGameOnState(isOn: false);
		state = PianoMinigameState.Uninitialized;
		pianoQuestUI.gameObject.SetActive(value: false);
		Singleton<MusicController>.Instance.StartMusic(LevelMusic.LivingRoom);
		onMinigameCancelled?.Invoke();
	}

	private void GameController_OnPauseGame(object sender, EventArgs e)
	{
		pianoQuestUI.gameObject.SetActive(value: false);
		PlayPauseBackingTrack(pauseTrack: true);
	}

	private void GameController_OnContinueGame(object sender, EventArgs e)
	{
		pianoQuestUI.gameObject.SetActive(IsMinigameActive());
		PlayPauseBackingTrack(pauseTrack: false);
	}

	public void OnStringHit(int stringID)
	{
		if ((state == PianoMinigameState.Running || state == PianoMinigameState.WaitingForEnd) && spawnedShmoops.ContainsKey(stringID) && spawnedShmoops[stringID].Count != 0)
		{
			noteSound.Play();
			noteSound.SetParameter("minigame_note_index", stringID);
			PianoMinigameShmoop pianoMinigameShmoop = spawnedShmoops[stringID][0];
			float posDiff = Math.Abs(Vector3.Dot(judgmentLine.position - pianoMinigameShmoop.transform.position, judgmentLine.forward));
			if (isGodMode)
			{
				posDiff = 0f;
			}
			HitQuality hitQuality = EvaluateShmoopHitQuality(posDiff);
			spawnedShmoops[stringID].Remove(pianoMinigameShmoop);
			pianoMinigameShmoop.Jump();
			UnityEngine.Object.Instantiate(pianoMinigameFeedbackPrefab, pianoMinigameShmoop.transform.position, Quaternion.identity, null).SetFeedbackText(hitQuality.ToString());
			if (state == PianoMinigameState.WaitingForEnd)
			{
				CheckGameEnd();
			}
		}
	}
}
