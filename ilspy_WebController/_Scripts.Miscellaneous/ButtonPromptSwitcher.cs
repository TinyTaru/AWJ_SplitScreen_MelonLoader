using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Miscellaneous;

public class ButtonPromptSwitcher : MonoBehaviour
{
	[SerializeField]
	private GameObject iconKeyboardAndMouse;

	[SerializeField]
	private GameObject iconGamepad;

	private void Update()
	{
		iconKeyboardAndMouse.SetActive(Singleton<GameController>.Instance.InputIsKeyboardMouse);
		iconGamepad.SetActive(!Singleton<GameController>.Instance.InputIsKeyboardMouse);
	}
}
