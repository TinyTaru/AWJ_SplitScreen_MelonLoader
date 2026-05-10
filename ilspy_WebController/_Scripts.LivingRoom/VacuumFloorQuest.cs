using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.LivingRoom;

public class VacuumFloorQuest : MonoBehaviour
{
	[SerializeField]
	private int dustBunniesToClean = 5;

	[SerializeField]
	[QuestPopup(false)]
	private string questName = "LivingRoom/VacuumFloor";

	[SerializeField]
	private UnityEvent onQuestProgressEvent;

	[SerializeField]
	private UnityEvent onQuestFinishedEvent;

	private int cleanedDustBunnies;

	private bool questFinished;

	private void Awake()
	{
		cleanedDustBunnies = 0;
		questFinished = false;
	}

	private void Start()
	{
		DustBunny[] array = Object.FindObjectsByType<DustBunny>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnCleanedEvent += DustBunny_OnCleanedEvent;
		}
	}

	private void DustBunny_OnCleanedEvent()
	{
		if (!questFinished)
		{
			cleanedDustBunnies++;
			onQuestProgressEvent?.Invoke();
			if (cleanedDustBunnies >= dustBunniesToClean)
			{
				questFinished = true;
				onQuestFinishedEvent?.Invoke();
			}
		}
	}
}
