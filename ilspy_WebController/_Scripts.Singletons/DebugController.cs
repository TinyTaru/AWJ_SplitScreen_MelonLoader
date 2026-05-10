using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PixelCrushers.DialogueSystem;
using QFSW.QC;
using QFSW.QC.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.General;
using _Scripts.Miscellaneous.Christmas;
using _Scripts.Miscellaneous.Halloween;
using _Scripts.Objects;
using _Scripts.Spider;
using _Scripts.UI.HUD;
using _Scripts.UI.MobileMonetization;
using _Scripts.UI.Utils;

namespace _Scripts.Singletons;

public class DebugController : Singleton<DebugController>
{
	[Header("References")]
	[SerializeField]
	private QuantumConsole quantumConsole;

	[SerializeField]
	private GameObject shmoopPrefab;

	[SerializeField]
	private Transform googlyEyePrefab;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference openDebugInputAction;

	[SerializeField]
	private InputActionReference closeDebugInputAction;

	private void OnEnable()
	{
		quantumConsole.OnDeactivate += QuantumConsole_OnDeactivate;
		openDebugInputAction.action.performed += OnOpenDebugConsole;
		closeDebugInputAction.action.performed += OnCloseDebugConsole;
	}

	private void OnDisable()
	{
		quantumConsole.OnDeactivate -= QuantumConsole_OnDeactivate;
		openDebugInputAction.action.performed -= OnOpenDebugConsole;
		closeDebugInputAction.action.performed -= OnCloseDebugConsole;
	}

	private IEnumerator FinishAllQuestsCoroutine(List<string> questsToFinish, float delay = 1f)
	{
		foreach (string questToFinish in questsToFinish)
		{
			string[] allQuests = QuestLog.GetAllQuests(QuestState.Unassigned | QuestState.Active | QuestState.Success | QuestState.Failure | QuestState.Abandoned | QuestState.Grantable | QuestState.ReturnToNPC);
			foreach (string text in allQuests)
			{
				if (text.ToUpper().Contains(questToFinish.ToUpper()))
				{
					QuestLog.SetQuestState(text, QuestState.Success);
					yield return new WaitForSeconds(delay);
				}
			}
		}
		yield return null;
	}

	private IEnumerator SpawnShmoopCoroutine(int value)
	{
		Transform player = Singleton<GameController>.Instance.Player.transform;
		value = Mathf.Clamp(value, 1, 100);
		for (int i = 0; i < value; i++)
		{
			Object.Instantiate(shmoopPrefab, player.position + player.up * 5f, player.rotation).GetComponent<SpawnableObject>().Setup();
			yield return new WaitForSeconds(0.2f);
		}
		yield return null;
	}

	private void OpenConsole()
	{
		if (!Singleton<SceneController>.Instance.IsStoryLevel)
		{
			Singleton<GameController>.Instance.State = GameController.GameState.Debugging;
			Singleton<DebugController>.Instance.quantumConsole.Activate(shouldFocus: true);
			Singleton<GameController>.Instance.SwitchInputMap("UI");
		}
	}

	[Command("help", Platform.AllPlatforms, MonoTargetType.Single)]
	private void Help()
	{
		quantumConsole.LogToConsole("List of commands:");
		quantumConsole.LogToConsole(" ");
		quantumConsole.LogToConsole("SetWebDistance [value]     Set the web distance to [value].");
		quantumConsole.LogToConsole("SetGravityY [value]        Set the vertical gravity to [value].");
		quantumConsole.LogToConsole("SetGravity [x] [y] [z]     Set the gravity to [x] [y] [z].");
		quantumConsole.LogToConsole("Shmoop                     Spawn a shmoop.");
		quantumConsole.LogToConsole("Shmoops [value]            Spawn [value] shmoops. 100 maximum per command.");
		quantumConsole.LogToConsole("FreeLook                   Toggle between spider and free look camera.");
		quantumConsole.LogToConsole("SetZoom [value]            Set the current zoom distance.");
		quantumConsole.LogToConsole("SetMinZoom [value]         Set the minimum zoom distance.");
		quantumConsole.LogToConsole("SetMaxZoom [value]         Set the maximum zoom distance.");
		quantumConsole.LogToConsole("HideUI                     Hide the UI for nice screenshots.");
		quantumConsole.LogToConsole("SetWaterLevel [value]      Set the water level. Only works in the Kitchen level.");
		quantumConsole.LogToConsole("StartHalloween             Start the Halloween event. Only works in the Kitchen level.");
		quantumConsole.LogToConsole("StartChristmas             Start the Christmas event. Only works in the Kitchen level.");
		quantumConsole.LogToConsole("ResetSnow                  Reset the snow. Only works in the Kitchen level while the Christmas event is active.");
	}

