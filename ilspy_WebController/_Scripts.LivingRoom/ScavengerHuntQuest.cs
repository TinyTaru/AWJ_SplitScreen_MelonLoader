using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.Spider;

namespace _Scripts.LivingRoom;

public class ScavengerHuntQuest : MonoBehaviour
{
	[SerializeField]
	[QuestPopup(false)]
	private string questName;

	[SerializeField]
	[VariablePopup(false)]
	private string scavengerHuntHintsFoundVariableName;

	[SerializeField]
	private BodyMovement gloria;

	[SerializeField]
	private Transform gloriaTempleTransform;

	private ScavengerHuntHintPiano hintPiano;

	private Aquarium aquarium;

	private FirePlace firePlace;

	private SpecialCouch specialCouch;

	private ScavengerHuntQuestFinishedTrigger finishTrigger;

	private int hintCounter;

	private void Start()
	{
		hintCounter = 0;
		hintPiano = Object.FindFirstObjectByType<ScavengerHuntHintPiano>();
		if (hintPiano != null)
		{
			hintPiano.OnHintFound += HintPiano_OnHintFound;
		}
		aquarium = Object.FindFirstObjectByType<Aquarium>();
		if (aquarium != null)
		{
			aquarium.OnScavengerHuntHintVisible += Aquarium_OnScavengerHuntHintVisible;
		}
		firePlace = Object.FindFirstObjectByType<FirePlace>();
		if (firePlace != null)
		{
			firePlace.OnScavengerHuntHintVisible += FirePlace_OnScavengerHuntHintVisible;
		}
		specialCouch = Object.FindFirstObjectByType<SpecialCouch>();
		if (specialCouch != null)
		{
			specialCouch.OnCorrectCode += SpecialCouch_OnCorrectCode;
		}
		finishTrigger = Object.FindFirstObjectByType<ScavengerHuntQuestFinishedTrigger>();
		if (finishTrigger != null)
		{
			finishTrigger.OnQuestFinished += FinishTrigger_OnQuestFinished;
		}
	}

	private void UpdateHintVariable()
	{
		DialogueLua.SetVariable(scavengerHuntHintsFoundVariableName, hintCounter);
	}

	private void HintPiano_OnHintFound()
	{
		if (QuestLog.GetQuestState(questName) != QuestState.Success && hintCounter < 1)
		{
			hintCounter = 1;
			UpdateHintVariable();
		}
	}

	private void Aquarium_OnScavengerHuntHintVisible()
	{
		if (QuestLog.GetQuestState(questName) != QuestState.Success && hintCounter < 2)
		{
			hintCounter = 2;
			UpdateHintVariable();
			QuestLog.SetQuestState(questName, QuestState.Active);
		}
	}

	private void FirePlace_OnScavengerHuntHintVisible()
	{
		if (QuestLog.GetQuestState(questName) != QuestState.Success && hintCounter < 3)
		{
			hintCounter = 3;
			UpdateHintVariable();
			QuestLog.SetQuestState(questName, QuestState.Active);
		}
	}

	private void SpecialCouch_OnCorrectCode()
	{
		if (QuestLog.GetQuestState(questName) != QuestState.Success && hintCounter < 4)
		{
			hintCounter = 4;
			UpdateHintVariable();
			QuestLog.SetQuestState(questName, QuestState.Active);
		}
	}

	private void FinishTrigger_OnQuestFinished()
	{
		if (QuestLog.GetQuestState(questName) != QuestState.Success)
		{
			QuestLog.SetQuestState(questName, QuestState.Success);
			gloria.Respawn(gloriaTempleTransform);
		}
	}
}
