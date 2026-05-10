using System.Collections;
using UnityEngine;

namespace _Scripts.Miscellaneous.Christmas;

public class Rocket : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem trailEffect;

	[SerializeField]
	private ParticleSystem explosionEffect;

	[SerializeField]
	private float speed;

	[SerializeField]
	private float travelTimeMin;

	[SerializeField]
	private float travelTimeMax;

	[SerializeField]
	private float explosionDelay;

	private bool isMoving;

	private void Start()
	{
		isMoving = true;
		trailEffect.Play();
		StartCoroutine(RocketCoroutine());
	}

	private void Update()
	{
		if (isMoving)
		{
			base.transform.Translate(base.transform.up * speed * Time.deltaTime, Space.World);
		}
	}

	private IEnumerator RocketCoroutine()
	{
		yield return new WaitForSeconds(Random.Range(travelTimeMin, travelTimeMax));
		isMoving = false;
		trailEffect.Stop();
		yield return new WaitForSeconds(explosionDelay);
		explosionEffect.Play();
		Object.Destroy(base.gameObject, 3f);
	}
}
