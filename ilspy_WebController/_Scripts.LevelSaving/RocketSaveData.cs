namespace _Scripts.LevelSaving;

public struct RocketSaveData : IHasId
{
	public string id;

	public string name;

	public bool isFlying;

	public float thrustDuration;

	public float thrustTimer;

	public string Id => id;
}
