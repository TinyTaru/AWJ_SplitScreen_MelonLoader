namespace _Scripts.LevelSaving;

public struct RocketQuestSaveData : IHasId
{
	public string id;

	public bool rocketIsLocked;

	public bool readyForLaunch;

	public float launchPower;

	public string Id => id;
}
