using System;
using TMPro;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public class PlaySessionTimerUI : MonoBehaviour
{
	[Header("UI")]
	[SerializeField]
	private TextMeshProUGUI resourcesText;

	private void OnEnable()
	{
		if (Singleton<PlaySessionManager>.Instance != null)
		{
			Singleton<PlaySessionManager>.Instance.OnPlaySessionTimeAdded += PLaySessionTimer_OnPlaySessionTimeAdded;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionTimerUpdated += PlaySessionTimer_OnPlaySessionTimerUpdated;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionWarningSent += PlaySessionTimer_OnPlaySessionWarningSent;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionExpired += PlaySessionTimer_OnPlaySessionExpired;
			TimeSpan remainingTime = Singleton<PlaySessionManager>.Instance.GetRemainingTime();
			if (remainingTime.TotalSeconds <= 0.0)
			{
				resourcesText.text = "Play Session Expired!";
				resourcesText.color = Color.red;
			}
			else
			{
				resourcesText.text = $"Play Session Timer: {remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
				resourcesText.color = Color.green;
			}
		}
	}

	private void OnDisable()
	{
		if (Singleton<PlaySessionManager>.Instance != null)
		{
			Singleton<PlaySessionManager>.Instance.OnPlaySessionTimeAdded -= PLaySessionTimer_OnPlaySessionTimeAdded;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionTimerUpdated -= PlaySessionTimer_OnPlaySessionTimerUpdated;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionWarningSent -= PlaySessionTimer_OnPlaySessionWarningSent;
			Singleton<PlaySessionManager>.Instance.OnPlaySessionExpired -= PlaySessionTimer_OnPlaySessionExpired;
		}
	}

	private void PLaySessionTimer_OnPlaySessionTimeAdded()
	{
		resourcesText.color = Color.green;
	}

	private void PlaySessionTimer_OnPlaySessionTimerUpdated(TimeSpan timeSpan)
	{
		if (timeSpan < TimeSpan.Zero)
		{
			resourcesText.text = "Play Session Timer: 00:00";
			resourcesText.color = Color.gray;
		}
		else
		{
			resourcesText.text = $"Play Session Timer: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
		}
	}

	private void PlaySessionTimer_OnPlaySessionWarningSent()
	{
		resourcesText.color = Color.yellow;
	}

	private void PlaySessionTimer_OnPlaySessionExpired()
	{
		resourcesText.text = "Play Session Expired!";
		resourcesText.color = Color.red;
	}
}
