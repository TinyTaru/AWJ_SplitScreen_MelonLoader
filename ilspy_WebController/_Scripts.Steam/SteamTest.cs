using Steamworks;
using UnityEngine;

namespace _Scripts.Steam;

public class SteamTest : MonoBehaviour
{
	private Callback<GameOverlayActivated_t> gameOverlayActivated;

	private CallResult<NumberOfCurrentPlayers_t> numberOfCurrentPlayers;

	private void OnEnable()
	{
		if (SteamManager.Initialized)
		{
			gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
			numberOfCurrentPlayers = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
		}
	}

	private void Start()
	{
		if (SteamManager.Initialized)
		{
			SteamFriends.GetPersonaName();
		}
	}

	private void Update()
	{
	}

	private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
	{
		_ = pCallback.m_bActive;
	}

	private void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure)
	{
		_ = pCallback.m_bSuccess != 1 || bIOFailure;
	}
}
