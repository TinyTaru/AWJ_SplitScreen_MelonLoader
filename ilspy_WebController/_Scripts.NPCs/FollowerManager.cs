using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _Scripts.Spider;

namespace _Scripts.NPCs;

public class FollowerManager : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> followers = new List<GameObject>();

	public void AddFollower(GameObject go)
	{
		if (!followers.Contains(go))
		{
			followers.Add(go);
		}
	}

	public bool Contains(GameObject go)
	{
		return followers.Contains(go);
	}

	public void RemoveFollower(GameObject go)
	{
		if (!followers.Contains(go))
		{
			return;
		}
		int num = followers.IndexOf(go);
		followers.Remove(go);
		if (num == 0)
		{
			if (followers.Count == 0)
			{
				return;
			}
			followers[num].GetComponentInParent<BodyMovement>().FollowTarget = base.transform;
			num++;
		}
		for (int i = num; i < followers.Count; i++)
		{
			followers[i].GetComponentInParent<BodyMovement>().FollowTarget = followers[i - 1].transform;
		}
	}

	public void ForceRemoveAllFollowers()
	{
		foreach (GameObject follower in followers)
		{
			NPC component = follower.GetComponent<NPC>();
			if (!(component == null))
			{
				component.OnPlayerRespawned();
			}
		}
		followers.Clear();
	}

	public Transform GetNextFollowTarget()
	{
		if (followers.Count != 0)
		{
			return followers.Last().transform;
		}
		return base.transform;
	}
}
