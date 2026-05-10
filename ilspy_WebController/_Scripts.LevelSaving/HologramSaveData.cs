namespace _Scripts.LevelSaving;

public struct HologramSaveData : IHasId
{
	public string id;

	public string name;

	public bool buildInProgress;

	public int currentLayer;

	public int[] amountOfTilesPerLayer;

	public string Id => id;
}
