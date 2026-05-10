using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.Interactable;
using _Scripts.Singletons;
using _Scripts.UI.MobileMonetization;

namespace _Scripts.Spider;

public class SpiderInteraction : MonoBehaviour
{
	[SerializeField]
	private float interactionUpdateInterval;

	[SerializeField]
	private DialogueSystemTrigger dialogueSystemTrigger;

	[SerializeField]
	private float cooldownTime;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference interactInputAction;

	[SerializeField]
	private InputActionReference monologInputAction;

	private BodyMovement bodyMovement;

	private List<IInteractable> interactables;

	private float interactionUpdateTimer;

	private bool canInteract;

	public BodyMovement Movement => bodyMovement;

	public event EventHandler OnInteractableAdded;

	public event EventHandler OnInteractableRemoved;

	private void Awake()
	{
		bodyMovement = GetComponentInParent<BodyMovement>();
	}

	private void OnEnable()
	{
		if (bodyMovement.IsPlayer)
		{
			interactInputAction.action.performed += OnInteract;
			monologInputAction.action.performed += OnMonolog;
		}
	}

	private void OnDisable()
	{
		if (bodyMovement.IsPlayer)
		{
			interactInputAction.action.performed -= OnInteract;
			monologInputAction.action.performed -= OnMonolog;
		}
	}

	private void Start()
	{
		if (bodyMovement.IsPlayer)
		{
			interactables = new List<IInteractable>();
			interactionUpdateTimer = interactionUpdateInterval;
			canInteract = true;
			if (Singleton<GameController>.Instance != null)
			{
				Singleton<GameController>.Instance.OnResetPlayer += GameController_OnResetPlayer;
			}
		}
	}

	private void Update()
	{
		if (bodyMovement.IsPlayer && canInteract)
		{
			interactionUpdateTimer -= Time.deltaTime;
			if (interactionUpdateTimer <= 0f)
			{
				interactionUpdateTimer += interactionUpdateInterval;
				GetClosestInteractable();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!bodyMovement.IsPlayer)
		{
			return;
		}
		IInteractable component = other.GetComponent<IInteractable>();
		if (component != null)
		{
			component.OnPlayerEnter(this);
			if (interactables.Count == 0)
			{
				component.ShowInteractionPrompt(this, interactables.Count == 0);
			}
			if (!interactables.Contains(component))
			{
				interactables.Add(component);
				component.OnInteractableDestroyed += InteractableOnOnInteractableDestroyed;
				this.OnInteractableAdded?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (bodyMovement.IsPlayer)
		{
			IInteractable component = other.GetComponent<IInteractable>();
			if (component != null)
			{
				component.ShowInteractionPrompt(this, value: false);
				interactables.Remove(component);
				component.OnInteractableDestroyed -= InteractableOnOnInteractableDestroyed;
				this.OnInteractableRemoved?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	private IEnumerator InteractCooldownCoroutine()
	{
		canInteract = false;
		yield return new WaitForSeconds(cooldownTime);
		canInteract = true;
	}

	private IInteractable GetClosestInteractable()
	{
		IInteractable interactable = null;
		foreach (IInteractable interactable2 in interactables)
		{
			if (interactable2.CanInteract())
			{
				if (interactable == null)
				{
					interactable = interactable2;
				}
				else if (Vector3.Distance(interactable2.Position(), base.transform.position) < Vector3.Distance(interactable.Position(), base.transform.position))
				{
					interactable = interactable2;
				}
			}
		}
		foreach (IInteractable interactable3 in interactables)
		{
			interactable3.ShowInteractionPrompt(this, interactable3 == interactable);
		}
		Singleton<MobileControls>.Instance.ShowInteractButton(interactable?.ShowInteractButton() ?? false);
		return interactable;
	}

	public void RemoveInteractable(IInteractable interactable)
	{
		interactables.Remove(interactable);
		this.OnInteractableRemoved?.Invoke(this, EventArgs.Empty);
	}

	public void StartInteractionCooldown()
	{
		StartCoroutine(InteractCooldownCoroutine());
	}

	public void MobileInteract()
	{
		if (!bodyMovement.IsPlayer || Singleton<GameController>.Instance == null || Singleton<GameController>.Instance.State != 0 || bodyMovement.State != 0 || !canInteract || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			return;
		}
		IInteractable closestInteractable = GetClosestInteractable();
		if (closestInteractable != null && closestInteractable.CanInteract())
		{
			if (closestInteractable is InteractableDialogue)
			{
				bodyMovement.OnStartDialogue(closestInteractable.Position());
			}
			Singleton<GameController>.Instance.IsMonolog = false;
			closestInteractable.OnInteract();
		}
	}

	private void OnInteract(InputAction.CallbackContext ctx)
	{
		if (bodyMovement.IsPlayer && !(Singleton<GameController>.Instance == null) && Singleton<GameController>.Instance.State == GameController.GameState.Running && bodyMovement.State == BodyMovement.MovementState.Walking && canInteract && !Singleton<PlayTimeCanvas>.Instance.IsOpen && ctx.ReadValueAsButton())
		{
			IInteractable closestInteractable = GetClosestInteractable();
			if (closestInteractable != null && closestInteractable.CanInteract())
			{
				_ = closestInteractable is InteractableDialogue;
				Singleton<GameController>.Instance.IsMonolog = false;
				closestInteractable.OnInteract();
			}
		}
	}

	private void OnMonolog(InputAction.CallbackContext ctx)
	{
	}

	private void GameController_OnResetPlayer(object sender, EventArgs e)
	{
		Debug.Log($"On Reset Player {interactables.Count}");
		foreach (IInteractable item in interactables.ToList())
		{
			item.HideInteractionPrompt();
			interactables.Remove(item);
		}
	}

	private void InteractableOnOnInteractableDestroyed(IInteractable interactable)
	{
		interactable.OnInteractableDestroyed -= InteractableOnOnInteractableDestroyed;
		if (interactables.Contains(interactable))
		{
			interactables.Remove(interactable);
		}
	}
}
