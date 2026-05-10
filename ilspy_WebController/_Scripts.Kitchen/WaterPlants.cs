using FMODUnity;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.Objects;
using _Scripts.Singletons;

namespace _Scripts.Kitchen;

public class WaterPlants : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private StudioEventEmitter plantWateredSoundEmitter;

	[Header("Parameters")]
	[SerializeField]
	private int plantsToWater = 3;

	[SerializeField]
	[QuestPopup(false)]
	private string questName;

	private int plantsWatered;

	private bool questFinished;

	private void Start()
	{
		KitchenPlant[] array = Object.FindObjectsByType<KitchenPlant>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnPlantWateredEvent += KitchenPlant_OnPlantWateredEvent;
		}
	}

	private void KitchenPlant_OnPlantWateredEvent(object sender, KitchenPlant.OnPlantWateredEventEventArgs e)
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
		if (!e.wateredByKitchenWater)
		{
			plantWateredSoundEmitter.Play();
		}
		plantsWatered++;
		if (plantsWatered >= plantsToWater)
		{
			questFinished = true;
			QuestLog.SetQuestState(questName, QuestState.Success);
		}
	}
}
