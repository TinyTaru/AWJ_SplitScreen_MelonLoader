using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using _Scripts.Game;
using _Scripts.General;
using _Scripts.LivingRoom;
using _Scripts.Spider;
using _Scripts.UI.MobileMonetization;

namespace _Scripts.Singletons;

public class GameController : Singleton<GameController>
{
	public class OnControlSchemeChangedEventArgs : EventArgs
	{
		public bool InputIsKeyboardMouse;
	}

	public class OnGameStateChangedEventArgs : EventArgs
	{
		public GameState State;
	}

	public enum GameState
	{
		Running,
		Paused,
		SelectEmote,
		PerformEmote,
		Dialogue,
		Cutscene,
		FreeLook,
		Respawn,
		Trailer,
		LevelFinished,
		Debugging,
		FinishCutscene
	}

	[SerializeField]
	private LevelMusic levelMusic = LevelMusic.Kitchen;

	[Header("Respawn")]
	[SerializeField]
	private FadeAnimation respawnFadeAnimation;

	[SerializeField]
	private Transform resetPoint;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference pauseInputAction;

	[SerializeField]
	private InputActionReference continueDialogueInputAction;

	[SerializeField]
	private InputActionReference toggleToDoListInputAction;

	[SerializeField]
	private InputActionReference toggleHUDInputAction;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onPause;

	[SerializeField]
	private UnityEvent onContinue;

	private List<BodyMovement> spiders;

	private BodyMovement player;

	private PlayerInput playerInputRef;

	private Transform respawnTransform;

	private Button currentContinueButton;

	private bool canContinueDialogue = true;

	private GameState lastState;

	private GameState state;

	private bool minigameActive;

	public bool InputIsKeyboardMouse => playerInputRef.currentControlScheme == "Keyboard And Mouse";

	public bool InputIsGamepad => playerInputRef.currentControlScheme == "Gamepad";

	public bool InputIsTouch => playerInputRef.currentControlScheme == "Touch";

	public GameState State
	{
		get
		{
			return Singleton<GameController>.Instance.state;
		}
		set
		{
			switch (value)
			{
			case GameState.Running:
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
				break;
			case GameState.Paused:
				Singleton<GameController>.Instance.player.ResetInput();
				break;
			case GameState.Dialogue:
				Singleton<GameController>.Instance.player.ResetInput();
				Singleton<CameraController>.Instance.SetDampingMode(CameraDamping.Default);
				break;
			case GameState.Cutscene:
				Singleton<GameController>.Instance.player.ResetInput();
				Singleton<CameraController>.Instance.SetDampingMode(CameraDamping.Cutscene);
				break;
			case GameState.Debugging:
				Singleton<GameController>.Instance.player.ResetInput();
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
				break;
			default:
				throw new ArgumentOutOfRangeException("value", value, null);
			case GameState.SelectEmote:
			case GameState.PerformEmote:
			case GameState.FreeLook:
			case GameState.Respawn:
			case GameState.Trailer:
			case GameState.LevelFinished:
			case GameState.FinishCutscene:
				break;
			}
			Singleton<GameController>.Instance.lastState = Singleton<GameController>.Instance.state;
			Singleton<GameController>.Instance.state = value;
			Singleton<GameController>.Instance.OnGameStateChanged?.Invoke(this, new OnGameStateChangedEventArgs
			{
				State = value
			});
		}
	}

	public BodyMovement Player => player;

	public List<BodyMovement> Spiders => spiders;

	public GameState LastState => Singleton<GameController>.Instance.lastState;

	public bool IsMonolog { get; set; }

	public LevelMusic Music
	{
		get
		{
			return levelMusic;
		}
		set
		{
			levelMusic = value;
		}
	}

	public bool MinigameActive => minigameActive;

	public event EventHandler OnToggleTaskList;

	public event EventHandler OnToggleHUD;

	public event EventHandler OnOpenWardrobeDirectly;

	public event EventHandler OnPauseGame;

	public event EventHandler OnContinueGame;

	public event EventHandler OnRespawnPlayer;

	public event EventHandler OnResetPlayer;

	public event EventHandler<OnControlSchemeChangedEventArgs> OnControlSchemeChanged;

	public event EventHandler<OnGameStateChangedEventArgs> OnGameStateChanged;

	protected override void Awake()
	{
		base.Awake();
		Singleton<GameController>.Instance.playerInputRef = GetComponent<PlayerInput>();
		Singleton<GameController>.Instance.spiders = new List<BodyMovement>();
		BodyMovement[] array = UnityEngine.Object.FindObjectsByType<BodyMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (BodyMovement bodyMovement in array)
		{
			spiders.Add(bodyMovement);
			if (bodyMovement.IsPlayer && bodyMovement.gameObject.activeSelf)
			{
				player = bodyMovement;
			}
		}
	}

