using _Scripts.Office;

namespace _Scripts.LevelSaving;

public struct OfficeComputerSaveData : IHasId
{
	public string id;

	public int screwCounter;

	public int shmoopCounter;

	public OfficeComputer.DebuggingStep debuggingStep;

	public int insertedParts;

	public bool glassRemoved;

	public bool gpuRemoved;

	public bool cpuRemoved;

	public bool panelOpened;

	public bool ramStick1Removed;

	public bool ramStick2Removed;

	public string Id => id;
}
