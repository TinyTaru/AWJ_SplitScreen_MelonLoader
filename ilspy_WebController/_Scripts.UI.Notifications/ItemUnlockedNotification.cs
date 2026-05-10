using System;
using FMOD.Studio;
using FMODUnity;
using MPUIKIT;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Notifications;

public class ItemUnlockedNotification : MonoBehaviour, IQueueableNotification
{
	public enum UnlockType
	{
		Hat,
		WebColor
	}

	[Header("References")]
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private MPImage itemImage;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI messageText;

	[SerializeField]
	private GameObject hint;

	[SerializeField]
	private TextMeshProUGUI hintText;

	[Header("Sounds")]
	[SerializeField]
	private EventReference itemUnlockedSoundRef;

	private UnlockType unlockType;

	public event System.EventHandler OnPopUpCompleted;

	public void ShowMessage()
	{
		hint.SetActive(unlockType == UnlockType.WebColor);
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

	public void SetUnlockType(UnlockType unlockType)
	{
		this.unlockType = unlockType;
	}

	public void SetMessageText(string message)
	{
		messageText.text = message;
	}

	public void SetTitleText(string message)
	{
		titleText.text = message;
	}

	public void SetItemSprite(Sprite sprite)
	{
		Color white = Color.white;
		white.a = 1f;
		itemImage.color = white;
		itemImage.sprite = sprite;
	}

	public void SetHintText(string message)
	{
		hintText.text = message;
	}
}
