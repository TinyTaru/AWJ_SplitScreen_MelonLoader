using System;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using _Scripts.Utils;

namespace _Scripts.Office;

public class NerfGunUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private NerfGunMinigame nerfGunMinigame;

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
	private string officeNerfGunQuest;

	[SerializeField]
	[VariablePopup(false)]
	private string scoreToBeatVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string highscoreVariable;

	private void Start()
	{
		canvas.SetActive(value: false);
		if (nerfGunMinigame != null)
		{
			nerfGunMinigame.OnMiniGameStarted += NerfGunMinigame_OnMiniGameStarted;
			nerfGunMinigame.OnMiniGameStopped += NerfGunMinigame_OnMiniGameStopped;
			nerfGunMinigame.OnTimerChanged += NerfGunMinigame_OnTimerChanged;
			nerfGunMinigame.OnScoreChanged += NerfGunMinigame_OnScoreChanged;
		}
	}

	private void NerfGunMinigame_OnScoreChanged(object sender, NerfGunMinigame.OnScoreChangedEventArgs e)
	{
		minigameScoreText.text = e.score.ToString();
	}

	private void NerfGunMinigame_OnTimerChanged(object sender, NerfGunMinigame.OnTimerChangedEventArgs e)
	{
		minigameTimerText.text = _Scripts.Utils.Utils.FormatTime(e.timer, 1);
	}

	private void NerfGunMinigame_OnMiniGameStopped(object sender, EventArgs e)
	{
		canvas.SetActive(value: false);
	}

	private void NerfGunMinigame_OnMiniGameStarted(object sender, EventArgs e)
	{
		canvas.SetActive(value: true);
		QuestState questState = QuestLog.GetQuestState(officeNerfGunQuest);
		if (questState == QuestState.Unassigned || questState == QuestState.Active)
		{
			minigameScoreToBeatText.text = string.Format("{0}{1}", DialogueManager.GetLocalizedText("Misc_Score to beat"), DialogueLua.GetVariable(scoreToBeatVariable, 0));
		}
		else if (QuestLog.GetQuestState(officeNerfGunQuest) == QuestState.Success)
		{
			minigameScoreToBeatText.text = string.Format("{0}{1}", DialogueManager.GetLocalizedText("Misc_Highscore"), DialogueLua.GetVariable(highscoreVariable, 0));
		}
	}
}
