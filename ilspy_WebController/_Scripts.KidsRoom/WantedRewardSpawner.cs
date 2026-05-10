using UnityEngine;

namespace _Scripts.KidsRoom;

public class WantedRewardSpawner : MonoBehaviour
{
	private bool rewardGiven;

	private WantedPosterTrigger[] wantedPosterTriggers;

	[SerializeField]
	[Tooltip("The burst spread of the object, to spawn them in a cone upwards from a position.")]
	private float burstSpread;

	[SerializeField]
	[Tooltip("The burst force of the objects spawned, applied as a multiplier to their upwards direction")]
	private float burstForce;

	private void Awake()
	{
		wantedPosterTriggers = Object.FindObjectsByType<WantedPosterTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
	}

	private void Start()
	{
		WantedPosterTrigger[] array = wantedPosterTriggers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnRewardConditionMet += WantedPosterTrigger_OnRewardConditionMet;
		}
	}

	private void WantedPosterTrigger_OnRewardConditionMet(object sender, WantedPosterTrigger.OnRewardConditionMetEventArgs e)
	{
		TrySpawnReward(e.rewardActor, e.rewardQuantity, e.rewardSpawnPosition);
	}

	private void BurstObject(GameObject rewardObject)
	{
		Rigidbody component = rewardObject.GetComponent<Rigidbody>();
		if ((bool)component)
		{
			float x = Random.Range((0f - burstSpread) / 2f, burstSpread / 2f);
			float y = Random.Range(0f, 360f);
			Vector3 vector = Quaternion.Euler(x, y, 0f) * Vector3.up;
			component.AddForce(vector * burstForce, ForceMode.Impulse);
		}
	}

	public bool TrySpawnReward(GameObject rewardActor, int rewardQuantity, Vector3 rewardPosition)
	{
		if (rewardActor == null)
		{
			Debug.LogError("WantedRewardSpawner asked to spawn a reward with no rewardActor set!");
			return false;
		}
		if (rewardQuantity <= 0)
		{
			Debug.LogError($"WantedRewardSpawner asked to spawn a reward with reward quantity {rewardQuantity}!");
			return false;
		}
		for (int i = 0; i < rewardQuantity; i++)
		{
			GameObject rewardObject = Object.Instantiate(rewardActor, rewardPosition, Quaternion.identity);
			BurstObject(rewardObject);
		}
		return false;
	}
}
