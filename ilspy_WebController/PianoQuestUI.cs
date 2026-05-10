using TMPro;
using UnityEngine;

public class PianoQuestUI : MonoBehaviour
{
	[Header("UI Components")]
	[SerializeField]
	private TextMeshProUGUI currentPointsText;

	[SerializeField]
	private TextMeshProUGUI multiplierText;

	[SerializeField]
	private TextMeshProUGUI gameInfoText;

	[SerializeField]
	private TextMeshProUGUI timerText;

	[SerializeField]
	private GameObject settingsUI;

	private float timeAtStart;

	private void Start()
	{
		ToggleGameplayHUD(isActive: false);
		gameInfoText.gameObject.SetActive(value: false);
		settingsUI.SetActive(value: false);
	}

	private void FixedUpdate()
	{
		if (timerText.gameObject.activeSelf)
		{
			float num = Time.timeSinceLevelLoad - timeAtStart;
			timerText.text = "Time: " + num.ToString("F2");
		}
	}

	private void ToggleGameplayHUD(bool isActive)
	{
		currentPointsText.gameObject.SetActive(isActive);
		multiplierText.gameObject.SetActive(isActive);
	}

	private void ResetTimer()
	{
		timeAtStart = Time.timeSinceLevelLoad;
	}

	public void PrepareForStartup()
	{
		ToggleGameplayHUD(isActive: false);
		gameInfoText.gameObject.SetActive(value: true);
	}

	public void StartMinigame()
	{
		gameInfoText.gameObject.SetActive(value: false);
		ToggleGameplayHUD(isActive: true);
		ResetTimer();
	}

	public void EndMinigame(int playerPoints, int maxPoints)
	{
		gameInfoText.text = ((playerPoints == maxPoints) ? $"You got the maximum {maxPoints} points!" : $"You got {playerPoints} out of {maxPoints} possible points.");
		gameInfoText.gameObject.SetActive(value: true);
		ToggleGameplayHUD(isActive: false);
	}

	public void SetGameInfoStartingText(int countdownSeconds)
	{
		gameInfoText.gameObject.SetActive(value: true);
		gameInfoText.text = "Starting in..." + countdownSeconds;
	}

	public void UpdatePoints(int newPoints)
	{
		currentPointsText.text = "Your Score: " + newPoints;
	}

	public void UpdateMultiplier(int newMultiplier)
	{
		multiplierText.text = "Multiplier: " + newMultiplier;
	}

	public void ShowSettingsUI()
	{
		settingsUI.SetActive(value: true);
	}

	public void HideSettingsUI()
	{
		settingsUI.SetActive(value: false);
	}

	public void HideGameplayTexts()
	{
		ToggleGameplayHUD(isActive: false);
	}

	public void HideInfoText()
	{
		gameInfoText.gameObject.SetActive(value: false);
	}
}
