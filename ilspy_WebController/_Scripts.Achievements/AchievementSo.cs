using UnityEngine;

namespace _Scripts.Achievements;

[CreateAssetMenu(menuName = "FTG/Achievements/New Achievement")]
public class AchievementSo : ScriptableObject
{
	[Header("IDs")]
	public string id;

	public string steamAPIName;

	[Header("Text")]
	public string displayName;

	public string description;

	[Header("Images")]
	public Sprite iconUnachieved;

	public Sprite iconAchieved;
}
