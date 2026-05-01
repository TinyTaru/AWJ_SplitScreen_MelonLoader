using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Camera;

public class DialogueFocus : MonoBehaviour
{
	private Transform player;

	private Transform dialogueConversant;

	private void Start()
	{
		player = Singleton<GameController>.Instance.Player.transform;
	}

	private void Update()
	{
		if (!(dialogueConversant == null))
		{
			base.transform.position = (player.position + dialogueConversant.position) / 2f;
			base.transform.rotation = Quaternion.Slerp(player.rotation, dialogueConversant.rotation, 0.5f);
		}
	}

	public void UpdateDialogueConversant()
	{
		dialogueConversant = (Singleton<GameController>.Instance.IsMonolog ? Singleton<GameController>.Instance.Player.transform : DialogueManager.Instance.currentConversant);
	}
}
