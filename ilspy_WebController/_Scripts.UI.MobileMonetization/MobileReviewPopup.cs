using System.Collections;
using DG.Tweening;
using UnityEngine;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.UnityGamingServices;

namespace _Scripts.UI.MobileMonetization;

public class MobileReviewPopup : Singleton<MobileReviewPopup>
{
	[SerializeField]
	private float popupDelay = 5f;

	[SerializeField]
	private GameObject backgroundImage;

	[SerializeField]
	private GameObject panel;

	private const int maxNumberOfReviewRequests = 5;

	private void Start()
	{
		Close();
	}

	private IEnumerator OpenReviewPopupCoroutine()
	{
		yield return new WaitForSeconds(Singleton<MobileReviewPopup>.Instance.popupDelay);
		Time.timeScale = 0f;
		Singleton<MobileReviewPopup>.Instance.backgroundImage.SetActive(value: true);
		Singleton<MobileReviewPopup>.Instance.panel.SetActive(value: true);
		Singleton<MobileReviewPopup>.Instance.panel.transform.DOScale(1f, 0.2f).From(0f).SetUpdate(isIndependentUpdate: true);
	}

	private void Close()
	{
		Singleton<MobileReviewPopup>.Instance.backgroundImage.SetActive(value: false);
		Singleton<MobileReviewPopup>.Instance.panel.SetActive(value: false);
	}

	public void OpenReviewPopup()
	{
	}

	public void Yes()
	{
		SaveController.Save("NumberOfReviewsRequests", 69, SaveData.General);
		Singleton<InAppReviewManager>.Instance.RequestReview();
		Singleton<MobileReviewPopup>.Instance.Close();
		Time.timeScale = 1f;
	}

	public void No()
	{
		Singleton<MobileReviewPopup>.Instance.Close();
		Time.timeScale = 1f;
	}
}
