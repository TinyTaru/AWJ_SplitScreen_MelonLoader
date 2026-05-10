using _Scripts.General;

namespace _Scripts.LevelSaving;

public struct SpawnedObjectSaveData : IHasId
{
	public string id;

	public string name;

	public SpawnableObjectType spawnableObjectType;

	public string Id => id;
}
