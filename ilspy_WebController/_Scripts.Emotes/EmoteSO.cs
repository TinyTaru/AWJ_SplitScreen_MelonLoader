using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Emotes;

[CreateAssetMenu(fileName = "New Emote", menuName = "FTG/New Emote")]
public class EmoteSO : WheelOptionSo
{
	public string displayName;

	public string emoteName;

	public override void ExecuteSelection()
	{
		if (emoteName == "")
		{
			Singleton<EmoteController>.Instance.InvalidEmote();
			Singleton<GameController>.Instance.State = GameController.GameState.Running;
		}
		else
		{
			Singleton<EmoteController>.Instance.PerformEmoteFromWheel(emoteName);
		}
	}
}
