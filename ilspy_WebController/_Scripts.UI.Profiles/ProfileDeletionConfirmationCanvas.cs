using TMPro;
using UnityEngine;
using _Scripts.Singletons;
using _Scripts.UI.Panels;

namespace _Scripts.UI.Profiles;

public class ProfileDeletionConfirmationCanvas : MonoBehaviour
{
	[SerializeField]
	private ProfileSelectionCanvas profileSelectionCanvas;

	[SerializeField]
	private RectTransform deletionMessageCanvas;

	[SerializeField]
	private TextMeshProUGUI confirmationText;

	private PanelManager panelManager;

	private int profileToDelete;

	private void Awake()
	{
		panelManager = GetComponentInParent<PanelManager>();
	}

	public void DeleteProfile()
	{
		Singleton<ProfileController>.Instance.DeleteProfile(profileToDelete);
		if (panelManager == null)
		{
			panelManager = GetComponentInParent<PanelManager>();
		}
		panelManager.OpenPanel(deletionMessageCanvas, removeCurrentPanelFromStack: true);
	}

	public void GoBack()
	{
		if (panelManager == null)
		{
			panelManager = GetComponentInParent<PanelManager>();
		}
		panelManager.GoBack();
	}

	public void Open(int profile)
	{
		profileToDelete = profile;
	}
}
