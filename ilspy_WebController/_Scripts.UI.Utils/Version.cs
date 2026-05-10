using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Scripts.Utils;

namespace _Scripts.UI.Utils;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Version : MonoBehaviour
{
	[SerializeField]
	private string eventVersionText = "A Webbing Journey - A1 Austrian eSports Festival";

	private TextMeshProUGUI versionText;

	private float timer;

	private void Start()
	{
		SceneManager.sceneLoaded += SceneManager_OnSceneLoaded;
		string text = "";
		text = GetVersionText();
		if (SceneManager.GetActiveScene().name.ToUpper().Contains("LEVEL"))
		{
			text = "";
		}
		versionText = GetComponent<TextMeshProUGUI>();
		versionText.text = text;
	}

	private void Update()
	{
	}

	private string GetVersionText()
	{
		return "Windows" + " v" + BuildInfo.FullVersionString;
	}

	private void SceneManager_OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		timer = 0f;
	}
}
