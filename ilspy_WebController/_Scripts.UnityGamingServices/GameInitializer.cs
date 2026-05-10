using System;
using System.Collections;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UnityConsent;
using _Scripts.Singletons;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.UnityGamingServices;

public class GameInitializer : Singleton<GameInitializer>
{
	[SerializeField]
	private LevelData sceneToLoad;

	[SerializeField]
	private float initializationRetryDuration = 10f;

	private const string internetTestUrl = "https://clients3.google.com/generate_204";

	protected override void Awake()
	{
		base.Awake();
		InitializeUnityServices();
	}

	private void Start()
	{
		StartCoroutine(InitializationCoroutine());
	}

	private IEnumerator InitializationCoroutine()
	{
		while (true)
		{
			yield return new WaitForSecondsRealtime(initializationRetryDuration);
			if (UnityServices.State == ServicesInitializationState.Initialized)
			{
				break;
			}
			Debug.Log("Retrying initialization for Unity Services");
			InitializeUnityServices();
		}
	}

	public IEnumerator CheckInternet(Action<bool> callback)
	{
		Debug.Log("Start checking internet...");
		using UnityWebRequest request = UnityWebRequest.Get("https://clients3.google.com/generate_204");
		request.timeout = 2;
		yield return request.SendWebRequest();
		bool obj = request.result == UnityWebRequest.Result.Success && request.responseCode == 204;
		callback?.Invoke(obj);
	}

	private async void InitializeUnityServices()
	{
		try
		{
			UnityServices.Initialized += UnityServices_OnInitialized;
			UnityServices.InitializeFailed += UnityServices_OnInitializeFailed;
			if (UnityServices.State == ServicesInitializationState.Uninitialized)
			{
				Debug.Log("Services Initializing");
				await UnityServices.InitializeAsync();
			}
			ConsentState consentState = new ConsentState();
			consentState.AdsIntent = ConsentStatus.Granted;
			EndUserConsent.SetConsentState(consentState);
		}
		catch (Exception arg)
		{
			Debug.Log($"Service initialization failed. Message: {arg}");
		}
	}

	public static bool HasNetworkInterface()
	{
		return Application.internetReachability != NetworkReachability.NotReachable;
	}

	private void UnityServices_OnInitialized()
	{
		Debug.Log("Unity Services initialized successfully.");
	}

	private void UnityServices_OnInitializeFailed(Exception obj)
	{
		Debug.Log("Unity Services initialization failed");
	}
}
