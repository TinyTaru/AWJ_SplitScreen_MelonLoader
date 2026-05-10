using System;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using MPUIKIT;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Notifications;

public class WishlistNotification : MonoBehaviour, IQueueableNotification
{
	[Header("References")]
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private RectTransform iconTransform;

	[SerializeField]
	private MPImage iconColor;

	[SerializeField]
	private MPImage hatSprite;

	[SerializeField]
	private RectTransform messageTransform;

	[SerializeField]
	private TextMeshProUGUI messageText;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private float displayDuration = 3f;

	[SerializeField]
	private EventReference itemUnlockedSoundRef;

	[Header("Positions")]
	[SerializeField]
	private int startScale;

	[SerializeField]
	private int endScale = 1;

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
}
