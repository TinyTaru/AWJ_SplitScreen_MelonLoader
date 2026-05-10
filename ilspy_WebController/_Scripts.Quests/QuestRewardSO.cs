using Sirenix.OdinInspector;
using UnityEngine;

namespace _Scripts.Quests;

[CreateAssetMenu(fileName = "New Quest Reward", menuName = "FTG/New Quest Reward")]
public class QuestRewardSO : SerializedScriptableObject
{
	public string rewardNameEnglish;

	public string rewardNameGerman;

	public GameObject questReward;
}
