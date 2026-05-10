namespace _Scripts.LevelSaving;

public struct BurnableObjectSaveData : IHasId
{
	public string id;

	public string name;

	public float burnAmount;

	public string Id => id;
}
