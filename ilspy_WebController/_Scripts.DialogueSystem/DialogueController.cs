using System;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.DialogueSystem;

public class DialogueController : Singleton<DialogueController>
{
	private void Start()
	{
		SceneController.OnSceneLoaded += SceneController_OnSceneLoaded;
	}

	private void OnDestroy()
	{
		SceneController.OnSceneLoaded -= SceneController_OnSceneLoaded;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel && !hasFocus)
		{
			SaveDialogueData();
		}
	}

	public void SaveDialogueData()
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel)
		{
			SaveController.SaveDialogueSystemData();
		}
	}

	private void SceneController_OnSceneLoaded(object sender, EventArgs e)
	{
		DialogueLua.SetVariable("Mobile", false);
		Debug.Log("Mobile = false");
	}
}
