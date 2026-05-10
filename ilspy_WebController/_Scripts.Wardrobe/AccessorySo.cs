using UnityEngine;

namespace _Scripts.Wardrobe;

[CreateAssetMenu(menuName = "FTG/New Accessory So", fileName = "New Accessory So", order = 0)]
public class AccessorySo : ScriptableObject
{
	public Accessory accessory;

	public Sprite accessorySprite;

	public string accessorySound;
}
