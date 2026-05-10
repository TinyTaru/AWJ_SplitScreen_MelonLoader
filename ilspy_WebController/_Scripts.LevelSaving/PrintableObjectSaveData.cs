using UnityEngine;

namespace _Scripts.LevelSaving;

public struct PrintableObjectSaveData : IHasId
{
	public string id;

	public string name;

	public Color color;

	public string Id => id;
}
