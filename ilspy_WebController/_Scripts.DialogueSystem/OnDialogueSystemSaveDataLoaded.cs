using System;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;

namespace _Scripts.DialogueSystem;

public class OnDialogueSystemSaveDataLoaded : MonoBehaviour
{
	[SerializeField]
	private UnityEvent onDialogueSystemSaveDataLoaded;

	private void Start()
	{
		SaveController.OnDialogueSystemSaveDataLoaded += SaveController_OnDialogueSystemSaveDataLoaded;
	}

	private void SaveController_OnDialogueSystemSaveDataLoaded(object sender, EventArgs e)
	{
		onDialogueSystemSaveDataLoaded?.Invoke();
	}
}
