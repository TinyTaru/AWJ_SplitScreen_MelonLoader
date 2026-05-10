using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public class LoginManager : Singleton<LoginManager>
{
	public Action PlayerSignedIn;

	private string googlePlayGamesToken;

	protected override void Awake()
	{
		base.Awake();
	}

	public void StartAnonymousSignIn()
	{
		SignInAnonymouslyAsync();
	}

	private async Task SignInAnonymouslyAsync()
	{
		try
		{
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
			Debug.Log("Sign in anonymously succeeded!");
			Debug.Log("PlayerID: " + AuthenticationService.Instance.PlayerId);
			PlayerSignedIn?.Invoke();
		}
		catch (AuthenticationException exception)
		{
			Debug.LogException(exception);
		}
		catch (RequestFailedException exception2)
		{
			Debug.LogException(exception2);
		}
	}

	public void OnGoogleSignInButtonPressed()
	{
	}
}
