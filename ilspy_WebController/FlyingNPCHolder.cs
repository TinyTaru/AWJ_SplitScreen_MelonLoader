using System.Collections;
using DG.Tweening;
using Dreamteck.Splines;
using FMODUnity;
using UnityEngine;
using _Scripts.NPCs;

public class FlyingNPCHolder : MonoBehaviour, INPC
{
	[SerializeField]
	private bool canFollow;

	[SerializeField]
	private bool followInfinitely;

	[SerializeField]
	private FlyingPoint point;

	[SerializeField]
	private Transform followTarget;

	[SerializeField]
	private Rigidbody pointRB;

	[SerializeField]
	private float moveSpeed = 10f;

	[Header("Flying Specific")]
	[SerializeField]
	private FlyingNPCType type;

	[SerializeField]
	private SplineFollower splineFollower;

	[SerializeField]
	private float waitingDuration = 1f;

	[SerializeField]
	private FlyingNPCMode mode = FlyingNPCMode.IDLE;

	[SerializeField]
	private WalkingNPC.InteractionType interactionType;

	[Tooltip("Sets the distance the NPC keeps to the target")]
	[SerializeField]
	private float distanceToTarget = 10f;

	[SerializeField]
	private float allowedDistanceBeforeStuck = 10f;

	[Tooltip("Sets the Amplitude how extreme the hovering should be.")]
	[SerializeField]
	[Range(0f, 10f)]
	private float hoveringAmplitude = 1f;

	[Tooltip("Sets the Duration of one hovering direction")]
	[SerializeField]
	[Range(0f, 10f)]
	private float hoverDuration = 0.5f;

	[SerializeField]
	private StudioEventEmitter waitForMeSound;

	private float initialYPosition;

	private bool isMovingUp;

	private Vector2 moveVector;

	private bool canSayWaitForMe;

	private float waitForMeActivateDistance;

	private float minFollowDistance;

	private Quaternion lastRotation;

	private float angleDifference;

	private Coroutine coroutine;

	private SplinePoint[] points;

	private Transform waypointTransform;

	private SplineComputer splineComputer;

	private bool isReturningToPath;

	private Transform lookAtTarget;

	private Sequence tweener;

	private float hoverMaxPosition => initialYPosition + hoveringAmplitude * 2f;

	public bool IsFollowing => followTarget != null;

	public Transform PointLookAtTarget
	{
		get
		{
			if (!(lookAtTarget == null))
			{
				return lookAtTarget;
			}
			return followTarget;
		}
	}

	public Transform PointTransform => point.transform;

	public FlyingNPCMode Mode => mode;

	public FlyingNPCType Type => type;

	private void Start()
	{
		switch (type)
		{
		case FlyingNPCType.FLYING:
			splineComputer = splineFollower.spline;
			points = splineComputer.GetPoints();
			waypointTransform = splineFollower.transform;
			if (followTarget == null)
			{
				SetFollowTarget(splineFollower.gameObject.transform);
			}
			break;
		case FlyingNPCType.STATIC:
			initialYPosition = base.transform.position.y;
			tweener = DOTween.Sequence();
			tweener.Append(pointRB.transform.DOMoveY(hoverMaxPosition, hoverDuration)).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
			FlyOnPoint();
			break;
		}
	}

