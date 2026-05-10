using _Scripts.Objects;

namespace _Scripts.LevelSaving;

public struct MagneticLockSaveData : IHasId
{
	public string id;

	public string name;

	public bool magneticLockEnabled;

	public MagneticLock.MagneticLockState lockState;

	public string connectedObjectId;

	public int magneticAnchorIndex;

	public string Id => id;
}
