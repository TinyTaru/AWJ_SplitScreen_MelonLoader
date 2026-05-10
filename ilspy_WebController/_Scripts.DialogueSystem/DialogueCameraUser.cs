using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.DialogueSystem;

public class DialogueCameraUser : MonoBehaviour
{
	private void OnEnable()
	{
		if (!(Singleton<CameraController>.Instance == null))
		{
			CameraController.AddDialogueCameraUser();
		}
	}

	private void OnDisable()
	{
		if (!(Singleton<CameraController>.Instance == null))
		{
			CameraController.RemoveDialogueCameraUser();
		}
	}
}
