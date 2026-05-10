using PixelCrushers.DialogueSystem;

namespace _Scripts.DialogueSystem;

public class SubtitlePanelNpc : TextAnimatorSubtitlePanel
{
	public override void SetContent(Subtitle subtitle)
	{
		base.SetContent(subtitle);
		string localizedText = DialogueManager.GetLocalizedText(DialogueLua.GetActorField(subtitle.speakerInfo.nameInDatabase, "Name").asString);
		portraitName.text = localizedText;
		UITools.SendTextChangeMessage(portraitName);
	}
}
