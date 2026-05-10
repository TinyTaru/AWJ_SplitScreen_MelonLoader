namespace _Scripts.LevelSaving;

public struct DefaultWebSaveData : IHasId
{
	public string name;

	public string id;

	public bool isInitialized;

	public string Id => id;
}
