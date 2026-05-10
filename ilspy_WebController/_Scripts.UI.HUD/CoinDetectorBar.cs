using MPUIKIT;
using UnityEngine;
using _Scripts.CoinDetector;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UI.HUD;

public class CoinDetectorBar : MonoBehaviour
{
	[SerializeField]
	private GameObject visuals;

	[SerializeField]
	private MPImageBasic barFillImage;

	private void Start()
	{
		Hide();
		_Scripts.CoinDetector.CoinDetector.OnMinDistanceUpdated -= CoinDetector_OnMinDistanceUpdated;
		_Scripts.CoinDetector.CoinDetector.OnStateChanged -= CoinDetector_OnStateChanged;
		_Scripts.CoinDetector.CoinDetector.OnMinDistanceUpdated += CoinDetector_OnMinDistanceUpdated;
		_Scripts.CoinDetector.CoinDetector.OnStateChanged += CoinDetector_OnStateChanged;
	}

	private void OnDestroy()
	{
		_Scripts.CoinDetector.CoinDetector.OnMinDistanceUpdated -= CoinDetector_OnMinDistanceUpdated;
		_Scripts.CoinDetector.CoinDetector.OnStateChanged -= CoinDetector_OnStateChanged;
	}

	private void Show()
	{
		visuals.SetActive(value: true);
	}

	private void Hide()
	{
		visuals.SetActive(value: false);
	}

	private void SetBarFillAmount(float amount)
	{
		barFillImage.fillAmount = amount;
	}

	private void CoinDetector_OnMinDistanceUpdated(float relativeDistance)
	{
		SetBarFillAmount(1f - relativeDistance);
	}

	private void CoinDetector_OnStateChanged(_Scripts.CoinDetector.CoinDetector.CoinDetectorState state)
	{
		if (state == _Scripts.CoinDetector.CoinDetector.CoinDetectorState.Active && (SettingsController.CoinDetectorMode == CoinDetectorModeEnum.Both || SettingsController.CoinDetectorMode == CoinDetectorModeEnum.Visual))
		{
			Show();
		}
		else
		{
			Hide();
		}
	}
}
