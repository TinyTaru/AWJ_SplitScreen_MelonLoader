using UnityEngine;

namespace _Scripts.UI.Tabs;

public class TabAffordance : MonoBehaviour
{
	private enum Tab
	{
		Left,
		Right
	}

	[SerializeField]
	private TabGroup tabs;

	[SerializeField]
	private CanvasGroup opacity;

	[SerializeField]
	private Tab activeTab;

	private void Start()
	{
	}

	private void Update()
	{
		if (!tabs)
		{
			return;
		}
		switch (activeTab)
		{
		case Tab.Left:
			if (tabs.GetActiveTabIndex() == 0)
			{
				opacity.alpha = 0f;
			}
			else
			{
				opacity.alpha = 1f;
			}
			break;
		case Tab.Right:
			if (tabs.GetActiveTabIndex() == tabs.ListLength() - 1)
			{
				opacity.alpha = 0f;
			}
			else
			{
				opacity.alpha = 1f;
			}
			break;
		default:
			Debug.Log("not finished");
			break;
		}
	}
}
