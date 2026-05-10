namespace _Scripts.LevelSaving;

public struct SnowballSaveData : IHasId
{
	public string id;

	public string name;

	public float size;

	public float defaultMass;

	public string Id => id;
}
