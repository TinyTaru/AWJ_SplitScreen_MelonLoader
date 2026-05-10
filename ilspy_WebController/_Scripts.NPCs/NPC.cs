using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;
using _Scripts.Spider;

namespace _Scripts.NPCs;

public class NPC : MonoBehaviour, INPC
{
	protected BodyMovement bodyMovement;

	[SerializeField]
	protected bool followInfinitely;

	[SerializeField]
	private UnityEvent onPlayerRespawn;

	private FollowerManager followerManager;

	private BodyMovement.PlayerInteraction defaultPlayerInteraction;

	public bool IsFollowing => bodyMovement.FollowTarget != null;

	protected virtual void Start()
	{
		bodyMovement = GetComponentInParent<BodyMovement>();
		followerManager = Singleton<GameController>.Instance.Player.GetComponent<FollowerManager>();
		defaultPlayerInteraction = bodyMovement.GetPlayerInteraction;
	}

	protected virtual void Update()
	{
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		if (!IgnoreCollision(other) && bodyMovement.CanFollow)
		{
			StartFollowing();
		}
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		if (!IgnoreCollision(other) && !followInfinitely && bodyMovement.CanFollow)
		{
			StopFollowing();
		}
	}

	protected virtual void StartLookingAtPlayer()
	{
		bodyMovement.SetPlayerInteraction(BodyMovement.PlayerInteraction.LookAt);
	}

	public virtual void ContinueDailyRoutine()
	{
	}

	public bool IgnoreCollision(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			return other.name != "SpiderInteraction";
		}
		return true;
	}

	public void OnPlayerRespawned()
	{
		SetFollowTarget(null);
		ContinueDailyRoutine();
		onPlayerRespawn.Invoke();
	}

	public virtual void StartFollowing()
	{
		if (!followerManager.Contains(base.gameObject))
		{
			bodyMovement.SetPlayerInteraction(BodyMovement.PlayerInteraction.Follow);
			SetFollowTarget(followerManager.GetNextFollowTarget());
			followerManager.AddFollower(base.gameObject);
		}
	}

	public virtual void StopFollowing()
	{
		followerManager.RemoveFollower(base.gameObject);
		bodyMovement.FollowTarget = null;
	}

	protected void SetFollowTarget(Transform target)
	{
		bodyMovement.FollowTarget = target;
	}
}
