using System;
using UnityEngine;

namespace _Scripts.LevelSaving;

[ExecuteAlways]
[DisallowMultipleComponent]
public class UniqueID : MonoBehaviour
{
	[SerializeField]
	private string uniqueId = Guid.NewGuid().ToString();

	[SerializeField]
	private string manualId;

	private bool forcedID;

	public string ID => uniqueId;

	public void GenerateNewID()
	{
		if (!forcedID)
		{
			uniqueId = Guid.NewGuid().ToString();
		}
	}

	private void OverwriteUniqueIdWithManualId()
	{
		uniqueId = manualId;
	}

	public void ForceID(string id)
	{
		uniqueId = id;
		forcedID = true;
	}
}
