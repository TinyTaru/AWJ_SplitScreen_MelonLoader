using System;
using Steamworks;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Steam;

public class SteamOverlay : MonoBehaviour
{
	private const string steamPageURL = "https://store.steampowered.com/app/2073910/A_Webbing_Journey/";

	public void OpenSteamPageInOverlay()
	{
		if (SettingsController.EventMode)
		{
			return;
		}
		try
		{
			SteamFriends.ActivateGameOverlayToWebPage("https://store.steampowered.com/app/2073910/A_Webbing_Journey/");
		}
		catch (Exception)
		{
			Application.OpenURL("https://store.steampowered.com/app/2073910/A_Webbing_Journey/");
		}
	}
}
