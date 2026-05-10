using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Scripts.General;
using _Scripts.UI.TaskList;

namespace _Scripts.Singletons;

public class QuestController : Singleton<QuestController>
{
	[Header("Parameters")]
	[SerializeField]
	private TaskList taskListKitchen;

	[SerializeField]
	private TaskList taskListOffice;

	[SerializeField]
	private TaskList taskListKidsRoom;

	[SerializeField]
	private TaskList taskListLivingRoom;

	[SerializeField]
	private int totalQuests;

	private int completedQuests;

	public int TotalQuests => totalQuests;

	protected override void Awake()
	{
		base.Awake();
		LoadQuestData();
	}

	private void Start()
	{
		SceneManager.sceneLoaded += SceneManager_OnSceneLoaded;
		SceneManager.sceneUnloaded += SceneManager_OnSceneUnloaded;
		CheckCompletedQuests();
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= SceneManager_OnSceneLoaded;
		SceneManager.sceneUnloaded -= SceneManager_OnSceneUnloaded;
	}

	private void LoadQuestData()
	{
		Singleton<QuestController>.Instance.completedQuests = SaveController.Load("CompletedQuests", 0, SaveData.Game);
	}

	private void SaveQuestData()
	{
		SaveController.Save("CompletedQuests", Singleton<QuestController>.Instance.completedQuests, SaveData.Game);
	}

	private void CompleteQuest(TaskDataSo taskDataSO)
	{
		if (taskDataSO.partOfTotalQuests && Singleton<SceneController>.Instance.IsStoryLevel)
		{
			Singleton<QuestController>.Instance.completedQuests++;
			Debug.Log($"Completed Quests = {Singleton<QuestController>.Instance.completedQuests}");
			SaveQuestData();
		}
	}

	private int GetCompletedTasks(TaskList taskList)
	{
		int num = 0;
		TaskDataSo[] tasks = taskList.tasks;
		for (int i = 0; i < tasks.Length; i++)
		{
			if (QuestLog.GetQuestState(tasks[i].questName) == QuestState.Success)
			{
				num++;
			}
		}
		return num;
	}

	private void CheckCompletedQuests()
	{
		int completedQuestsKitchen = GetCompletedQuestsKitchen();
		if (completedQuestsKitchen >= 7)
		{
			Singleton<ProfileController>.Instance.UnlockOffice();
		}
		int completedQuestsOffice = GetCompletedQuestsOffice();
		if (completedQuestsOffice >= 7)
		{
			Singleton<ProfileController>.Instance.UnlockKidsRoom();
		}
		int completedQuestsKidsRoom = GetCompletedQuestsKidsRoom();
		if (completedQuestsKidsRoom >= 7)
		{
			Singleton<ProfileController>.Instance.UnlockLivingRoom();
		}
		int num = completedQuestsKitchen + completedQuestsOffice + completedQuestsKidsRoom;
		if (num > Singleton<QuestController>.Instance.completedQuests)
		{
			int num2 = num - Singleton<QuestController>.Instance.completedQuests;
			Singleton<CoinController>.Instance.AddCoins(30 * num2);
			Singleton<QuestController>.Instance.completedQuests = num;
			SaveQuestData();
		}
	}

	public int GetCompletedQuestsKitchen()
	{
		return GetCompletedTasks(taskListKitchen);
	}

	public int GetCompletedQuestsOffice()
	{
		return GetCompletedTasks(taskListOffice);
	}

	public int GetCompletedQuestsKidsRoom()
	{
		return GetCompletedTasks(taskListKidsRoom);
	}

	public int GetCompletedQuestsLivingRoom()
	{
		return GetCompletedTasks(taskListLivingRoom);
	}

	private void SceneManager_OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		LoadQuestData();
		CheckCompletedQuests();
		if (Singleton<TaskListUI>.Instance != null)
		{
			Singleton<TaskListUI>.Instance.OnTaskCompleted += TaskListUI_OnTaskCompleted;
		}
	}

	private void SceneManager_OnSceneUnloaded(Scene arg0)
	{
		if (Singleton<TaskListUI>.Instance != null)
		{
			Singleton<TaskListUI>.Instance.OnTaskCompleted -= TaskListUI_OnTaskCompleted;
		}
	}

	private void TaskListUI_OnTaskCompleted(object sender, TaskListUI.OnTaskCompletedEventArgs e)
	{
		CompleteQuest(e.taskDataSO);
	}
}
