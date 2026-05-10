using UnityEngine;
using UnityEngine.UI;
using _Scripts.Singletons;

namespace _Scripts.Mobile;

public class ContinueButtonMobile : MonoBehaviour
{
	private Button button;

	private void Awake()
	{
		button = GetComponent<Button>();
	}

	private void OnEnable()
	{
		if (!(Singleton<GameController>.Instance == null))
		{
			Singleton<GameController>.Instance.SetCurrentContinueButton(button);
		}
	}
}
