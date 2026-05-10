namespace _Scripts.LevelSaving;

public struct StarShooterSaveData : IHasId
{
	public string id;

	public string name;

	public float angle;

	public bool starLoaded;

	public bool shmoopLoaded;

	public string Id => id;
}
