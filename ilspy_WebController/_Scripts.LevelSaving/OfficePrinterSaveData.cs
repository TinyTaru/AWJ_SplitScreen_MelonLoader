namespace _Scripts.LevelSaving;

public struct OfficePrinterSaveData : IHasId
{
	public string id;

	public bool isPrinting;

	public float printTimer;

	public int pagesRemaining;

	public float printProgress;

	public string currentPageId;

	public int pageIndex;

	public int printInstructionIndex;

	public string Id => id;
}
