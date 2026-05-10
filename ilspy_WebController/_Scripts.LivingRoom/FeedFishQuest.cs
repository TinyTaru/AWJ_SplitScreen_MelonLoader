using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace _Scripts.LivingRoom;

public class FeedFishQuest : MonoBehaviour
{
	[SerializeField]
	[QuestPopup(false)]
	private string questName;

	[SerializeField]
	private int fishToFeed = 5;

	[SerializeField]
	private StudioEventEmitter fishFedSound;

	private List<Fish> fishList;

	private bool questFinished;

	private int fishFed;

	private void Start()
	{
		fishList = Object.FindObjectsByType<Fish>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
		foreach (Fish fish in fishList)
		{
			fish.OnFishFed += Fish_OnFishFed;
		}
	}

	private void Fish_OnFishFed()
	{
		if (!questFinished)
		{
			fishFed++;
			fishFedSound.Play();
			if (fishFed >= fishToFeed)
			{
				FinishQuest();
			}
		}
	}

	private void FinishQuest()
	{
		if (!questFinished)
		{
			questFinished = true;
			QuestLog.SetQuestState(questName, QuestState.Success);
		}
	}

	public void AddFishToList(Fish fish)
	{
		if (!fishList.Contains(fish))
		{
			fish.OnFishFed += Fish_OnFishFed;
			fishList.Add(fish);
		}
	}
}
