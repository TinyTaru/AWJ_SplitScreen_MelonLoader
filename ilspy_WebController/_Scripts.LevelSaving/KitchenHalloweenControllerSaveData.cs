namespace _Scripts.LevelSaving;

public struct KitchenHalloweenControllerSaveData : IHasId
{
	public string id;

	public bool halloweenEventIsActive;

	public bool halloweenObjectsAreActive;

	public string Id => id;
}
