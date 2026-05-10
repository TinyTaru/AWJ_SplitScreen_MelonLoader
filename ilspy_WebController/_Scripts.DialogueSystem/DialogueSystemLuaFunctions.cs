using System;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Scripts.Emotes;
using _Scripts.Game;
using _Scripts.Interactable;
using _Scripts.KidsRoom;
using _Scripts.NPCs;
using _Scripts.Office;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.Utils;

namespace _Scripts.DialogueSystem;

public class DialogueSystemLuaFunctions : MonoBehaviour
{
	private List<DialogueActor> dialogueActorsInScene;

	private List<Actor> allActors;

	private void OnEnable()
	{
		Lua.RegisterFunction("PerformEmotePlayer", this, SymbolExtensions.GetMethodInfo(() => PerformEmotePlayer(string.Empty, playSound: false)));
		Lua.RegisterFunction("PerformEmoteNPC", this, SymbolExtensions.GetMethodInfo(() => PerformEmoteNPC(string.Empty, playSound: false)));
		Lua.RegisterFunction("PerformEmoteAll", this, SymbolExtensions.GetMethodInfo(() => PerformEmoteAll(string.Empty, playSound: false)));
		Lua.RegisterFunction("PlaySound", this, SymbolExtensions.GetMethodInfo(() => PlaySound(string.Empty)));
		Lua.RegisterFunction("PlaySpeakSound", this, SymbolExtensions.GetMethodInfo(() => PlaySpeakSound(string.Empty)));
		Lua.RegisterFunction("SetCanInteract", this, SymbolExtensions.GetMethodInfo(() => SetCanInteract(value: false)));
		Lua.RegisterFunction("StartFollowing", this, SymbolExtensions.GetMethodInfo(() => StartFollowing(value: false)));
		Lua.RegisterFunction("FormatTime", this, SymbolExtensions.GetMethodInfo(() => FormatTime(0f)));
		Lua.RegisterFunction("SetGameState", this, SymbolExtensions.GetMethodInfo(() => SetGameState(string.Empty)));
		Lua.RegisterFunction("LookAllAtActor", this, SymbolExtensions.GetMethodInfo(() => LookAllAtActor()));
		Lua.RegisterFunction("ActivateRace", this, SymbolExtensions.GetMethodInfo(() => ActivateRace()));
		Lua.RegisterFunction("ActivateMinigame", this, SymbolExtensions.GetMethodInfo(() => ActivateMinigame(string.Empty)));
		SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
	}

	private void OnDisable()
	{
		Lua.UnregisterFunction("PerformEmotePlayer");
		Lua.UnregisterFunction("PerformEmoteNPC");
		Lua.UnregisterFunction("PerformEmoteAll");
		Lua.UnregisterFunction("PlaySound");
		Lua.UnregisterFunction("PlaySpeakSound");
		Lua.UnregisterFunction("SetCanInteract");
		Lua.UnregisterFunction("StartFollowing");
		Lua.UnregisterFunction("FormatTime");
		Lua.UnregisterFunction("SetGameState");
		Lua.UnregisterFunction("LookAtConversant");
		Lua.UnregisterFunction("ActivateRace");
		Lua.UnregisterFunction("ActivateMinigame");
		SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
	}

	private void Awake()
	{
		allActors = DialogueManager.Instance.DatabaseManager.DefaultDatabase.actors;
	}

	private DialogueActor GetCorrespondingObject(int id)
	{
		string actorName = allActors.First((Actor ac) => ac.id == id).Name;
		return dialogueActorsInScene.First((DialogueActor da) => da.actor == actorName);
	}

