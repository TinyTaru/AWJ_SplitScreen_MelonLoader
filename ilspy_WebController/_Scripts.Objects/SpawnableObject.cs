using System;
using UnityEngine;
using _Scripts.General;
using _Scripts.LevelSaving;

namespace _Scripts.Objects;

[RequireComponent(typeof(MovableObject), typeof(UniqueID))]
[DisallowMultipleComponent]
public class SpawnableObject : MonoBehaviour
{
	[SerializeField]
	private SpawnableObjectType spawnableObjectType;

	public event Action OnSetupDone;

	public void Setup()
	{
		UniqueID component = GetComponent<UniqueID>();
		if (component != null)
		{
			component.GenerateNewID();
		}
		this.OnSetupDone?.Invoke();
	}

	public SpawnableObjectType GetSpawnableObjectType()
	{
		return spawnableObjectType;
	}
}
