using UnityEngine;

namespace _Scripts.LevelSaving;

public struct WebThreadSaveData : IHasId
{
	public string id;

	public string[] webJoints;

	public Color webColor;

	public float webThickness;

	public int webIndex;

	public string Id => id;
}
