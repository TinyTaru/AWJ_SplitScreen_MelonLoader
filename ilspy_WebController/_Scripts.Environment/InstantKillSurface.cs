using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Environment;

public class InstantKillSurface : MonoBehaviour
{
	[SerializeField]
	private Transform respawnPosition;

	[SerializeField]
	private UnityEvent onKillPlayerEvent;

	public Transform RespawnPosition => respawnPosition;

	public void KillPlayer()
	{
		onKillPlayerEvent?.Invoke();
	}
}