	private void OnEnable()
	{
		pauseInputAction.action.performed += PauseInputAction_OnPerformed;
		continueDialogueInputAction.action.performed += ContinueDialogue_OnPerformed;
		toggleToDoListInputAction.action.performed += ToggleToDoList_OnPerformed;
		toggleHUDInputAction.action.performed += ToggleHUD_OnPerformed;
		playerInputRef.onControlsChanged += PlayerInputRef_OnControlsChanged;
	}

	private void OnDisable()
	{
		pauseInputAction.action.performed -= PauseInputAction_OnPerformed;
		continueDialogueInputAction.action.performed -= ContinueDialogue_OnPerformed;
		toggleToDoListInputAction.action.performed -= ToggleToDoList_OnPerformed;
		toggleHUDInputAction.action.performed -= ToggleHUD_OnPerformed;
		playerInputRef.onControlsChanged -= PlayerInputRef_OnControlsChanged;
	}

	private void Start()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		Singleton<MusicController>.Instance.StartMusic(levelMusic);
		Singleton<GameController>.Instance.State = GameState.Running;
		if (resetPoint == null)
		{
			GameObject gameObject = GameObject.Find("ResetPoint");
			if (gameObject != null)
			{
				resetPoint = gameObject.transform;
			}
			else
			{
				GameObject gameObject2 = new GameObject("ResetPoint");
				gameObject2.transform.SetPositionAndRotation(player.transform.position, player.transform.rotation);
				resetPoint = gameObject2.transform;
			}
		}
		SwitchInputMap("Spider");
	}

	private IEnumerator OpenWardrobeCoroutine()
	{
		yield return null;
		Singleton<GameController>.Instance.State = GameState.Paused;
		Singleton<MusicController>.Instance.Pause();
		Time.timeScale = 0f;
		SwitchInputMap("UI");
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		Singleton<GameController>.Instance.OnOpenWardrobeDirectly?.Invoke(this, EventArgs.Empty);
	}

	private IEnumerator DelayContinueDialogueCoroutine()
	{
		canContinueDialogue = false;
		yield return new WaitForSeconds(0.1f);
		canContinueDialogue = true;
	}

	public void SwitchInputMap(string map)
	{
		if (Singleton<GameController>.Instance.playerInputRef.inputIsActive)
		{
			Singleton<GameController>.Instance.playerInputRef.SwitchCurrentActionMap(map);
		}
	}

	public void PauseGame()
	{
		if ((Singleton<GameController>.Instance.State != 0 && Singleton<GameController>.Instance.State != GameState.FreeLook) || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			return;
		}
		Singleton<GameController>.Instance.State = GameState.Paused;
		Singleton<MusicController>.Instance.Pause();
		Time.timeScale = 0f;
		SwitchInputMap("UI");
		Cursor.lockState = ((!(Cursor.visible = Singleton<GameController>.Instance.State == GameState.Paused)) ? CursorLockMode.Locked : CursorLockMode.None);
		KitchenRaceController kitchenRaceController = UnityEngine.Object.FindObjectsByType<KitchenRaceController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
		if (kitchenRaceController != null && kitchenRaceController.State == KitchenRaceController.RaceState.Running)
		{
			kitchenRaceController.ShowRaceSettings();
			return;
		}
		PianoMinigame pianoMinigame = UnityEngine.Object.FindObjectsByType<PianoMinigame>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
		if (pianoMinigame != null && pianoMinigame.IsMinigameActive())
		{
			pianoMinigame.ShowMinigameSettings();
			return;
		}
		Singleton<GameController>.Instance.onPause.Invoke();
		Singleton<GameController>.Instance.OnPauseGame?.Invoke(this, EventArgs.Empty);
	}

	public void ContinueGame()
	{
		if (Singleton<GameController>.Instance.State == GameState.Paused)
		{
			Singleton<GameController>.Instance.State = Singleton<GameController>.Instance.lastState;
			Singleton<MusicController>.Instance.Continue();
			Time.timeScale = 1f;
			SwitchInputMap("Spider");
			Cursor.lockState = ((!(Cursor.visible = Singleton<GameController>.Instance.State == GameState.Paused)) ? CursorLockMode.Locked : CursorLockMode.None);
			Singleton<GameController>.Instance.onContinue.Invoke();
			Singleton<GameController>.Instance.OnContinueGame?.Invoke(this, EventArgs.Empty);
		}
	}

	public void MovePlayer(Vector3 position)
	{
		player.Respawn(position, player.transform.rotation);
	}

	public void RespawnPlayer(Transform newRespawnTransform)
	{
		Singleton<WebController>.Instance.DestroyAllPlayerWebs();
		RespawnPlayer(newRespawnTransform, playSound: true);
		this.OnRespawnPlayer?.Invoke(this, EventArgs.Empty);
	}

	public void RespawnPlayer(Transform newRespawnTransform, bool playSound)
	{
		if (Singleton<GameController>.Instance.State == GameState.Respawn)
		{
			return;
		}
		Singleton<GameController>.Instance.respawnTransform = newRespawnTransform;
		Singleton<WebController>.Instance.DestroyAllPlayerWebs();
		if (playSound)
		{
			if (newRespawnTransform == null)
			{
				Singleton<MusicController>.Instance.GeneralSoundList.Respawn.start();
			}
			else
			{
				Singleton<MusicController>.Instance.GeneralSoundList.FireHurt.start();
			}
		}
		Time.timeScale = 0f;
		Singleton<WebController>.Instance.Respawn();
		if (Singleton<DebugController>.Instance != null)
		{
			Singleton<DebugController>.Instance.CloseConsole();
		}
		Singleton<GameController>.Instance.respawnFadeAnimation.StartFadeOutAnimation();
		Singleton<GameController>.Instance.State = GameState.Respawn;
	}

	public void FadeOutFinished()
	{
		Singleton<GameController>.Instance.player.Respawn(Singleton<GameController>.Instance.respawnTransform);
		Singleton<GameController>.Instance.respawnFadeAnimation.StartFadeInAnimation();
		Time.timeScale = 1f;
		Singleton<CameraController>.Instance.OnRespawn();
	}

	public void FadeInFinished()
	{
		Singleton<GameController>.Instance.State = GameState.Running;
	}

	public static void StartDialogue()
	{
		Singleton<GameController>.Instance.State = GameState.Dialogue;
		Singleton<GameController>.Instance.player.ResetInput();
		Singleton<GameController>.Instance.StartCoroutine(Singleton<GameController>.Instance.DelayContinueDialogueCoroutine());
	}

	public static void EndDialogue()
	{
		Singleton<GameController>.Instance.State = GameState.Running;
		Singleton<GameController>.Instance.player.GetComponentInChildren<SpiderInteraction>().StartInteractionCooldown();
	}

	public void OpenWardrobe()
	{
		Singleton<GameController>.Instance.StartCoroutine(Singleton<GameController>.Instance.OpenWardrobeCoroutine());
	}

	public void ResetPlayer()
	{
		if (!(Singleton<GameController>.Instance.resetPoint == null))
		{
			ContinueGame();
			RespawnPlayer(Singleton<GameController>.Instance.resetPoint, playSound: false);
			this.OnResetPlayer?.Invoke(this, EventArgs.Empty);
		}
	}

	public void SetRespawnPosition(Transform newRespawnPosition)
	{
		Singleton<GameController>.Instance.player.SetRespawnPosition(newRespawnPosition);
	}

	public void SetResetPosition(Transform newResetPoint)
	{
		Singleton<GameController>.Instance.resetPoint = newResetPoint;
	}

	public void ResumeLastState()
	{
		Singleton<GameController>.Instance.State = Singleton<GameController>.Instance.lastState;
	}

	public Transform GetSpiderPosition(string spiderName)
	{
		foreach (BodyMovement spider in spiders)
		{
			if (!(spider == null) && spider.name.ToUpper().Contains(spiderName.ToUpper()))
			{
				return spider.transform;
			}
		}
		return null;
	}

	public void SetCurrentContinueButton(Button button)
	{
		Singleton<GameController>.Instance.currentContinueButton = button;
	}

	public void ContinueDialogue()
	{
		if (canContinueDialogue && (state == GameState.Dialogue || state == GameState.Cutscene) && Singleton<GameController>.Instance.currentContinueButton != null)
		{
			Singleton<GameController>.Instance.currentContinueButton.onClick.Invoke();
		}
	}

	public void ToggleTaskListMobile()
	{
	}

	public void SetMinigameActive(bool value)
	{
		minigameActive = value;
	}

	private void PauseInputAction_OnPerformed(InputAction.CallbackContext ctx)
	{
		if (ctx.ReadValueAsButton())
		{
			PauseGame();
		}
	}

	private void ContinueDialogue_OnPerformed(InputAction.CallbackContext ctx)
	{
		if (canContinueDialogue && (state == GameState.Dialogue || state == GameState.Cutscene) && ctx.ReadValueAsButton())
		{
			ContinueDialogue();
		}
	}

	private void ToggleToDoList_OnPerformed(InputAction.CallbackContext ctx)
	{
		this.OnToggleTaskList?.Invoke(this, EventArgs.Empty);
	}

	private void ToggleHUD_OnPerformed(InputAction.CallbackContext ctx)
	{
		this.OnToggleHUD?.Invoke(this, EventArgs.Empty);
	}

	private void PlayerInputRef_OnControlsChanged(PlayerInput playerInput)
	{
		this.OnControlSchemeChanged?.Invoke(this, new OnControlSchemeChangedEventArgs
		{
			InputIsKeyboardMouse = InputIsKeyboardMouse
		});
	}
}
