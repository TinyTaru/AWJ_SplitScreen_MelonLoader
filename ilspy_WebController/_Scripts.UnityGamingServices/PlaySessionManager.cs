using System;
using System.Collections;
using Unity.Services.CloudCode.GeneratedBindings.AWebbingJourneyCloud;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public class PlaySessionManager : Singleton<PlaySessionManager>
{
	private PlaySessionData localPlaySessionData;

	private DateTime playSessionExpiresAt;

	private Coroutine expirationCoroutine;

	private bool hasExpired;

	private bool warningSent;

	private bool adAvailable;

	private const int secondsLeftForWarning = 60;

	private int minutesForAdAvailable = 50;

	private TimeSpan nextAdAvailableTime;

	public bool AdAvailable => adAvailable;

	public event Action<PlaySessionData> PlaySessionDataUpdated;

	public event Action OnPlaySessionTimeAdded;

	public event Action OnAdAvailable;

	public event Action<TimeSpan> OnPlaySessionTimerUpdated;

	public event Action OnPlaySessionWarningSent;

	public event Action OnPlaySessionExpired;

	private IEnumerator ExpirationRoutine()
	{
		while (true)
		{
			TimeSpan timeSpan = playSessionExpiresAt - DateTime.UtcNow;
			nextAdAvailableTime = timeSpan.Subtract(new TimeSpan(0, minutesForAdAvailable, 0));
			this.OnPlaySessionTimerUpdated?.Invoke(timeSpan);
			if (timeSpan < TimeSpan.Zero)
			{
				break;
			}
			if (timeSpan.TotalSeconds < 60.0 && !warningSent)
			{
				warningSent = true;
				this.OnPlaySessionWarningSent?.Invoke();
			}
			if (!adAvailable && timeSpan.TotalMinutes <= (double)minutesForAdAvailable)
			{
				adAvailable = true;
				this.OnAdAvailable?.Invoke();
			}
			yield return new WaitForSecondsRealtime(1f);
		}
		if (!hasExpired)
		{
			hasExpired = true;
			this.OnPlaySessionExpired?.Invoke();
		}
	}

	public void HandlePlaySessionDataUpdate(PlaySessionData playSessionData)
	{
		localPlaySessionData = playSessionData;
		this.PlaySessionDataUpdated?.Invoke(localPlaySessionData);
		SetExpirationTime(localPlaySessionData.PlaySessionExpiresAt);
	}

	public void SetExpirationTime(DateTime newPlaySessionsExpiresAt)
	{
		playSessionExpiresAt = newPlaySessionsExpiresAt;
		TimeSpan timeSpan = playSessionExpiresAt - DateTime.UtcNow;
		this.OnPlaySessionTimerUpdated?.Invoke(timeSpan);
		adAvailable = timeSpan.TotalMinutes <= (double)minutesForAdAvailable;
		this.OnAdAvailable?.Invoke();
		if (timeSpan < TimeSpan.Zero)
		{
			hasExpired = true;
			warningSent = true;
			return;
		}
		hasExpired = false;
		warningSent = false;
		if (expirationCoroutine != null)
		{
			StopCoroutine(expirationCoroutine);
		}
		expirationCoroutine = StartCoroutine(ExpirationRoutine());
		this.OnPlaySessionTimeAdded?.Invoke();
	}

	public TimeSpan GetRemainingTime()
	{
		return playSessionExpiresAt - DateTime.UtcNow;
	}

	public TimeSpan GetNextAdAvailableTime()
	{
		return nextAdAvailableTime;
	}

	public void SetMinutesForAdAvailable(int rewardedAdPlaytimeMinutes)
	{
		minutesForAdAvailable = 60 - rewardedAdPlaytimeMinutes;
	}
}
