using System;
using UnityEngine;
using _Scripts.Objects;

namespace _Scripts.Office;

public class NerfGun : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private MovableObject nerfGunBulletPrefab;

	[SerializeField]
	private Transform bulletSpawnTransform;

	[SerializeField]
	private NerfGunMinigame nerfGunMinigame;

	[Header("Parameters")]
	[SerializeField]
	private float spawnInterval = 1f;

	[SerializeField]
	private float bulletSpeed = 20f;

	private float spawnTimer;

	private bool isShooting;

	private void Start()
	{
		spawnTimer = spawnInterval;
		isShooting = false;
		if (nerfGunMinigame != null)
		{
			nerfGunMinigame.OnMiniGameStarted += NerfGunMinigame_OnMiniGameStarted;
			nerfGunMinigame.OnMiniGameStopped += NerfGunMinigame_OnMiniGameStopped;
		}
	}

	private void Update()
	{
		if (isShooting)
		{
			spawnTimer -= Time.deltaTime;
			if (spawnTimer <= 0f)
			{
				ShootBullet();
				spawnTimer = spawnInterval;
			}
		}
	}

	private void ShootBullet()
	{
		MovableObject movableObject = UnityEngine.Object.Instantiate(nerfGunBulletPrefab, bulletSpawnTransform.position, bulletSpawnTransform.rotation, null);
		movableObject.GetRigidbody().linearVelocity = movableObject.transform.forward * bulletSpeed;
	}

	private void NerfGunMinigame_OnMiniGameStarted(object sender, EventArgs e)
	{
		isShooting = true;
	}

	private void NerfGunMinigame_OnMiniGameStopped(object sender, EventArgs e)
	{
		isShooting = false;
	}
}
