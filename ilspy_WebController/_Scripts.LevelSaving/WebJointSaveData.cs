using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.LevelSaving;

public struct WebJointSaveData : IHasId
{
	public string id;

	public Vector3 position;

	public bool isKinematic;

	public List<string> connectedWebJointIDs;

	public List<string> attachedWebThreadIDs;

	public bool hasFixedJoint;

	public string fixedJointConnectedBodyID;

	public List<SpringJointSaveData> springJointSaveDataList;

	public string Id => id;
}
