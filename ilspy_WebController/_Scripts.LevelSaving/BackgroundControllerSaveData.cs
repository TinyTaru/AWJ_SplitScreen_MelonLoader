using _Scripts.General;

namespace _Scripts.LevelSaving;

public struct BackgroundControllerSaveData : IHasId
{
	public string id;

	public BackgroundType backgroundType;

	public string Id => id;
}
