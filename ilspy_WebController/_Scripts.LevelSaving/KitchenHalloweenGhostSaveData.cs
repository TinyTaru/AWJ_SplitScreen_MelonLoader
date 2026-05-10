using UnityEngine;

namespace _Scripts.LevelSaving;

public struct KitchenHalloweenGhostSaveData : IHasId
{
	public string id;

	public string name;

	public Color color;

	public float spawnTime;

	public string Id => id;
}
