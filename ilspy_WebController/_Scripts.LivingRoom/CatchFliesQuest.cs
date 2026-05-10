using FMODUnity;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace _Scripts.LivingRoom;

public class CatchFliesQuest : MonoBehaviour
{
	[SerializeField]
	[QuestPopup(false)]
	private string questName;

	[SerializeField]
	private int fliesToCatch = 5;

	[SerializeField]
	private StudioEventEmitter flyCaughtSound;

	[SerializeField]
	private StudioEventEmitter flyReleasedSound;

	private Fly[] flies;

	private bool questFinished;

	private int fliesCaught;

	private void Start()
	{
		flies = Object.FindObjectsByType<Fly>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		Fly[] array = flies;
		foreach (Fly obj in array)
		{
			obj.OnFlyCaught += Fly_OnFlyCaught;
			obj.OnFlyReleased += Fly_OnFlyReleased;
		}
	}

	private void Fly_OnFlyCaught()
	{
		if (!questFinished)
		{
			fliesCaught++;
			flyCaughtSound.Play();
			if (fliesCaught >= fliesToCatch)
			{
				FinishQuest();
			}
		}
	}

	private void Fly_OnFlyReleased()
	{
		if (!questFinished)
		{
			fliesCaught--;
			fliesCaught = Mathf.Max(0, fliesCaught);
			flyReleasedSound.Play();
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
}
