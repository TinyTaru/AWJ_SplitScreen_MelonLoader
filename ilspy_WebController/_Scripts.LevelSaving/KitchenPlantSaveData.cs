namespace _Scripts.LevelSaving;

public struct KitchenPlantSaveData : IHasId
{
	public string id;

	public string name;

	public bool isWatered;

	public string Id => id;
}
