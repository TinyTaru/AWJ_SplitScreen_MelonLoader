using System.Collections;
using UnityEngine;
using _Scripts.Singletons;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.UnityGamingServices;

public class OnlineBootstrap : MonoBehaviour
{
	private enum Environment
	{
		Production,
		Development,
		Staging
	}

	private static OnlineBootstrap instance;

	private Coroutine routine;

	[SerializeField]
	private Environment environment;

	[Header("Delays in seconds")]
	[SerializeField]
	private float startDelay = 0.5f;

	[SerializeField]
	private float retryInterval = 20f;

	[SerializeField]
	private float ensureOnlineTimeOutEditor = 8f;

	[SerializeField]
	private float ensureOnlineTimeoutMobile = 60f;

	[Header("Main Menu")]
	[SerializeField]
	private float loadMainMenuDelay = 2f;

	[SerializeField]
	private LevelData mainMenuLevelData;

	private string environmentString = "development";

	private bool privacyPolicyAndTermsAccepted;

	private void Awake()
	{
	}

	private void OnDisable()
	{
	}

	private IEnumerator ReoccurringOnlineCheckCoroutine()
	{
		return null;
	}

	private IEnumerator LoadMainMenuCoroutine()
	{
		yield return new WaitForSeconds(loadMainMenuDelay);
		Singleton<SceneController>.Instance.LoadScene(mainMenuLevelData.sceneName);
	}

	private void OnApplicationFocus(bool hasFocus)
	{
	}

	public void SetPrivacyPolicyAndTermsAccepted(bool value)
	{
		privacyPolicyAndTermsAccepted = value;
		if (privacyPolicyAndTermsAccepted)
		{
			routine = StartCoroutine(ReoccurringOnlineCheckCoroutine());
			StartCoroutine(LoadMainMenuCoroutine());
		}
	}
}
