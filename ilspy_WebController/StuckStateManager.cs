using System.Collections;
using UnityEngine;

public class StuckStateManager : MonoBehaviour
{
	public enum StuckState
	{
		SHRINKING,
		GROWING,
		IDLE
	}

	[SerializeField]
	private SphereCollider collider;

	[SerializeField]
	private float maximumColliderSize = 5f;

	[SerializeField]
	private float minimumColliderSize = 1f;

	[SerializeField]
	private float adaptionSpeed = 1f;

	private StuckState stuckState;

	private Coroutine coroutine;

	public StuckState State => stuckState;

	private void Start()
	{
		stuckState = StuckState.IDLE;
	}

	private void Update()
	{
		switch (stuckState)
		{
		case StuckState.SHRINKING:
			ReduceSize();
			break;
		case StuckState.GROWING:
			IncreaseSize();
			break;
		case StuckState.IDLE:
			break;
		}
	}

	public void TryGrowing()
	{
		if (coroutine == null)
		{
			coroutine = StartCoroutine(DelayAndGrow());
		}
	}

	public void SetShrinking()
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}
		if (stuckState != 0)
		{
			SetState(StuckState.SHRINKING);
		}
	}

	private IEnumerator DelayAndGrow()
	{
		yield return new WaitForSeconds(2f);
		SetState(StuckState.GROWING);
		coroutine = null;
	}

	public void SetState(StuckState newState)
	{
		if (newState != stuckState)
		{
			stuckState = newState;
		}
	}

	private void ReduceSize()
	{
		if (collider.radius > minimumColliderSize)
		{
			collider.radius -= Time.deltaTime * adaptionSpeed;
		}
	}

	private void IncreaseSize()
	{
		if (collider.radius < maximumColliderSize)
		{
			collider.radius += Time.deltaTime * adaptionSpeed;
		}
		else
		{
			SetState(StuckState.IDLE);
		}
	}
}
