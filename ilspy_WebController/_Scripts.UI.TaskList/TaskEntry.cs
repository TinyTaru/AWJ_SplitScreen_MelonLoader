using DG.Tweening;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.UI.Animations;

namespace _Scripts.UI.TaskList;

public class TaskEntry : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Image checkboxChecked;

	[SerializeField]
	private string questName;

	private TaskDataSo taskDataSo;

	private bool isDone;

	private bool wasDone;

	private void SetTaskToDone()
	{
		isDone = true;
		UIAnimation.CreateSequence().Append(UIAnimation.AnimateFillAmount(checkboxChecked, 1f, 0.5f)).OnComplete(delegate
		{
			label.fontStyle |= FontStyles.Strikethrough;
		});
	}

	public void Setup(TaskDataSo taskDataSO)
	{
		taskDataSo = taskDataSO;
		label.text = DialogueManager.GetLocalizedText(taskDataSO.text);
		questName = taskDataSO.questName;
		isDone = QuestLog.GetQuestState(taskDataSo.questName) == QuestState.Success;
		checkboxChecked.fillAmount = (isDone ? 1f : 0f);
		if (isDone)
		{
			label.fontStyle |= FontStyles.Strikethrough;
		}
	}

	public TaskDataSo GetTaskData()
	{
		return taskDataSo;
	}

	public bool CheckIfDone(out bool justCompleted)
	{
		justCompleted = false;
		if (isDone)
		{
			return true;
		}
		if (QuestLog.GetQuestState(questName) == QuestState.Success)
		{
			justCompleted = true;
			SetTaskToDone();
			return true;
		}
		return false;
	}

	public void TranslateText()
	{
		label.text = DialogueManager.GetLocalizedText(taskDataSo.text);
	}

	public void ChangeFont(TMP_FontAsset newFont)
	{
		label.font = newFont;
	}
}
