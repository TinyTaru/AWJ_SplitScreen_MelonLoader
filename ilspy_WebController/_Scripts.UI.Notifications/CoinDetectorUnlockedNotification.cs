using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace _Scripts.UI.Notifications;

public class CoinDetectorUnlockedNotification : MonoBehaviour, IQueueableNotification
{
	[Header("References")]
	[SerializeField]
	private Animator animator;

	[Header("Sounds")]
	[SerializeField]
	private EventReference coinDetectorUnlockedSoundRef;

	public event System.EventHandler OnPopUpCompleted;

	public void ShowMessage()
	{
		animator.SetTrigger("Play");
	}

	public void PlaySound()
	{
		EventInstance eventInstance = RuntimeManager.CreateInstance(coinDetectorUnlockedSoundRef);
		eventInstance.start();
		eventInstance.release();
	}

	public void OnAnimationFinished()
	{
		this.OnPopUpCompleted?.Invoke(this, EventArgs.Empty);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
