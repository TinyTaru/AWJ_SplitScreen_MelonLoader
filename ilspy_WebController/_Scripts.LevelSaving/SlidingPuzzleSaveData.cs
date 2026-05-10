namespace _Scripts.LevelSaving;

public struct SlidingPuzzleSaveData : IHasId
{
	public string id;

	public bool isSolved;

	public bool isBroken;

	public bool isCompleted;

	public string Id => id;
}
