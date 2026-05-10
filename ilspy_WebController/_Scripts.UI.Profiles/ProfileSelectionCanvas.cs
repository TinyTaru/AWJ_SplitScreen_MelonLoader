using TMPro;
using UnityEngine;
using _Scripts.UI.Panels;

namespace _Scripts.UI.Profiles;

public class ProfileSelectionCanvas : MonoBehaviour
{
	[Header("Deletion Confirmation")]
	[SerializeField]
	private ProfileDeletionConfirmationCanvas profileDeletionConfirmationCanvas;

	[SerializeField]
	private RectTransform deletionMessageCanvas;

	[SerializeField]
	private TextMeshProUGUI deletionMessage;

	private PanelManager panelManager;

	private void Awake()
	{
		panelManager = GetComponentInParent<PanelManager>();
	}

	public void OpenDeletionConfirmation(int profile)
	{
		profileDeletionConfirmationCanvas.Open(profile);
		panelManager.OpenPanel(profileDeletionConfirmationCanvas.GetComponent<RectTransform>());
	}

	public void ShowDeletionMessage(int profile)
	{
		panelManager.OpenPanel(deletionMessageCanvas);
	}
}
