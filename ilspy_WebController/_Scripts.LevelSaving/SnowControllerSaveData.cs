namespace _Scripts.LevelSaving;

public struct SnowControllerSaveData : IHasId
{
	public string id;

	public string name;

	public byte[] renderTextureData;

	public string Id => id;
}
