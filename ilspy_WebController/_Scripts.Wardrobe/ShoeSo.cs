using UnityEngine;

namespace _Scripts.Wardrobe;

[CreateAssetMenu(menuName = "FTG/New Shoe SO", fileName = "New Shoe SO", order = 0)]
public class ShoeSo : ScriptableObject
{
	public Shoe shoe;

	public Sprite shoeSprite;

	public string shoeSound;
}
