using System.Collections;
using UnityEngine;

namespace _Scripts.UI.Menus;

public class SplashScreenStartButtonMobile : MonoBehaviour
{
	[SerializeField]
	private GameObject uiElements;

	private IEnumerator Start()
	{
		uiElements.SetActive(value: false);
		yield return new WaitForSeconds(0.5f);
		uiElements.SetActive(value: true);
	}
}
