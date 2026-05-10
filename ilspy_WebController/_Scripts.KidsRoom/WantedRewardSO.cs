using UnityEngine;

namespace _Scripts.KidsRoom;

[CreateAssetMenu(fileName = "New WantedRewardSO", menuName = "ScriptableObjects/WantedRewardSO")]
public class WantedRewardSO : ScriptableObject
{
	public GameObject rewardActor;

	public int rewardQuantity;
}
