using System;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.Singletons;
using _Scripts.UI.Animations;
using _Scripts.UI.TaskList;

namespace _Scripts.UI.Notifications;

public class TaskCompleteNotification : MonoBehaviour, IQueueableNotification
{
	[Header("References")]
	[SerializeField]
	private RectTransform popUpRectTransform;

	[SerializeField]
	private TextMeshProUGUI messageText;

	[SerializeField]
	private Image checkMark;

	[SerializeField]
	private float displayDuration = 3f;

	[SerializeField]
	private TaskListFontsSo taskListFontsSo;

	[Header("Positions")]
	[SerializeField]
	private int startPos = Screen.height;

	[SerializeField]
	private int endPos = Screen.height / 4;

	[Header("Animation duration")]
	[SerializeField]
	private float inDuration = 0.5f;

	[SerializeField]
	private float outDuration = 0.5f;

	[Header("Easing")]
	[SerializeField]
	private Ease easingTypeInMove = Ease.OutBack;

	[SerializeField]
	private Ease easingTypeOutMove = Ease.InBack;

	[Header("Sounds")]
	[SerializeField]
	private EventReference taskCompleteSoundRef;

	public event System.EventHandler OnPopUpCompleted;

	private void OnEnable()
	{
		popUpRectTransform.transform.position = new Vector2(0f, startPos);
	}

	public void ShowMessage()
	{
		TMP_FontAsset font = taskListFontsSo.dictionary[SystemLanguage.English];
		foreach (KeyValuePair<SystemLanguage, TMP_FontAsset> item in taskListFontsSo.dictionary)
		{
			if (item.Key == SettingsController.Language)
			{
				font = item.Value;
				break;
			}
		}
		messageText.font = font;
		base.gameObject.SetActive(value: true);
		Vector2 vector = new Vector2(0f, startPos);
		Vector2 endValue = new Vector2(0f, endPos);
		popUpRectTransform.anchoredPosition = vector;
		Sequence sequence = UIAnimation.CreateSequence();
		sequence.SetUpdate(isIndependentUpdate: true);
		sequence.Append(UIAnimation.AnimateAnchorPosition(popUpRectTransform, endValue, inDuration, easingTypeInMove)).AppendCallback(delegate
		{
			EventInstance eventInstance = RuntimeManager.CreateInstance(taskCompleteSoundRef);
			eventInstance.start();
			eventInstance.release();
		}).Append(UIAnimation.AnimateFillAmount(checkMark, 1f, 0.2f))
			.AppendInterval(displayDuration / 2f)
			.AppendCallback(delegate
			{
				messageText.fontStyle |= FontStyles.Strikethrough;
			})
			.AppendInterval(displayDuration / 2f)
			.Append(UIAnimation.AnimateAnchorPosition(popUpRectTransform, vector, outDuration, easingTypeOutMove))
			.OnComplete(delegate
			{
				this.OnPopUpCompleted?.Invoke(this, EventArgs.Empty);
				UnityEngine.Object.Destroy(base.gameObject);
			});
		sequence.Play();
	}

	public void SetMessageText(string message)
	{
		messageText.text = DialogueManager.GetLocalizedText(message);
	}
}
