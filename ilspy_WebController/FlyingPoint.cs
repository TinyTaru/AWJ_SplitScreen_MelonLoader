using System;
using System.Linq;
using UnityEngine;

public class FlyingPoint : MonoBehaviour
{
	[SerializeField]
	private float stuckTimer = 2f;

	private SphereCollider collider;

	private float initialTime = 1f;

	private float currentTime;

	private float positionChange;

	private float positionThreshold = 0.01f;

	private bool didReduceColliderRecently;

	private bool needsColliderAdaption;

	private float upcomingRadius = 5f;

	private StuckStateManager manager;

	private int lastPositionSize = 30;

	private float[] lastPositions;

	private int positionCounter;

	private float lastStuckPosition;

	private bool isStuck;

	private void Start()
	{
		lastPositions = new float[lastPositionSize];
		collider = GetComponent<SphereCollider>();
		manager = GetComponent<StuckStateManager>();
	}

	public void TriggerStuckCheck()
	{
		if (positionCounter >= lastPositions.Length)
		{
			float num = lastPositions.First();
			float num2 = lastPositions.Sum() / (float)lastPositions.Length;
			if (Mathf.Abs(num - num2) < positionThreshold)
			{
				isStuck = true;
				lastStuckPosition = num2;
				manager.SetState(StuckStateManager.StuckState.SHRINKING);
			}
			else
			{
				isStuck = false;
				manager.TryGrowing();
			}
			lastPositions = new float[lastPositionSize];
			positionCounter = 0;
		}
		else
		{
			Vector3 vector = new Vector3(base.transform.position.x - MathF.Truncate(base.transform.position.x), base.transform.position.y - MathF.Truncate(base.transform.position.y), base.transform.position.z - MathF.Truncate(base.transform.position.z));
			lastPositions[positionCounter] = vector.magnitude;
			positionCounter++;
		}
	}

	public void TryReset()
	{
		if (collider.radius < 5f)
		{
			manager.TryGrowing();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (isStuck)
		{
			manager.SetShrinking();
		}
	}

	private void OnCollisionExit(Collision other)
	{
		if (!(Mathf.Abs(lastPositions.Sum() / (float)lastPositions.Length - lastStuckPosition) < positionThreshold) && manager.State == StuckStateManager.StuckState.SHRINKING)
		{
			manager.TryGrowing();
		}
	}
}
