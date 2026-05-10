using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UI.Mobile_UI;

public class MobileButtonController : MonoBehaviour
{
	[Header("Buttons")]
	[SerializeField]
	private RectTransform[] buttons;

	private void Start()
	{
		UpdateButtonPositions();
	}

	private void OnEnable()
	{
		MobileUICustomizer.OnButtonCustomizationSaved += MobileUICustomizer_OnCustomizationSaved;
	}

	private void OnDisable()
	{
		MobileUICustomizer.OnButtonCustomizationSaved -= MobileUICustomizer_OnCustomizationSaved;
	}

	private void UpdateButtonPositions()
	{
		RectTransform[] array = buttons;
		foreach (RectTransform rectTransform in array)
		{
			if ((bool)rectTransform)
			{
				if (Singleton<MobileCustomizationController>.Instance.ButtonExists(rectTransform.name))
				{
					rectTransform.anchoredPosition = Singleton<MobileCustomizationController>.Instance.GetButtonPosition(rectTransform.name);
					Vector3 localScale = rectTransform.localScale;
					localScale.x = Singleton<MobileCustomizationController>.Instance.GetButtonSize(rectTransform.name);
					rectTransform.localScale = new Vector3(localScale.x, localScale.x, 1f);
				}
				else
				{
					Singleton<MobileCustomizationController>.Instance.SaveButtonPosition(rectTransform.name, rectTransform.anchoredPosition);
					Singleton<MobileCustomizationController>.Instance.SaveButtonSize(rectTransform.name, rectTransform.localScale.x);
				}
			}
		}
	}

	private void MobileUICustomizer_OnCustomizationSaved()
	{
		Debug.Log("Button customization has been saved!");
		UpdateButtonPositions();
	}
}
