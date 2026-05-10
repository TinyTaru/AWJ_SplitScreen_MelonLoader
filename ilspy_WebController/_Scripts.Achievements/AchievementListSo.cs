using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Achievements;

[CreateAssetMenu(menuName = "FTG/Achievements/New Achievement List")]
public class AchievementListSo : ScriptableObject
{
	public List<AchievementSo> achievements;
}
