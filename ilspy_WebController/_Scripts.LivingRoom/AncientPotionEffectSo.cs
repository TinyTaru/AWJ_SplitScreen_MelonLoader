using UnityEngine;
using _Scripts.General;

namespace _Scripts.LivingRoom;

[CreateAssetMenu(menuName = "FTG/New Ancient Potion Effect", fileName = "New Ancient Potion Effect", order = 0)]
public class AncientPotionEffectSo : ScriptableObject
{
	public AncientPotionEffectType effectType;

	public float effectDuration;

	public Sprite effectSprite;
}
