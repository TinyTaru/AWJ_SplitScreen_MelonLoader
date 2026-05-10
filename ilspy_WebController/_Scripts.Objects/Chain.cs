using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;
using _Scripts.LevelSaving;
using _Scripts.Singletons;

namespace _Scripts.Objects;

public class Chain : MonoBehaviour, IInitializable<ChainSaveData>, IHasSaveData<ChainSaveData>
{
	[SerializeField]
	private float linkLength = 3.333333f;

	[SerializeField]
	private int linkAmount;

	[SerializeField]
	private bool linkCollisionsEnabled = true;

	[SerializeField]
	private Rigidbody chainStartPrefab;

	[SerializeField]
	private Rigidbody chainLinkPrefab;

	[SerializeField]
	private Transform linkContainer;

	[SerializeField]
	private SplineComputer splineComputer;

	[SerializeField]
	private Dreamteck.Splines.SplineMesh splineMesh;

	[SerializeField]
	private SplineComputer initialSpline;

	[SerializeField]
	private SplinePositioner splinePositioner;

	[SerializeField]
	private SplineUser splineUser;

	[SerializeField]
	private float splineUserUpdateThreshold = 0.1f;

	[SerializeField]
	private bool useFixedJointForEndConnection = true;

	[Header("Start to End")]
	[SerializeField]
	private bool startToEnd;

	[SerializeField]
	private Rigidbody connectedObjectStart;

	[SerializeField]
	private Rigidbody connectedObjectEnd;

	[SerializeField]
	private bool followInitialSpline;

	private List<Rigidbody> linkList;

	private List<Vector3> linkPositions;

	private void Start()
	{
		linkList = new List<Rigidbody>();
		linkPositions = new List<Vector3>();
		foreach (Transform item in linkContainer)
		{
			Object.Destroy(item.gameObject);
		}
		if (startToEnd)
		{
			linkList.Add(connectedObjectStart);
			linkPositions.Add(connectedObjectStart.position);
			if (followInitialSpline)
			{
				int num = Mathf.FloorToInt(initialSpline.CalculateLength() / linkLength);
				for (int i = 0; i < num; i++)
				{
					Vector3 vector = splinePositioner.EvaluatePosition((float)(i + 1) / (float)num);
					Vector3 vector2 = splinePositioner.EvaluatePosition((float)i / (float)num);
					Vector3 normalized = (vector - vector2).normalized;
					Vector3 position = ((i < num - 1) ? vector : connectedObjectEnd.position);
					Rigidbody rigidbody = Object.Instantiate(chainLinkPrefab, position, Quaternion.LookRotation(normalized), linkContainer);
					UniqueID component = rigidbody.GetComponent<UniqueID>();
					if (component != null)
					{
						component.GenerateNewID();
					}
					rigidbody.linearVelocity = Vector3.zero;
					rigidbody.angularVelocity = Vector3.zero;
					ConfigurableJoint component2 = rigidbody.GetComponent<ConfigurableJoint>();
					component2.connectedBody = linkList[i];
					component2.enableCollision = linkCollisionsEnabled;
					component2.anchor = new Vector3(0f, 0f, 0f - linkLength);
					linkList.Add(rigidbody);
					linkPositions.Add(rigidbody.position);
					if (i == num - 1)
					{
						if (useFixedJointForEndConnection)
						{
							FixedJoint fixedJoint = component2.gameObject.AddComponent<FixedJoint>();
							fixedJoint.connectedBody = connectedObjectEnd;
							fixedJoint.enableCollision = false;
							fixedJoint.anchor = new Vector3(0f, 0f, 0f - linkLength);
						}
						else
						{
							ConfigurableJoint configurableJoint = component2.gameObject.AddComponent<ConfigurableJoint>();
							configurableJoint.connectedBody = connectedObjectEnd;
							configurableJoint.enableCollision = false;
							configurableJoint.anchor = new Vector3(0f, 0f, 0f);
							configurableJoint.xMotion = ConfigurableJointMotion.Locked;
							configurableJoint.yMotion = ConfigurableJointMotion.Locked;
							configurableJoint.zMotion = ConfigurableJointMotion.Locked;
						}
					}
				}
			}
			else
			{
				Vector3 vector3 = connectedObjectEnd.position - connectedObjectStart.position;
				Vector3 normalized2 = vector3.normalized;
				int num2 = Mathf.FloorToInt(vector3.magnitude / linkLength);
				for (int j = 0; j < num2; j++)
				{
					Vector3 position2 = ((j < num2 - 1) ? (linkList[j].position + normalized2 * linkLength) : connectedObjectEnd.position);
					Rigidbody rigidbody2 = Object.Instantiate(chainLinkPrefab, position2, Quaternion.LookRotation(normalized2), linkContainer);
					UniqueID component3 = rigidbody2.GetComponent<UniqueID>();
					if (component3 != null)
					{
						component3.GenerateNewID();
					}
					ConfigurableJoint component4 = rigidbody2.GetComponent<ConfigurableJoint>();
					component4.connectedBody = linkList[j];
					component4.enableCollision = linkCollisionsEnabled;
					component4.anchor = new Vector3(0f, 0f, 0f - linkLength);
					linkList.Add(rigidbody2);
					linkPositions.Add(rigidbody2.position);
					if (j == num2 - 1)
					{
						if (useFixedJointForEndConnection)
						{
							FixedJoint fixedJoint2 = component4.gameObject.AddComponent<FixedJoint>();
							fixedJoint2.connectedBody = connectedObjectEnd;
							fixedJoint2.enableCollision = false;
							fixedJoint2.anchor = new Vector3(0f, 0f, 0f - linkLength);
						}
						else
						{
							ConfigurableJoint configurableJoint2 = component4.gameObject.AddComponent<ConfigurableJoint>();
							configurableJoint2.connectedBody = connectedObjectEnd;
							configurableJoint2.enableCollision = false;
							configurableJoint2.anchor = new Vector3(0f, 0f, 0f);
							configurableJoint2.xMotion = ConfigurableJointMotion.Locked;
							configurableJoint2.yMotion = ConfigurableJointMotion.Locked;
							configurableJoint2.zMotion = ConfigurableJointMotion.Locked;
						}
					}
				}
			}
		}
		else
		{
			Rigidbody rigidbody3 = connectedObjectStart;
			if (connectedObjectStart == null)
			{
				rigidbody3 = Object.Instantiate(chainStartPrefab, linkContainer.position, Quaternion.identity, linkContainer);
				UniqueID component5 = rigidbody3.GetComponent<UniqueID>();
				if (component5 != null)
				{
					component5.GenerateNewID();
				}
			}
			linkList.Add(rigidbody3);
			linkPositions.Add(rigidbody3.position);
			for (int k = 0; k < linkAmount; k++)
			{
				Rigidbody rigidbody4 = Object.Instantiate(chainLinkPrefab, linkList[k].position + base.transform.up * linkLength, Quaternion.identity, linkContainer);
				UniqueID component6 = rigidbody4.GetComponent<UniqueID>();
				if (component6 != null)
				{
					component6.GenerateNewID();
				}
				ConfigurableJoint component7 = rigidbody4.GetComponent<ConfigurableJoint>();
				component7.connectedBody = linkList[k];
				component7.enableCollision = linkCollisionsEnabled;
				component7.anchor = new Vector3(0f, 0f, 0f - linkLength);
				linkList.Add(rigidbody4);
				linkPositions.Add(rigidbody4.position);
			}
		}
		for (int l = 0; l < linkList.Count; l++)
		{
			splineComputer.SetPointSize(l, 1f);
		}
		splineMesh.GetChannel(0).count = linkList.Count * 3;
		if (initialSpline != null)
		{
			initialSpline.gameObject.SetActive(value: false);
		}
		if (splinePositioner != null)
		{
			splinePositioner.gameObject.SetActive(value: false);
		}
	}