	private void FixedUpdate()
	{
		if (mode != 0)
		{
			_ = 1;
		}
		else if (followTarget != null && !isReturningToPath)
		{
			float num = Vector3.Distance(followTarget.position, point.transform.position);
			if (num > distanceToTarget)
			{
				Vector3 position = Vector3.MoveTowards(point.transform.position, followTarget.transform.position, moveSpeed * Time.deltaTime);
				pointRB.MovePosition(position);
				if (num > allowedDistanceBeforeStuck)
				{
					point.TriggerStuckCheck();
				}
				else
				{
					point.TryReset();
				}
			}
			if (num > waitForMeActivateDistance && canSayWaitForMe)
			{
				canSayWaitForMe = false;
				waitForMeSound.Play();
			}
			else if (num < waitForMeActivateDistance)
			{
				canSayWaitForMe = true;
			}
		}
		else if (waypointTransform != null && isReturningToPath)
		{
			if (Vector3.Distance(waypointTransform.position, point.transform.position) > 3f)
			{
				Vector3 position2 = Vector3.MoveTowards(point.transform.position, waypointTransform.position, moveSpeed * Time.deltaTime);
				pointRB.MovePosition(position2);
			}
			else
			{
				isReturningToPath = false;
				ResumeSplineFollower();
			}
		}
		else
		{
			pointRB.linearVelocity = Vector3.zero;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		pointRB.linearVelocity = Vector3.zero;
		if (IgnoreCollision(other) || !canFollow)
		{
			return;
		}
		switch (type)
		{
		case FlyingNPCType.STATIC:
			if (followTarget == null)
			{
				StartFollowing(other.transform);
			}
			break;
		case FlyingNPCType.FLYING:
			if (interactionType == WalkingNPC.InteractionType.DIALOGUE)
			{
				StartLookingAtPlayer(other.transform);
				followTarget = null;
				StopSplineFollower();
				break;
			}
			if (coroutine != null)
			{
				StopCoroutine(coroutine);
				coroutine = null;
			}
			StartFollowing(other.transform);
			StopSplineFollower();
			break;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		pointRB.linearVelocity = Vector3.zero;
		if (!IgnoreCollision(other) && !followInfinitely)
		{
			if (interactionType == WalkingNPC.InteractionType.DIALOGUE)
			{
				StopLookingAtPlayer();
				StartFollowing(splineFollower.transform);
				ResumeSplineFollower();
			}
			else if (canFollow)
			{
				StopFollowing(other);
			}
		}
	}

	private void StartLookingAtPlayer(Transform other)
	{
		lookAtTarget = other;
	}

	private void StopLookingAtPlayer()
	{
		lookAtTarget = null;
	}

	private bool IgnoreCollision(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			return other.name == "SpiderInteraction";
		}
		return true;
	}

	private void StartFollowing(Transform transform)
	{
		if (type == FlyingNPCType.STATIC)
		{
			tweener.Pause();
		}
		SetFollowTarget(transform);
	}

	private void StopFollowing(Collider other)
	{
		followTarget = null;
		switch (type)
		{
		case FlyingNPCType.FLYING:
			coroutine = StartCoroutine(HandleExit(other));
			break;
		case FlyingNPCType.STATIC:
			FlyOnPoint();
			break;
		}
	}

	private IEnumerator HandleExit(Collider other)
	{
		yield return new WaitForSeconds(waitingDuration);
		FlyingNPCType flyingNPCType = type;
		if (flyingNPCType != 0 && flyingNPCType == FlyingNPCType.FLYING)
		{
			WalkingNPC.InteractionType interactionType = this.interactionType;
			if (interactionType != 0 && interactionType == WalkingNPC.InteractionType.DIALOGUE)
			{
				lookAtTarget = null;
			}
			FindClosestPointOnPath();
		}
		coroutine = null;
	}

	private void FindClosestPointOnPath()
	{
		float num = float.PositiveInfinity;
		SplinePoint? splinePoint = null;
		int? num2 = null;
		for (int i = 0; i < points.Length; i++)
		{
			SplinePoint value = points[i];
			float num3 = Vector3.Distance(value.position, base.transform.position);
			if (num3 < num)
			{
				num = num3;
				splinePoint = value;
				num2 = i;
			}
		}
		if (splinePoint.HasValue && num2.HasValue)
		{
			double pointPercent = splineComputer.GetPointPercent(num2.Value);
			splineFollower.SetPercent(pointPercent);
			splineFollower.gameObject.transform.position = splinePoint.Value.position;
			SetFollowTarget(waypointTransform);
			isReturningToPath = true;
		}
	}

	private void SetFollowTarget(Transform transform)
	{
		followTarget = transform;
	}

	public void StopSplineFollower()
	{
		splineFollower.follow = false;
	}

	public void ResumeSplineFollower()
	{
		splineFollower.follow = true;
	}

	private void FlyOnPoint()
	{
		tweener.Play();
	}
}
