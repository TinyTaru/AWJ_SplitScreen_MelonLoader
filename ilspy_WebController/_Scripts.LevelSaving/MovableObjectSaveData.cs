using UnityEngine;

namespace _Scripts.LevelSaving;

public struct MovableObjectSaveData : IHasId
{
	public string id;

	public string name;

	public Vector3 position;

	public Quaternion rotation;

	public Vector3 linearVelocity;

	public Vector3 angularVelocity;

	public string Id => id;
}
