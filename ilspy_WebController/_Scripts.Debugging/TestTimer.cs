using TMPro;
using UnityEngine;

namespace _Scripts.Debugging;

public class TestTimer : MonoBehaviour
{
	private TextMeshProUGUI timerText;

	private float timer;

	private void Start()
	{
		timerText = GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		timer += Time.unscaledDeltaTime;
		timerText.text = $"{timer:0.0}";
	}
}
