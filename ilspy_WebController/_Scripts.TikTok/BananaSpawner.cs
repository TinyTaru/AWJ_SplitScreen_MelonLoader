using UnityEngine;
using UnityEngine.Events;
using _Scripts.Objects;

namespace _Scripts.TikTok;

public class BananaSpawner : MonoBehaviour
{
	[SerializeField]
	private Rigidbody bananaPrefab;

	[SerializeField]
	private Transform spawnPosition;

	[SerializeField]
	private float spawnForceMin;

	[SerializeField]
	private float spawnForceMax;

	[SerializeField]
	private float spawnTorqueMin;

	[SerializeField]
	private float spawnTorqueMax;

	[SerializeField]
	private float buttonOnThreshold = 0.05f;

	[SerializeField]
	private float buttonOffThreshold;

	[SerializeField]
	private Rigidbody buttonOneShot;

	[SerializeField]
	private Rigidbody buttonContinuous;

	[SerializeField]
	private UnityEvent onBananaSpawnedEvent;

	[SerializeField]
	private float spawnInterval;

	private bool oneShotIsPressed;

	private bool continuousIsPressed;

	private float spawnTimer;

	private void Start()
	{
		spawnTimer = 0f;
	}

	private void Update()
	{
		if (!oneShotIsPressed)
		{
			if (buttonOneShot.transform.localPosition.z > buttonOnThreshold)
			{
				oneShotIsPressed = true;
				SpawnBanana();
			}
		}
		else if (buttonOneShot.transform.localPosition.z < buttonOffThreshold)
		{
			oneShotIsPressed = false;
		}
		if (!continuousIsPressed)
		{
			if (buttonContinuous.transform.localPosition.z > buttonOnThreshold)
			{
				continuousIsPressed = true;
			}
		}
		else if (buttonContinuous.transform.localPosition.z < buttonOffThreshold)
		{
			continuousIsPressed = false;
		}
		spawnTimer -= Time.deltaTime;
		if (continuousIsPressed && spawnTimer <= 0f)
		{
			spawnTimer = spawnInterval;
			SpawnBanana();
		}
	}

	private void SpawnBanana()
	{
		Rigidbody rigidbody = Object.Instantiate(bananaPrefab, spawnPosition.position, Quaternion.identity, null);
		rigidbody.rotation = Quaternion.Euler(Random.Range(spawnTorqueMin, spawnTorqueMax), Random.Range(spawnTorqueMin, spawnTorqueMax), Random.Range(spawnTorqueMin, spawnTorqueMax));
		rigidbody.AddForce(spawnPosition.up * Random.Range(spawnForceMin, spawnForceMax), ForceMode.Impulse);
		rigidbody.AddTorque(new Vector3(Random.Range(spawnTorqueMin, spawnTorqueMax), Random.Range(spawnTorqueMin, spawnTorqueMax), Random.Range(spawnTorqueMin, spawnTorqueMax)), ForceMode.Impulse);
		SpawnableObject component = rigidbody.GetComponent<SpawnableObject>();
		if (component != null)
		{
			component.Setup();
		}
		onBananaSpawnedEvent?.Invoke();
	}
}