	[Command("SetWebDistance", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SetWebDistance(float value)
	{
		value = Mathf.Max(value, 0f);
		Singleton<WebController>.Instance.SetWebDistance(value);
	}

	[Command("SetGravityY", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SetGravityY(float value)
	{
		Physics.gravity = Vector3.up * value;
	}

	[Command("SetGravity", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SetGravity(float x, float y, float z)
	{
		Physics.gravity = new Vector3(x, y, z);
	}

	[Command("SetZoom", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SetZoom(float value)
	{
		Singleton<CameraController>.Instance.SetCameraZoom(value);
	}

	[Command("SetMinZoom", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SetMinZoom(float value)
	{
		Singleton<CameraController>.Instance.SetMinCameraZoom(value);
	}

	[Command("SetMaxZoom", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SetMaxZoom(float value)
	{
		Singleton<CameraController>.Instance.SetMaxCameraZoom(value);
	}

	[Command("ResetSnow", Platform.AllPlatforms, MonoTargetType.Single)]
	public void ResetSnow()
	{
		if (Singleton<GameController>.Instance.Music == LevelMusic.Kitchen)
		{
			SnowController snowController = Object.FindObjectsByType<SnowController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
			if (snowController != null)
			{
				snowController.ResetSnow();
			}
		}
		else
		{
			quantumConsole.LogToConsole("This command is only supported in the Kitchen level while the Christmas event is active!");
		}
	}

	private void HideUI()
	{
		Version version = Object.FindObjectsByType<Version>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
		if (version != null)
		{
			version.gameObject.SetActive(value: false);
		}
		TaskListHUD taskListHUD = Object.FindObjectsByType<TaskListHUD>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
		if (taskListHUD != null)
		{
			taskListHUD.Disable();
		}
		GameplayUI gameplayUI = Object.FindObjectsByType<GameplayUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
		if (gameplayUI != null)
		{
			gameplayUI.gameObject.SetActive(value: false);
		}
	}

	[Command("SetWaterLevel", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SetWaterLevel(float value)
	{
		if (Singleton<GameController>.Instance.Music == LevelMusic.Kitchen)
		{
			WaterKitchen waterKitchen = Object.FindObjectsByType<WaterKitchen>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
			if (waterKitchen != null)
			{
				waterKitchen.SetWaterLevel(value);
			}
		}
		else
		{
			quantumConsole.LogToConsole("This command is only supported in the Kitchen level for now!");
		}
	}

	[Command("StartChristmas", Platform.AllPlatforms, MonoTargetType.Single)]
	public void StartChristmas()
	{
		if (Singleton<GameController>.Instance.Music == LevelMusic.Kitchen)
		{
			if (Singleton<KitchenChristmasController>.Instance != null)
			{
				Singleton<KitchenChristmasController>.Instance.StartChristmas();
			}
		}
		else
		{
			quantumConsole.LogToConsole("This command is only supported in the Kitchen level!");
		}
	}

	[Command("StartHalloween", Platform.AllPlatforms, MonoTargetType.Single)]
	public void StartHalloween()
	{
		if (Singleton<GameController>.Instance.Music == LevelMusic.Kitchen)
		{
			if (Singleton<KitchenHalloweenController>.Instance != null)
			{
				Singleton<KitchenHalloweenController>.Instance.StartHalloween();
			}
		}
		else
		{
			quantumConsole.LogToConsole("This command is only supported in the Kitchen level!");
		}
	}

	[Command("StopHalloween", Platform.AllPlatforms, MonoTargetType.Single)]
	public void StopHalloween()
	{
		if (Singleton<GameController>.Instance.Music == LevelMusic.Kitchen)
		{
			if (Singleton<KitchenHalloweenController>.Instance != null)
			{
				Singleton<KitchenHalloweenController>.Instance.StopHalloween();
			}
		}
		else
		{
			quantumConsole.LogToConsole("This command is only supported in the Kitchen level!");
		}
	}

	[Command("Shmoop", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SpawnShmoop()
	{
		Transform transform = Singleton<GameController>.Instance.Player.transform;
		Object.Instantiate(shmoopPrefab, transform.position + transform.up * 5f, transform.rotation).GetComponent<SpawnableObject>().Setup();
	}

	[Command("Shmoops", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SpawnShmoops(int value)
	{
		StartCoroutine(SpawnShmoopCoroutine(value));
	}

	[Command("SetBurnAmount", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SetBurnAmount(float value)
	{
		BurnableObject component = Singleton<GameController>.Instance.Player.GetComponent<BurnableObject>();
		if (component != null)
		{
			component.SetBurnAmount(value);
		}
	}

	[Command("SpawnGooglyEye", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SpawnGooglyEye(int size = 100)
	{
		Transform transform = Singleton<GameController>.Instance.Player.transform;
		size = Mathf.Clamp(size, 30, 300);
		float num = (float)size / 100f;
		Transform obj = Object.Instantiate(googlyEyePrefab, transform.position + transform.up * 3f * (1f + num), transform.rotation);
		obj.localScale = Vector3.one * num;
		SpawnableObject component = obj.GetComponent<SpawnableObject>();
		if (component != null)
		{
			component.Setup();
		}
	}

	[Command("SetFollowCameraFOV", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SetFollowCameraFOV(float value)
	{
		value = Mathf.Clamp(value, 1f, 179f);
		Singleton<CameraController>.Instance.SetCinemachineFollowCameraFOV(value);
	}

	[Command("SetFreeLookCameraFOV", Platform.AllPlatforms, MonoTargetType.Single)]
	private void SetFreeLookCameraFOV(float value)
	{
		value = Mathf.Clamp(value, 1f, 179f);
		Singleton<CameraController>.Instance.SetCinemachineFreeLookCameraFOV(value);
	}

	private void ResetPlayer()
	{
		CloseConsole();
		Singleton<GameController>.Instance.ResetPlayer();
	}

	private void RespawnPlayer()
	{
		CloseConsole();
		Singleton<GameController>.Instance.RespawnPlayer(null);
	}

	private void SetWorldOffset(float x, float y, float z)
	{
		Singleton<CameraController>.Instance.FollowCameraFollowTarget.SetWorldOffset(x, y, z);
	}

	private void SetSpiderOffset(float x, float y, float z)
	{
		Singleton<CameraController>.Instance.FollowCameraFollowTarget.SetSpiderOffset(x, y, z);
	}

	private void SetCameraOffset(float x, float y, float z)
	{
		Singleton<CameraController>.Instance.FollowCameraFollowTarget.SetCameraOffset(x, y, z);
	}

	private void ResetCameraOffsets()
	{
		Singleton<CameraController>.Instance.FollowCameraFollowTarget.ResetCameraOffsets();
	}

	[Command("FreeLook", Platform.AllPlatforms, MonoTargetType.Single)]
	private void FreeLook()
	{
		CloseConsole();
		Singleton<CameraController>.Instance.ToggleFreeLookCamera();
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void MoveTo(string spiderName)
	{
		if (!string.IsNullOrEmpty(spiderName))
		{
			Transform spiderPosition = Singleton<GameController>.Instance.GetSpiderPosition(spiderName);
			Transform transform = GameObject.Find(spiderName).transform;
			if (spiderPosition != null)
			{
				Singleton<GameController>.Instance.MovePlayer(spiderPosition.position);
			}
			else if (transform != null)
			{
				Singleton<GameController>.Instance.MovePlayer(transform.position);
			}
			else
			{
				Debug.Log("Couldn't find animal with the name " + spiderName + "!");
			}
		}
	}

	private void ResetQuest(string questName)
	{
		string[] allQuests = QuestLog.GetAllQuests(QuestState.Unassigned | QuestState.Active | QuestState.Success | QuestState.Failure | QuestState.Abandoned | QuestState.Grantable | QuestState.ReturnToNPC);
		foreach (string text in allQuests)
		{
			if (text.ToUpper().Contains(questName.ToUpper()))
			{
				QuestLog.SetQuestState(text, QuestState.Unassigned);
				break;
			}
		}
	}

	private IEnumerator<ICommandAction> ResetQuest()
	{
		List<string> questsForCurrentLevel = GetAllQuestsForCurrentLevel();
		if (questsForCurrentLevel.Count == 0)
		{
			quantumConsole.LogToConsole("No quests found!");
			yield break;
		}
		questsForCurrentLevel.Add("Cancel");
		string selection = null;
		yield return new Value("Pick a quest to reset:");
		Choice<string>.Config @default = Choice<string>.Config.Default;
		@default.Delimiter = "\n";
		yield return new Choice<string>(questsForCurrentLevel, delegate(string s)
		{
			selection = s;
		}, @default);
		if (!(selection == "Cancel"))
		{
			QuestLog.SetQuestState(selection, QuestState.Unassigned);
			quantumConsole.LogToConsole("Reset quest " + selection + "! Please use the 'ReloadCurrentLevel' command!");
		}
	}

	private void StartQuest(string questName)
	{
		string[] allQuests = QuestLog.GetAllQuests(QuestState.Unassigned | QuestState.Active | QuestState.Success | QuestState.Failure | QuestState.Abandoned | QuestState.Grantable | QuestState.ReturnToNPC);
		foreach (string text in allQuests)
		{
			if (text.ToUpper().Contains(questName.ToUpper()))
			{
				QuestLog.SetQuestState(text, QuestState.Active);
				break;
			}
		}
	}

	private void FinishQuest(string questName)
	{
		string[] allQuests = QuestLog.GetAllQuests(QuestState.Unassigned | QuestState.Active | QuestState.Success | QuestState.Failure | QuestState.Abandoned | QuestState.Grantable | QuestState.ReturnToNPC);
		foreach (string text in allQuests)
		{
			if (text.ToUpper().Contains(questName.ToUpper()))
			{
				QuestLog.SetQuestState(text, QuestState.Success);
				break;
			}
		}
	}

	private IEnumerator<ICommandAction> FinishQuest()
	{
		List<string> unfinishedQuestsForCurrentLevel = GetUnfinishedQuestsForCurrentLevel();
		if (unfinishedQuestsForCurrentLevel.Count == 0)
		{
			quantumConsole.LogToConsole("No unfinished quests found!");
			yield break;
		}
		unfinishedQuestsForCurrentLevel.Add("Cancel");
		string selection = null;
		yield return new Value("Pick a quest to finish:");
		Choice<string>.Config @default = Choice<string>.Config.Default;
		@default.Delimiter = "\n";
		yield return new Choice<string>(unfinishedQuestsForCurrentLevel, delegate(string s)
		{
			selection = s;
		}, @default);
		if (!(selection == "Cancel"))
		{
			QuestLog.SetQuestState(selection, QuestState.Success);
			quantumConsole.LogToConsole("Quest " + selection + " set to finished!");
		}
	}

	private List<string> GetUnfinishedQuestsForCurrentLevel()
	{
		string[] allQuests = QuestLog.GetAllQuests(QuestState.Unassigned | QuestState.Active);
		List<string> list = new List<string>();
		switch (Singleton<GameController>.Instance.Music)
		{
		case LevelMusic.Tutorial:
		{
			string[] array = allQuests;
			foreach (string text2 in array)
			{
				if (text2.Contains("Tutorial"))
				{
					list.Add(text2);
				}
			}
			break;
		}
		case LevelMusic.Kitchen:
		{
			string[] array = allQuests;
			foreach (string text4 in array)
			{
				if (!text4.Contains("Kitchen/FriedEgg") && !text4.Contains("Kitchen/Bacon") && !text4.Contains("Kitchen/BrokenKnob") && !text4.Contains("Christmas") && text4.Contains("Kitchen"))
				{
					list.Add(text4);
				}
			}
			array = allQuests;
			foreach (string text5 in array)
			{
				if (text5.Contains("Christmas"))
				{
					list.Add(text5);
				}
			}
			break;
		}
		case LevelMusic.Office:
		{
			string[] array = allQuests;
			foreach (string text6 in array)
			{
				if (text6.Contains("Office"))
				{
					list.Add(text6);
				}
			}
			break;
		}
		case LevelMusic.KidsRoom:
		{
			string[] array = allQuests;
			foreach (string text3 in array)
			{
				if (text3.Contains("KidsRoom"))
				{
					list.Add(text3);
				}
			}
			break;
		}
		case LevelMusic.LivingRoom:
		{
			string[] array = allQuests;
			foreach (string text in array)
			{
				if (text.Contains("LivingRoom"))
				{
					list.Add(text);
				}
			}
			break;
		}
		}
		return list;
	}

	private List<string> GetAllQuestsForCurrentLevel()
	{
		string[] allQuests = QuestLog.GetAllQuests(QuestState.Unassigned | QuestState.Active | QuestState.Success);
		List<string> list = new List<string>();
		switch (Singleton<GameController>.Instance.Music)
		{
		case LevelMusic.Tutorial:
		{
			string[] array = allQuests;
			foreach (string text2 in array)
			{
				if (text2.Contains("Tutorial"))
				{
					list.Add(text2);
				}
			}
			break;
		}
		case LevelMusic.Kitchen:
		{
			string[] array = allQuests;
			foreach (string text4 in array)
			{
				if (!text4.Contains("Kitchen/FriedEgg") && !text4.Contains("Kitchen/Bacon") && !text4.Contains("Kitchen/BrokenKnob") && !text4.Contains("Christmas") && text4.Contains("Kitchen"))
				{
					list.Add(text4);
				}
			}
			array = allQuests;
			foreach (string text5 in array)
			{
				if (text5.Contains("Christmas"))
				{
					list.Add(text5);
				}
			}
			break;
		}
		case LevelMusic.Office:
		{
			string[] array = allQuests;
			foreach (string text6 in array)
			{
				if (text6.Contains("Office"))
				{
					list.Add(text6);
				}
			}
			break;
		}
		case LevelMusic.KidsRoom:
		{
			string[] array = allQuests;
			foreach (string text3 in array)
			{
				if (text3.Contains("KidsRoom"))
				{
					list.Add(text3);
				}
			}
			break;
		}
		case LevelMusic.LivingRoom:
		{
			string[] array = allQuests;
			foreach (string text in array)
			{
				if (text.Contains("LivingRoom"))
				{
					list.Add(text);
				}
			}
			break;
		}
		}
		return list;
	}

	private void MoveToXZ(float x, float z)
	{
		RaycastHit[] array = (from x in Physics.RaycastAll(new Vector3(x, 300f, z), Vector3.down, 400f, Singleton<GameController>.Instance.Player.WhatIsGround).ToList()
			orderby x.point.y descending
			select x).ToArray();
		if (array.Length != 0)
		{
			Debug.Log(array[0].point);
			Singleton<GameController>.Instance.MovePlayer(array[0].point);
		}
	}

	private void SetNight()
	{
		Transform directionalLight = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault().transform;
		Sequence sequence = DOTween.Sequence();
		sequence.Append(directionalLight.DORotate(new Vector3(-100f, -30f, 0f), 2f));
		sequence.OnComplete(delegate
		{
			directionalLight.gameObject.SetActive(value: false);
		});
	}

	private void ResetSpiderRotation()
	{
		Singleton<GameController>.Instance.Player.Root.localRotation = Quaternion.identity;
	}

	private void RotateSpider(float value)
	{
		BodyMovement player = Singleton<GameController>.Instance.Player;
		Vector3 eulerAngles = player.Root.localRotation.eulerAngles;
		eulerAngles += new Vector3(0f, value, 0f);
		player.Root.localRotation = Quaternion.Euler(eulerAngles);
	}

	private void FinishAllQuests()
	{
		List<string> unfinishedQuestsForCurrentLevel = GetUnfinishedQuestsForCurrentLevel();
		CloseConsole();
		StartCoroutine(FinishAllQuestsCoroutine(unfinishedQuestsForCurrentLevel));
	}

	public void CloseConsole()
	{
		if (!Singleton<SceneController>.Instance.IsStoryLevel)
		{
			Singleton<DebugController>.Instance.quantumConsole.Deactivate();
		}
	}

	private void QuantumConsole_OnDeactivate()
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Debugging)
		{
			Singleton<GameController>.Instance.ResumeLastState();
			Singleton<GameController>.Instance.SwitchInputMap("Spider");
		}
	}

	private void OnOpenDebugConsole(InputAction.CallbackContext ctx)
	{
		if (!(Singleton<GameController>.Instance == null) && (Singleton<GameController>.Instance.State == GameController.GameState.Running || Singleton<GameController>.Instance.State == GameController.GameState.FreeLook || !Singleton<PlayTimeCanvas>.Instance.IsOpen))
		{
			OpenConsole();
		}
	}

	private void OnCloseDebugConsole(InputAction.CallbackContext ctx)
	{
		if (!(Singleton<GameController>.Instance == null) && (Singleton<GameController>.Instance.State == GameController.GameState.Debugging || !Singleton<PlayTimeCanvas>.Instance.IsOpen))
		{
			CloseConsole();
		}
	}
}
