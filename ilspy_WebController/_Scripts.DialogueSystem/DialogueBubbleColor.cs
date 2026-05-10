using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.DialogueSystem;

public class DialogueBubbleColor : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	[Tooltip("Images whose colours are set to the colour of the current dialogue actor.")]
	private Image[] images;

	[SerializeField]
	[Tooltip("Name text whose colour will be changed to contrast with the background image colours, if needed.")]
	private TextMeshProUGUI nameText;

	[Header("Parameters")]
	[SerializeField]
	private float brightnessThresholdForWhiteText = 0.5f;

	private void OnEnable()
	{
		if (DialogueManager.Instance.ConversationController == null)
		{
			return;
		}
		int id = DialogueManager.Instance.conversationController.currentState.subtitle.speakerInfo.id;
		string text = Field.LookupValue(DialogueManager.Instance.masterDatabase.GetActor(id).fields, "NodeColor");
		if (text != null)
		{
			Color color = Tools.WebColor(text);
			Image[] array = images;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = color;
			}
			Color.RGBToHSV(color, out var _, out var _, out var V);
			if (V < brightnessThresholdForWhiteText)
			{
				nameText.color = Color.white;
			}
		}
	}
}
