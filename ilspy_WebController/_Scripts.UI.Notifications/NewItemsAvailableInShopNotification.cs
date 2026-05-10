using System;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Notifications;

public class NewItemsAvailableInShopNotification : MonoBehaviour, IQueueableNotification
{
	[Header("References")]
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private TextMeshProUGUI messageText;

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
}
