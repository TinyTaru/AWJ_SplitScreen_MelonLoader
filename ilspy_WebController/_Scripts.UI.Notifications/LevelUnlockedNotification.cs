using System;
using FMOD.Studio;
using FMODUnity;
using MPUIKIT;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Notifications;

public class LevelUnlockedNotification : MonoBehaviour, IQueueableNotification
{
	[Header("References")]
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private MPImage levelSprite;

	[SerializeField]
	private TextMeshProUGUI messageText;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private EventReference itemUnlockedSoundRef;

	public event System.EventHandler OnPopUpCompleted;

	public void ShowMessage()
	{
		animator.SetTrigger("Play");
	}

	public void PlaySound()
	{
		EventInstance eventInstance = RuntimeManager.CreateInstance(itemUnlockedSoundRef);
		eventInstance.start();
		eventInstance.release();
	}

	public void OnAnimationFinished()
	{
		this.OnPopUpCompleted?.Invoke(this, EventArgs.Empty);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void SetMessageText(string message)
	{
		messageText.text = message;
	}

	public void SetTitleText(string message)
	{
		titleText.text = message;
	}

	public void SetLevelSprite(Sprite sprite)
	{
		Color white = Color.white;
		white.a = 1f;
		levelSprite.color = white;
		levelSprite.sprite = sprite;
	}
}
