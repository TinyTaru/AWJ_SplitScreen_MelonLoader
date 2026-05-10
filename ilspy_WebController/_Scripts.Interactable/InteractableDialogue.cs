using System;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using _Scripts.DialogueSystem;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.MobileMonetization;

namespace _Scripts.Interactable;

public class InteractableDialogue : MonoBehaviour, IInteractable
{
	[Header("References")]
	[SerializeField]
	private bool canInteract;

	[SerializeField]
	private GameObject interactionPrompt;

	[SerializeField]
	private _Scripts.DialogueSystem.DialogueActor dialogueActor;

	[SerializeField]
	private UnityEvent onInteractEvent;

	[SerializeField]
	private GameObject nameParent;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI talkText;

	[Header("Parameters")]
	[SerializeField]
	private float brightnessThresholdForWhiteText = 0.5f;

	private SpiderInteraction spiderInteraction;

	private bool isVisible;

	public event Action<IInteractable> OnInteractableDestroyed;

	private void Start()
	{
		interactionPrompt.SetActive(value: false);
		SetActorColor();
	}

	private void OnDestroy()
	{
		this.OnInteractableDestroyed?.Invoke(this);
	}

	private void SetActorColor()
	{
		Actor actor = DialogueManager.Instance.masterDatabase.GetActor(dialogueActor.actor);
		if (actor == null)
		{
			return;
		}
		string text = Field.LookupValue(actor.fields, "NodeColor");
		if (text != null)
		{
			Color color = Tools.WebColor(text);
			nameParent.GetComponent<Image>().color = color;
			Color.RGBToHSV(color, out var _, out var _, out var V);
			if (V < brightnessThresholdForWhiteText)
			{
				nameText.color = Color.white;
			}
		}
	}

	private void SetActorName()
	{
		if (DialogueManager.Instance.masterDatabase.GetActor(dialogueActor.actor) != null)
		{
			string fullName = dialogueActor.GetFullName();
			nameText.text = DialogueManager.GetLocalizedText(fullName);
		}
	}

	public void SetCanInteract(bool value)
	{
		canInteract = value;
	}

	public void OnPlayerEnter(SpiderInteraction player)
	{
	}

	public void ShowInteractionPrompt(SpiderInteraction newSpiderInteraction, bool value)
	{
		if (value)
		{
			if (!isVisible && canInteract && Singleton<GameController>.Instance.State == GameController.GameState.Running && !Singleton<PlayTimeCanvas>.Instance.IsOpen)
			{
				isVisible = value;
				spiderInteraction = newSpiderInteraction;
				interactionPrompt.SetActive(isVisible);
				SetActorName();
			}
		}
		else
		{
			HideInteractionPrompt();
		}
	}

	public void HideInteractionPrompt()
	{
		isVisible = false;
		interactionPrompt.SetActive(isVisible);
	}

	public void OnInteract()
	{
		if (canInteract)
		{
			HideInteractionPrompt();
			onInteractEvent.Invoke();
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

	public void Reset()
	{
		spiderInteraction.RemoveInteractable(this);
		interactionPrompt.SetActive(value: false);
	}
}
