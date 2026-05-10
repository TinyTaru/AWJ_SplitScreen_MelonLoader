using UnityEngine;

namespace _Scripts.Wardrobe;

[CreateAssetMenu(menuName = "FTG/New Eye SO", fileName = "New Eye SO", order = 0)]
public class EyeSo : ScriptableObject
{
	public Eye eye;

	public Sprite eyeSprite;

	public string eyeSound;
}
