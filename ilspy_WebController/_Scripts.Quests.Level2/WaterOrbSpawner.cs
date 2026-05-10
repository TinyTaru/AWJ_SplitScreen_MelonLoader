using System.Collections;
using UnityEngine;

namespace _Scripts.Quests.Level2;

public class WaterOrbSpawner : MonoBehaviour
{
	[SerializeField]
	private WaterOrb waterOrbPrefab;

	[SerializeField]
	private float spawnDelay = 5f;

	private WaterOrb currentWaterOrb;

	private bool isSpawning;

	private void Start()
	{
		SpawnWaterOrb();
	}

	private void OnTriggerExit(Collider other)
	{
		if (!isSpawning && other.GetComponentInParent<WaterOrb>() == currentWaterOrb)
		{
			StartCoroutine(SpawnNewWaterOrb());
		}
	}

	private IEnumerator SpawnNewWaterOrb()
	{
		isSpawning = true;
		currentWaterOrb = null;
		yield return new WaitForSeconds(spawnDelay);
		SpawnWaterOrb();
		isSpawning = false;
	}

	private void SpawnWaterOrb()
	{
		currentWaterOrb = Object.Instantiate(waterOrbPrefab, base.transform.position, Quaternion.identity, null);
	}
}