	private HashSet<int> GetPassiveListeners(Conversation conversation, int speakerId, int conversantId)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (DialogueEntry dialogueEntry in conversation.dialogueEntries)
		{
			hashSet.Add(dialogueEntry.ConversantID);
			hashSet.Add(dialogueEntry.ActorID);
		}
		hashSet.Remove(speakerId);
		hashSet.Remove(conversantId);
		return hashSet;
	}

	public void LookAllAtActor()
	{
		int conversationID = DialogueManager.Instance.CurrentConversationState.subtitle.dialogueEntry.conversationID;
		Conversation conversation = DialogueManager.Instance.DatabaseManager.DefaultDatabase.GetConversation(conversationID);
		DialogueEntry dialogueEntry = DialogueManager.Instance.currentConversationState.subtitle.dialogueEntry;
		int num = conversation.dialogueEntries.IndexOf(dialogueEntry);
		if (num + 1 >= conversation.dialogueEntries.Count - 1)
		{
			return;
		}
		DialogueEntry upcomingConvo = conversation.dialogueEntries[num + 1];
		if (upcomingConvo == null)
		{
			return;
		}
		DialogueActor upcomingSpeaker = GetCorrespondingObject(upcomingConvo.ActorID);
		DialogueActor correspondingObject = GetCorrespondingObject(upcomingConvo.ConversantID);
		upcomingSpeaker.GetComponentInParent<BodyMovement>().OnStartDialogue(correspondingObject.transform.position);
		correspondingObject.GetComponentInParent<BodyMovement>().OnStartDialogue(upcomingSpeaker.transform.position);
		HashSet<int> listeners = GetPassiveListeners(conversation, upcomingConvo.ActorID, upcomingConvo.ConversantID);
		allActors.Where((Actor ac) => listeners.Contains(ac.id) && (ac.id != upcomingConvo.ActorID || ac.id != upcomingConvo.ConversantID)).ToList().ForEach(delegate(Actor ac)
		{
			dialogueActorsInScene.First((DialogueActor da) => da.actor == ac.Name).GetComponentInParent<BodyMovement>().OnStartDialogue(upcomingSpeaker.transform.position);
		});
	}

	private void PerformEmotePlayer(string emoteName, bool playSound = true)
	{
		SpiderEmotes component = Singleton<GameController>.Instance.Player.GetComponent<SpiderEmotes>();
		if (component != null)
		{
			component.PerformEmote(emoteName, playSound, stopCurrentEmote: true);
		}
	}

	private void PerformEmoteNPC(string emoteName, bool playSound = true)
	{
		SpiderEmotes componentInParent = DialogueManager.Instance.currentConversant.GetComponentInParent<SpiderEmotes>();
		if (componentInParent != null)
		{
			componentInParent.PerformEmote(emoteName, playSound, stopCurrentEmote: true);
		}
	}

	private void PerformEmoteAll(string emoteName, bool playSound = true)
	{
		Singleton<EmoteController>.Instance.PerformEmoteAll(emoteName, playSound, stopCurrentEmote: true);
	}

	private void PlaySound(string soundName)
	{
		if (!(Singleton<MusicController>.Instance == null))
		{
			Singleton<MusicController>.Instance.PlaySound(soundName);
		}
	}

	private void PlaySpeakSound(string soundName)
	{
		if (!(Singleton<MusicController>.Instance == null))
		{
			Singleton<MusicController>.Instance.PlaySpeakSound(soundName);
		}
	}

	private void SetCanInteract(bool value)
	{
		DialogueManager.Instance.currentConversant.GetComponentInChildren<InteractableDialogue>().SetCanInteract(value);
	}

	private void StartFollowing(bool value)
	{
		BodyMovement component = DialogueManager.Instance.currentConversant.GetComponent<BodyMovement>();
		if (component != null)
		{
			if (value)
			{
				component.SetPlayerInteraction(BodyMovement.PlayerInteraction.Follow);
				component.GetComponentInChildren<NPC>().StartFollowing();
				component.GetComponentInChildren<FollowTrigger>().OnFollow.Invoke();
			}
			else
			{
				component.SetPlayerInteraction(BodyMovement.PlayerInteraction.LookAt);
				component.GetComponentInChildren<NPC>().StopFollowing();
			}
		}
	}

	private string FormatTime(float timeInSeconds)
	{
		return _Scripts.Utils.Utils.FormatTime(timeInSeconds);
	}

	private void SetGameState(string stateName)
	{
		if (Enum.TryParse<GameController.GameState>(stateName, out var result))
		{
			Singleton<GameController>.Instance.State = result;
		}
	}

	private void ActivateRace()
	{
		KitchenRaceController kitchenRaceController = UnityEngine.Object.FindAnyObjectByType<KitchenRaceController>();
		if (kitchenRaceController != null)
		{
			kitchenRaceController.ActivateRace();
		}
	}

	private void ActivateMinigame(string minigame)
	{
		if (!(minigame == "NerfGun"))
		{
			if (minigame == "ShmoopTrial")
			{
				ShmoopTrialMinigame shmoopTrialMinigame = UnityEngine.Object.FindAnyObjectByType<ShmoopTrialMinigame>();
				if (shmoopTrialMinigame != null)
				{
					shmoopTrialMinigame.StartMinigame();
				}
			}
		}
		else
		{
			NerfGunMinigame nerfGunMinigame = UnityEngine.Object.FindAnyObjectByType<NerfGunMinigame>();
			if (nerfGunMinigame != null)
			{
				nerfGunMinigame.StartMinigame();
			}
		}
	}

	private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		dialogueActorsInScene = (from actor in UnityEngine.Object.FindObjectsByType<DialogueActor>(FindObjectsInactive.Include, FindObjectsSortMode.None)
			select (actor)).ToList();
	}
}
