using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Spider;

namespace _Scripts.Objects;

public class PlayerCannon : MonoBehaviour, IAffectedByWater
{
	[SerializeField]
	private float yeetForceMagnitude = 2000f;

	[Header("References")]
	[SerializeField]
	private Transform yeetDirectionTransform;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onPlayerYeetedEvent;

	private void OnTriggerEnter(Collider other)
	{
		BodyMovement componentInParent = other.GetComponentInParent<BodyMovement>();
		if (componentInParent != null)
		{
			YeetPlayer(componentInParent);
		}
	}

	private IEnumerator YeetPlayerCoroutine(BodyMovement player)
	{
		player.YeetPlayer(yeetForceMagnitude, yeetDirectionTransform.transform.forward);
		onPlayerYeetedEvent?.Invoke();
		yield return new WaitForSeconds(0.1f);
	}

	private IEnumerator DeactivateAfterDelayCoroutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		base.gameObject.SetActive(value: false);
	}

	private void YeetPlayer(BodyMovement player)
	{
		StartCoroutine(YeetPlayerCoroutine(player));
	}

	public void DeactivateAfterDelay(float delay)
	{
		StartCoroutine(DeactivateAfterDelayCoroutine(delay));
	}
}
