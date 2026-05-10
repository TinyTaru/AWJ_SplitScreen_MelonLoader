using System.Collections;
using Dreamteck.Splines;
using UnityEngine;
using _Scripts.NPCs;
using _Scripts.Spider;

public class WalkingNPC : NPC
{
	public enum InteractionType
	{
		NONE,
		DIALOGUE
	}

	[SerializeField]
	private SplineFollower splineFollower;

	[SerializeField]
	private InteractionType interactionType;

	[SerializeField]
	private float waitingDuration = 1f;

	private SplinePoint[] points;

	private Transform waypointTransform;

	private Vector3 followerPosition;

	private bool isReturningToPath;

	private SplineComputer splineComputer;

	private Coroutine coroutine;

	protected override void Start()
	{
		base.Start();
		SetupComponents();
	}

	protected override void Update()
	{
		base.Update();
		if (isReturningToPath && Vector3.Distance(base.transform.position, followerPosition) < 3f)
		{
			isReturningToPath = false;
			ResumeSplineFollower();
		}
	}

	protected override void OnTriggerEnter(Collider other)
	{
		if (!IgnoreCollision(other))
		{
			if (coroutine != null)
			{
				StopCoroutine(coroutine);
				coroutine = null;
			}
			if (interactionType == InteractionType.DIALOGUE)
			{
				StartLookingAtPlayer();
			}
			else
			{
				base.OnTriggerEnter(other);
			}
			StopSplineFollower();
		}
	}

	protected override void OnTriggerExit(Collider other)
	{
		if (!IgnoreCollision(other) && !followInfinitely && coroutine == null)
		{
			coroutine = StartCoroutine(HandleExit(other));
		}
	}

	private void SetupComponents()
	{
		splineComputer = splineFollower.spline;
		points = splineComputer.GetPoints();
		waypointTransform = splineFollower.transform;
		SetFollowTarget(waypointTransform);
	}

	private IEnumerator HandleExit(Collider other)
	{
		yield return new WaitForSeconds(waitingDuration);
		if (interactionType == InteractionType.DIALOGUE)
		{
			ContinueDailyRoutine();
		}
		else
		{
			base.OnTriggerExit(other);
			FindClosestPointOnPath();
		}
		coroutine = null;
	}

	public override void ContinueDailyRoutine()
	{
		bodyMovement.SetPlayerInteraction(BodyMovement.PlayerInteraction.Follow);
		ResumeSplineFollower();
	}

	public void StopSplineFollower()
	{
		splineFollower.follow = false;
	}

	private void ResumeSplineFollower()
	{
		splineFollower.follow = true;
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
			followerPosition = waypointTransform.position;
			isReturningToPath = true;
		}
	}

	public void UpdateInteractionType(InteractionType type)
	{
		interactionType = type;
	}

	public void PauseRoaming(float seconds)
	{
		StopSplineFollower();
		StartCoroutine(Pause(seconds));
	}

	private IEnumerator Pause(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		ResumeSplineFollower();
	}
}
