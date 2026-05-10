using TMPro;
using UnityEngine;

namespace _Scripts.LivingRoom;

public class PianoMinigameFeedback : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI feedbackText;

	public void OnAnimationFinished()
	{
		Object.Destroy(base.gameObject);
	}

	public void SetFeedbackText(string feedback)
	{
		feedbackText.text = feedback;
	}
}
