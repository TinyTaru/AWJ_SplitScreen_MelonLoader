using System;
using UnityEngine;
using _Scripts.Miscellaneous.Christmas;
using _Scripts.Objects;
using _Scripts.Singletons;

namespace _Scripts.Miscellaneous.Halloween;

public class KitchenHalloweenGhostSpawner : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Ghost ghostPrefab;

	[SerializeField]
	private Color ghostColorWhite;

	[SerializeField]
	private Color ghostColorRed;

	[SerializeField]
	private Color ghostColorGreen;

	[Header("Settings")]
	[SerializeField]
	private Vector2 spawnIntervalRange = new Vector2(60f, 120f);

	[SerializeField]
	private float circleRadius = 5f;

	[SerializeField]
	private float targetAngleRandomness = 45f;

	[SerializeField]
	private Vector2 heightRange = new Vector2(1f, 5f);

	[SerializeField]
	private float moveSpeed = 5f;

	[SerializeField]
	private int collectibleHatIndex = 13;

	private bool isActive;

	private float spawnTimer;

	private int spawnCounter;

	private bool useChristmasGhostMaterials;

	private void Start()
	{
		if (Singleton<KitchenHalloweenController>.Instance != null)
		{
			Singleton<KitchenHalloweenController>.Instance.OnActivateHalloweenEffects += HalloweenController_OnActivateHalloweenEffects;
		}
		if (Singleton<KitchenChristmasController>.Instance != null)
		{
			Singleton<KitchenChristmasController>.Instance.OnActivateChristmasEffects += ChristmasController_OnActivateChristmasEffects;
		}
	}

	private void Update()
	{
		if (isActive)
		{
			spawnTimer -= Time.deltaTime;
			if (spawnTimer <= 0f)
			{
				SpawnAndMoveObject();
				spawnTimer = UnityEngine.Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
			}
		}
	}

	private void SpawnAndMoveObject()
	{
		spawnCounter++;
		float num = UnityEngine.Random.Range(0f, 360f);
		Vector3 randomPositionOnCircle = GetRandomPositionOnCircle(num);
		float angle = (num + 180f + targetAngleRandomness * UnityEngine.Random.Range(-1f, 1f)) % 360f;
		Vector3 randomPositionOnCircle2 = GetRandomPositionOnCircle(angle);
		Ghost ghost = UnityEngine.Object.Instantiate(ghostPrefab, randomPositionOnCircle, Quaternion.identity);
		ghost.GetComponent<SpawnableObject>().Setup();
		Vector3 velocity = (randomPositionOnCircle2 - ghost.transform.position).normalized * moveSpeed;
		ghost.SetVelocity(velocity);
		ghost.SetCurrentTimeAsSpawnTime();
		if (useChristmasGhostMaterials)
		{
			Color[] array = new Color[3] { ghostColorWhite, ghostColorRed, ghostColorGreen };
			ghost.SetColor(array[UnityEngine.Random.Range(0, array.Length)]);
		}
		else
		{
			ghost.SetColor(ghostColorWhite);
		}
		if (spawnCounter % collectibleHatIndex == 0)
		{
			ghost.ActivateCollectibleHat();
		}
	}

	private Vector3 GetRandomPositionOnCircle(float angle)
	{
		float f = angle * (MathF.PI / 180f);
		float x = Mathf.Cos(f) * circleRadius;
		float z = Mathf.Sin(f) * circleRadius;
		float y = UnityEngine.Random.Range(heightRange.x, heightRange.y);
		return new Vector3(x, y, z);
	}

	public void Initialize(bool newIsActive)
	{
		isActive = newIsActive;
		Activate();
	}

	private void Activate()
	{
		SpawnAndMoveObject();
		spawnTimer = UnityEngine.Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
		isActive = true;
	}

	private void HalloweenController_OnActivateHalloweenEffects(object sender, EventArgs e)
	{
		Activate();
	}

	private void ChristmasController_OnActivateChristmasEffects(object sender, EventArgs e)
	{
		if (Singleton<KitchenHalloweenController>.Instance != null)
		{
			useChristmasGhostMaterials = true;
		}
	}
}
