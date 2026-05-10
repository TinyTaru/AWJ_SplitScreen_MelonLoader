using UnityEngine;
using UnityEngine.Events;
using _Scripts.NPCs;

namespace _Scripts.Spider;

public class FollowTrigger : MonoBehaviour
{
	public UnityEvent OnFollow;

	private INPC npc;

	private void Start()
	{
		npc = base.transform.parent.GetComponentInChildren<INPC>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && other.name != "SpiderInteraction" && !npc.IsFollowing)
		{
			OnFollow.Invoke();
		}
	}
}
