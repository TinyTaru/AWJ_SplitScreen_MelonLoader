using System;
using UnityEngine;
using _Scripts.Spider;

namespace _Scripts.Interactable;

public interface IInteractable
{
	event Action<IInteractable> OnInteractableDestroyed;

	void OnPlayerEnter(SpiderInteraction spiderInteraction);

	void ShowInteractionPrompt(SpiderInteraction spiderInteraction, bool value);

	void HideInteractionPrompt();

	void OnInteract();

	Vector3 Position();

	bool CanInteract();

	bool ShowInteractButton();

	GameObject GetGameObject();
}
