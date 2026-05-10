using FMODUnity;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.Objects;
using _Scripts.Singletons;

namespace _Scripts.Kitchen;

public class CleanFloorQuest : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private StudioEventEmitter cleaningFinishedSoundEmitter;

	[Header("Parameters")]
	[SerializeField]
	private int splatsToClean = 5;

	[SerializeField]
	[QuestPopup(false)]
	private string questName;

	private int splatsCleaned;

	private bool questFinished;

	private void Start()
	{
		Splat[] array = Object.FindObjectsByType<Splat>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnSplatCleanedEvent += Splat_OnSplatCleanedEvent;
		}
	}

	private void Splat_OnSplatCleanedEvent(object sender, Splat.OnSplatCleanedEventEventArgs e)
	{
		if (questFinished || !Singleton<SceneController>.Instance.IsStoryLevel)
		{
			return;
		}
		if (QuestLog.GetQuestState(questName) == QuestState.Success)
		{
			questFinished = true;
			return;
		}
		if (!e.cleanedByKitchenWater)
		{
			cleaningFinishedSoundEmitter.Play();
		}
		splatsCleaned++;
		if (splatsCleaned >= splatsToClean)
		{
			questFinished = true;
			QuestLog.SetQuestState(questName, QuestState.Success);
		}
	}
}
