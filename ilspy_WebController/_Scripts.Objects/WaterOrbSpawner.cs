using UnityEngine;

namespace _Scripts.Objects;

public class WaterOrbSpawner : MonoBehaviour
{
	[SerializeField]
	private WaterOrb waterOrbPrefab;

	[SerializeField]
	private Transform waterOrbSpawnPosition;

	[SerializeField]
	private float spawnDelay = 5f;

	[SerializeField]
	private float popForce;

	protected WaterOrb currentWaterOrb;

	private float spawnTimer;

	public WaterOrb CurrentWaterOrb => currentWaterOrb;

	private void Awake()
	{
		currentWaterOrb = null;
		spawnTimer = spawnDelay;
	}

	protected void Update()
	{
		if (!(currentWaterOrb != null))
		{
			spawnTimer -= Time.deltaTime;
		}
	}

	protected void OnTriggerExit(Collider other)
	{
		if (other.GetComponentInParent<WaterOrb>() == currentWaterOrb)
		{
			currentWaterOrb = null;
		}
	}

	protected virtual void SpawnWaterOrb()
	{
		currentWaterOrb = Object.Instantiate(waterOrbPrefab, waterOrbSpawnPosition.position, Quaternion.identity, null);
		SpawnableObject component = currentWaterOrb.GetComponent<SpawnableObject>();
		if (component != null)
		{
			component.Setup();
		}
		Vector3 force = base.transform.right * popForce * Random.Range(-1f, 1f);
		currentWaterOrb.GetComponent<MovableObject>().GetRigidbody().AddForce(force, ForceMode.VelocityChange);
		spawnTimer = spawnDelay;
	}

	public void SetCurrentWaterOrb(WaterOrb newWaterOrb)
	{
		currentWaterOrb = newWaterOrb;
	}

	public void TrySpawnWaterOrb()
	{
		if (spawnTimer <= 0f && currentWaterOrb == null)
		{
			SpawnWaterOrb();
		}
	}
}