	private void FixedUpdate()
	{
		float num = 0f;
		for (int i = 0; i < linkList.Count; i++)
		{
			num += Vector3.SqrMagnitude(linkPositions[i] - linkList[i].position);
		}
		if (num > splineUserUpdateThreshold)
		{
			for (int j = 0; j < linkList.Count; j++)
			{
				Vector3 position = linkList[j].position;
				splineComputer.SetPointPosition(j, position);
				linkPositions[j] = position;
			}
			splineComputer.RebuildImmediate();
			splineUser.RebuildImmediate();
		}
	}

	public void Initialize(ChainSaveData saveData)
	{
		if (linkList == null)
		{
			return;
		}
		for (int i = 0; i < linkList.Count; i++)
		{
			Rigidbody rigidbody = linkList[i];
			UniqueID component = rigidbody.GetComponent<UniqueID>();
			if (component == null)
			{
				Debug.LogError("chainLink " + rigidbody.name + " has no uniqueID");
				continue;
			}
			component.ForceID(saveData.chainLinkSaveDataList[i].id);
			LevelSavingController.TryAddUniqueGameObjectById(component.ID, rigidbody.gameObject);
		}
	}

	public ChainSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Chain " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		List<ChainLinkSaveData> list = new List<ChainLinkSaveData>();
		foreach (Rigidbody link in linkList)
		{
			ChainLinkSaveData item = default(ChainLinkSaveData);
			UniqueID component2 = link.GetComponent<UniqueID>();
			if (component2 != null)
			{
				item.id = component2.ID;
			}
			item.name = link.gameObject.name;
			list.Add(item);
		}
		ChainSaveData result = default(ChainSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.chainLinkSaveDataList = list;
		return result;
	}
}
