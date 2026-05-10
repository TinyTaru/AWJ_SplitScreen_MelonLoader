using UnityEngine;

namespace _Scripts.LivingRoom;

[CreateAssetMenu(menuName = "FTG/New Tablet Shop Item", fileName = "New Tablet Shop Item", order = 0)]
public class TabletShopItemSo : ScriptableObject
{
	public GameObject spawnableObjectPrefab;

	public Sprite sprite;

	public float price = 1.99f;

	public int maxAmount = 5;

	public float spawnInterval = 1f;
}
