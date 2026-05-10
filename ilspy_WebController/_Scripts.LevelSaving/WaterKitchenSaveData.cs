namespace _Scripts.LevelSaving;

public struct WaterKitchenSaveData : IHasId
{
	public string id;

	public string name;

	public float waterLevel;

	public bool drainIsOpen;

	public bool isFillingUp;

	public bool isFull;

	public bool isEmpty;

	public string Id => id;
}
