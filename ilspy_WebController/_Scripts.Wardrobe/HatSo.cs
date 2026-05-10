using UnityEngine;

namespace _Scripts.Wardrobe;

[CreateAssetMenu(menuName = "FTG/New Hat SO", fileName = "New Hat SO", order = 0)]
public class HatSo : ScriptableObject
{
	public Hat hat;

	public Sprite hatSprite;

	public string hatSound;
}
