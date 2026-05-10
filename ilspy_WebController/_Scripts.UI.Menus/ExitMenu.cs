using UnityEngine;
using _Scripts.UI.Panels;

namespace _Scripts.UI.Menus;

public class ExitMenu : MonoBehaviour
{
	private PanelManager panelManager;

	private void Awake()
	{
		panelManager = GetComponentInParent<PanelManager>();
		if (panelManager == null)
		{
			Debug.LogError("No PanelManager found in parent!");
		}
	}

	public void Exit()
	{
		if (!(panelManager == null))
		{
			panelManager.GoBack();
		}
	}
}
