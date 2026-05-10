using _Scripts.Puzzles;

namespace _Scripts.LevelSaving;

public struct MagneticObjectSaveData : IHasId
{
	public string id;

	public string name;

	public MagneticObject.MagneticObjectState magneticObjectState;

	public string Id => id;
}
