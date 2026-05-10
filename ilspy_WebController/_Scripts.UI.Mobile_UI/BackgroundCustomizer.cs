using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Mobile_UI;

public class BackgroundCustomizer : MonoBehaviour
{
	[SerializeField]
	private Image colorBackground;

	[SerializeField]
	private Image screenshotBackground;

	public void ToggleBackground()
	{
		if ((bool)colorBackground || (bool)screenshotBackground)
		{
			if (colorBackground.gameObject.activeSelf)
			{
				colorBackground.gameObject.SetActive(value: false);
				screenshotBackground.gameObject.SetActive(value: true);
			}
			else
			{
				colorBackground.gameObject.SetActive(value: true);
				screenshotBackground.gameObject.SetActive(value: false);
			}
		}
	}
}
