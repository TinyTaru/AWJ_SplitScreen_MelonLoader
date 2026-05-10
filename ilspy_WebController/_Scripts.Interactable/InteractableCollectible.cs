using System;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.MobileMonetization;

namespace _Scripts.Interactable;

public class InteractableCollectible : MonoBehaviour, IInteractable
{
	[SerializeField]
	private bool canInteract = true;

	[SerializeField]
	private GameObject interactionPrompt;

	[SerializeField]
	private GameObject item;

	[SerializeField]
	private CollectibleSO collectibleSO;

	[SerializeField]
	private UnityEvent onInteractEvent;

	[SerializeField]
	private GameObject defaultButton;

	[SerializeField]
	private GameObject mobileButton;

	public event Action<IInteractable> OnInteractableDestroyed;

	private void Start()
	{
		interactionPrompt.SetActive(value: false);
		defaultButton.SetActive(value: true);
		mobileButton.SetActive(value: false);
	}

	private void Update()
	{
		interactionPrompt.transform.rotation = Quaternion.identity;
	}

	private void OnDestroy()
	{
		this.OnInteractableDestroyed?.Invoke(this);
	}

	public void SetCanInteract(bool value)
	{
		canInteract = value;
	}

	public void OnPlayerEnter(SpiderInteraction player)
	{
	}

	public void ShowInteractionPrompt(SpiderInteraction spiderInteraction, bool value)
	{
		if (canInteract && Singleton<GameController>.Instance.State == GameController.GameState.Running && !Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			interactionPrompt.SetActive(value);
		}
	}

	public void HideInteractionPrompt()
	{
		interactionPrompt.SetActive(value: false);
	}

	public void OnInteract()
	{
		if (canInteract)
		{
			interactionPrompt.SetActive(value: false);
			onInteractEvent.Invoke();
		}
	}

	public void ShowCollectNotification()
	{
		switch (SettingsController.Language)
		{
		case SystemLanguage.English:
			_ = DialogueManager.GetLocalizedText(collectibleSO.displayName) + " collected!";
			break;
		case SystemLanguage.German:
			_ = DialogueManager.GetLocalizedText(collectibleSO.displayName) + " gesammelt!";
			break;
		}
		Singleton<MusicController>.Instance.PlaySound("event:/game/general/quest_accepted");
	}

	public void CollectItem()
	{
		DontDestroyMe[] componentsInChildren = item.GetComponentsInChildren<DontDestroyMe>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ResetParent();
		}
		Singleton<WebController>.Instance.ReleaseWeb();
		UnityEngine.Object.Destroy(item);
		canInteract = false;
	}

	public Vector3 Position()
	{
		return base.transform.position;
	}

	public bool CanInteract()
	{
		return canInteract;
	}

	public bool ShowInteractButton()
	{
		return true;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}
}
