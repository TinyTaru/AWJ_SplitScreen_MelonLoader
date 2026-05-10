using DG.Tweening;
using TMPro;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UI.Shop;

public class CoinUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Transform coinDisplayTransform;

	[SerializeField]
	private TextMeshProUGUI coinAmountText;

	[Header("Animation Parameters")]
	[SerializeField]
	private bool playAddCoinsAnimation;

	[SerializeField]
	private float punchScaleSize = 1.2f;

	[SerializeField]
	private float punchScaleDuration = 0.5f;

	[SerializeField]
	private int punchScaleVibrato = 5;

	[SerializeField]
	private float punchScaleElasticity = 1f;

	[Space(5f)]
	[SerializeField]
	private float displayDuration = 0.5f;

	[Space(5f)]
	[SerializeField]
	private float fadeDuration = 0.5f;

	private CanvasGroup coinDisplayCanvasGroup;

	private Sequence sequence;

	private void Awake()
	{
		coinDisplayCanvasGroup = coinDisplayTransform.GetComponent<CanvasGroup>();
		coinDisplayTransform.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		if (Singleton<CoinController>.Instance != null)
		{
			Singleton<CoinController>.Instance.OnCoinAmountChanged += CoinController_OnCoinAmountChanged;
			coinAmountText.text = Singleton<CoinController>.Instance.CurrentCoinAmount.ToString();
		}
	}

	private void OnDestroy()
	{
		if (Singleton<CoinController>.Instance != null)
		{
			Singleton<CoinController>.Instance.OnCoinAmountChanged -= CoinController_OnCoinAmountChanged;
		}
	}

	public void Show()
	{
		coinDisplayTransform.gameObject.SetActive(value: true);
		coinDisplayTransform.localScale = Vector3.zero;
		coinDisplayCanvasGroup.alpha = 1f;
		sequence.Kill();
		sequence = DOTween.Sequence();
		sequence.SetUpdate(isIndependentUpdate: true);
		sequence.Append(coinDisplayTransform.DOScale(Vector3.one, 0.2f));
		sequence.Play();
	}

	public void Hide(bool hideImmediate = false)
	{
		if (hideImmediate)
		{
			coinDisplayTransform.localScale = Vector3.zero;
			return;
		}
		coinDisplayTransform.gameObject.SetActive(value: true);
		coinDisplayTransform.localScale = Vector3.one;
		coinDisplayCanvasGroup.alpha = 1f;
		sequence.Kill();
		sequence = DOTween.Sequence();
		sequence.SetUpdate(isIndependentUpdate: true);
		sequence.Append(coinDisplayTransform.DOScale(Vector3.zero, 0.2f));
		sequence.Play();
	}

	private void CoinController_OnCoinAmountChanged(CoinController.OnCoinAmountChangedEventArgs e)
	{
		coinAmountText.text = e.coinAmount.ToString();
		if (e.coinsAdded && playAddCoinsAnimation)
		{
			coinDisplayTransform.gameObject.SetActive(value: true);
			coinDisplayCanvasGroup.alpha = 1f;
			coinDisplayTransform.localScale = Vector3.one;
			sequence.Kill();
			sequence = DOTween.Sequence();
			sequence.SetUpdate(isIndependentUpdate: true);
			sequence.Append(coinDisplayTransform.DOPunchScale(Vector3.one * punchScaleSize, punchScaleDuration, punchScaleVibrato, punchScaleElasticity));
			sequence.AppendInterval(displayDuration);
			sequence.Append(coinDisplayCanvasGroup.DOFade(0f, fadeDuration));
			sequence.OnComplete(delegate
			{
				coinDisplayTransform.gameObject.SetActive(value: false);
			});
			sequence.Play();
		}
	}
}
