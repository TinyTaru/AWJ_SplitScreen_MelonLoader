using UnityEngine;
using _Scripts.Environment;
using _Scripts.Singletons;

namespace _Scripts.Objects;

[RequireComponent(typeof(MovableObject))]
public class WaterOrb : MonoBehaviour
{
	[SerializeField]
	private GameObject waterSplashEffect;

	private MovableObject movableObject;

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
	}

	protected void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.GetComponentInParent<InstantKillSurface>() != null)
		{
			DestroyWaterOrb();
		}
		IAffectedByWater componentInParent = collision.gameObject.GetComponentInParent<IAffectedByWater>();
		if (componentInParent != null)
		{
			componentInParent.TouchedByWater();
			DestroyWaterOrb();
		}
	}

	protected void DestroyWaterOrb()
	{
		if (Singleton<WebController>.Instance.WebTargetObject == base.gameObject)
		{
			Singleton<WebController>.Instance.ReleaseWeb();
		}
		Object.Instantiate(waterSplashEffect, base.transform.position, Quaternion.identity, null);
		movableObject.DestroySafely();
	}
}
