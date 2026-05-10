namespace _Scripts.LevelSaving;

public struct CleanableObjectSaveData : IHasId
{
	public string id;

	public string name;

	public float dirtAmount;

	public string Id => id;
}
