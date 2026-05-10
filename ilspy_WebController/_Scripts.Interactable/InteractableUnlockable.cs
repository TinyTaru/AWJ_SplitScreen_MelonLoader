using System;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.MobileMonetization;

namespace _Scripts.Interactable;

public class InteractableUnlockable : MonoBehaviour, IInteractable
{
	[SerializeField]
	private bool canInteract = true;

	[Header("References")]
	[SerializeField]
	private GameObject interactionPrompt;

	[SerializeField]
	private GameObject defaultButton;

	[SerializeField]
	private GameObject mobileButton;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onInteractEvent;

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
			onInteractEvent?.Invoke();
		}
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
