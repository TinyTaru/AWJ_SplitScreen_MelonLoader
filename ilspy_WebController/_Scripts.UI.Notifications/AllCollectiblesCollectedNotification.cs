using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace _Scripts.UI.Notifications;

public class AllCollectiblesCollectedNotification : MonoBehaviour, IQueueableNotification
{
	[Header("References")]
	[SerializeField]
	private Animator animator;

	[Header("Sounds")]
	[SerializeField]
	private EventReference allCollectiblesCollectedSoundRef;

	public event System.EventHandler OnPopUpCompleted;

	public void ShowMessage()
	{
		animator.SetTrigger("Play");
	}

	public void PlaySound()
	{
		EventInstance eventInstance = RuntimeManager.CreateInstance(allCollectiblesCollectedSoundRef);
		eventInstance.start();
		eventInstance.release();
	}

	public void OnAnimationFinished()
	{
		this.OnPopUpCompleted?.Invoke(this, EventArgs.Empty);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
