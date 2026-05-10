namespace _Scripts.LevelSaving;

public struct BreakableJointSaveData : IHasId
{
	public string id;

	public string name;

	public bool jointIsBroken;

	public string Id => id;
}
