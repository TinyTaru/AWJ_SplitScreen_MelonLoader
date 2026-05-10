using TMPro;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UI.Menus;

public class CoinUILevel : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private TextMeshProUGUI coinAmountText;

	private void OnEnable()
	{
		if (Singleton<SceneController>.Instance.GetCurrentLevelName().Contains("Tutorial") || Singleton<SceneController>.Instance.GetCurrentLevelName().Contains("Hub"))
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			coinAmountText.text = $"{Singleton<SceneController>.Instance.GetCurrentLevelDisplayName()}: {Singleton<CoinController>.Instance.GetCollectedCoinAmountInCurrentLevel()} / 100";
		}
	}
}
