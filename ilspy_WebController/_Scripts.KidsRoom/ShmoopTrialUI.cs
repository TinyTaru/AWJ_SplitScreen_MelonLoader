using System;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using _Scripts.Utils;

namespace _Scripts.KidsRoom;

public class ShmoopTrialUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private ShmoopTrialMinigame shmoopTrialMinigame;

	[SerializeField]
	private GameObject canvas;

	[SerializeField]
	private TextMeshProUGUI minigameScoreText;

	[SerializeField]
	private TextMeshProUGUI minigameScoreToBeatText;

	[SerializeField]
	private TextMeshProUGUI minigameTimerText;

	[Header("Dialogue System References")]
	[SerializeField]
	[QuestPopup(false)]
	private string shmoopTrialQuest;

	[SerializeField]
	[VariablePopup(false)]
	private string scoreToBeatVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string highscoreVariable;

	private void Start()
	{
		canvas.SetActive(value: false);
		if (shmoopTrialMinigame != null)
		{
			shmoopTrialMinigame.OnMiniGameStarted += ShmoopTrialMinigame_OnMiniGameStarted;
			shmoopTrialMinigame.OnMiniGameStopped += ShmoopTrialMinigame_OnMiniGameStopped;
			shmoopTrialMinigame.OnTimerChanged += ShmoopTrialMinigame_OnTimerChanged;
			shmoopTrialMinigame.OnScoreChanged += ShmoopTrialMinigame_OnScoreChanged;
		}
	}

	private void ShmoopTrialMinigame_OnScoreChanged(object sender, ShmoopTrialMinigame.OnScoreChangedEventArgs e)
	{
		minigameScoreText.text = e.score.ToString();
	}

	private void ShmoopTrialMinigame_OnTimerChanged(object sender, ShmoopTrialMinigame.OnTimerChangedEventArgs e)
	{
		minigameTimerText.text = _Scripts.Utils.Utils.FormatTime(e.timer, 1);
	}

	private void ShmoopTrialMinigame_OnMiniGameStopped(object sender, EventArgs e)
	{
		canvas.SetActive(value: false);
	}

	private void ShmoopTrialMinigame_OnMiniGameStarted(object sender, EventArgs e)
	{
		canvas.SetActive(value: true);
		QuestState questState = QuestLog.GetQuestState(shmoopTrialQuest);
		if (questState == QuestState.Unassigned || questState == QuestState.Active)
		{
			minigameScoreToBeatText.text = string.Format("{0}{1}", DialogueManager.GetLocalizedText("Misc_Score to beat"), DialogueLua.GetVariable(scoreToBeatVariable, 0));
		}
		else if (QuestLog.GetQuestState(shmoopTrialQuest) == QuestState.Success)
		{
			minigameScoreToBeatText.text = string.Format("{0}{1}", DialogueManager.GetLocalizedText("Misc_Highscore"), DialogueLua.GetVariable(highscoreVariable, 0));
		}
	}
}
