namespace _Scripts.LevelSaving;

public struct KitchenSinkSaveData : IHasId
{
	public string id;

	public string name;

	public float waterLevel;

	public string currentWaterOrbId;

	public string Id => id;
}
