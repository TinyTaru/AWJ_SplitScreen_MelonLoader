using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UI.Wardrobe;

public class CustomizationIndicator : MonoBehaviour
{
	[SerializeField]
	private GameObject scrollView;

	[SerializeField]
	private GameObject infoMessage;

	private bool abdomenOn;

	private void OnEnable()
	{
		abdomenOn = Singleton<CosmeticItemsController>.Instance.LoadAbdomenEnabled();
		if (abdomenOn)
		{
			scrollView.SetActive(value: true);
			infoMessage.SetActive(value: false);
		}
		else
		{
			scrollView.SetActive(value: false);
			infoMessage.SetActive(value: true);
		}
	}

	private void Update()
	{
	}
}
