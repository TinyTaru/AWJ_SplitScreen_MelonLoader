using System.Collections;
using UnityEngine;

public class RocketSpawner : MonoBehaviour
{
	[SerializeField]
	private GameObject RocketPrefab;

	[SerializeField]
	private float spawnIntervalMin;

	[SerializeField]
	private float spawnIntervalMax;

	[SerializeField]
	private float angleMin;

	[SerializeField]
	private float angleMax;

	private void Start()
	{
		StartCoroutine(SpawnRocketCoroutine());
	}

	private void Update()
	{
	}

	private IEnumerator SpawnRocketCoroutine()
	{
		while (true)
		{
			SpawnRocket();
			yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
		}
	}

	private void SpawnRocket()
	{
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		eulerAngles.z += Random.Range(angleMin, angleMax);
		Object.Instantiate(RocketPrefab, base.transform.position, Quaternion.Euler(eulerAngles));
	}
}
