using TMPro;
using UnityEngine;

namespace _Scripts.Debugging;

public class QuitButton : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void RandomNumber()
	{
		GetComponentInChildren<TextMeshProUGUI>().text = Random.Range(0, 10).ToString();
	}
}
