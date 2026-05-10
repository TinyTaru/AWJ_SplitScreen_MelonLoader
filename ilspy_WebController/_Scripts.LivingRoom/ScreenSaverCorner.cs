using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.LivingRoom;

public class ScreenSaverCorner : MonoBehaviour
{
	[SerializeField]
	private TvLivingRoom tvLivingRoom;

	[SerializeField]
	private Transform screenSaverLogo;

	[SerializeField]
	private float distanceThreshold = 0.1f;

	[SerializeField]
	private float cooldownTime = 1f;

	[SerializeField]
	private UnityEvent onCornerHitEvent;

	private float cooldownTimer;

	private void Update()
	{
		cooldownTimer -= Time.deltaTime;
		if (tvLivingRoom.ScreenSaverIsActive && !(cooldownTimer > 0f) && (screenSaverLogo.position - base.transform.position).sqrMagnitude <= distanceThreshold * distanceThreshold)
		{
			onCornerHitEvent?.Invoke();
			cooldownTimer = cooldownTime;
		}
	}
}
