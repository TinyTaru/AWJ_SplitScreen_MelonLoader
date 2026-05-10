using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UI.Menus;

public class ArachnophobiaHolder : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private GameObject normalImage;

	[SerializeField]
	private GameObject arachnophobiaImage;

	private void OnEnable()
	{
		bool arachnophobiaMode = SettingsController.ArachnophobiaMode;
		if (normalImage != null)
		{
			normalImage.SetActive(!arachnophobiaMode);
		}
		if (arachnophobiaImage != null)
		{
			arachnophobiaImage.SetActive(arachnophobiaMode);
		}
	}
}
