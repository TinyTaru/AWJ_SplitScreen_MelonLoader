using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Objects;

public class CleaningArea : MonoBehaviour
{
	[SerializeField]
	private bool startCleaningAtStart;

	[SerializeField]
	private float cleaningPower = 1f;

	private bool isCleaning;

	private List<CleanableObject> cleanableObjects;

	private List<BurnableObject> burnableObjects;

	private float backupCheckTimer;

	private readonly float backupCheckInterval = 1f;

	private bool performBackupCheck;

	private void Awake()
	{
		cleanableObjects = new List<CleanableObject>();
		burnableObjects = new List<BurnableObject>();
	}

	private void Start()
	{
		if (startCleaningAtStart)
		{
			StartCleaning();
		}
	}

	private void Update()
	{
		if (!isCleaning)
		{
			return;
		}
		for (int num = cleanableObjects.Count - 1; num >= 0; num--)
		{
			CleanableObject cleanableObject = cleanableObjects[num];
			if (cleanableObject == null)
			{
				cleanableObjects.Remove(cleanableObject);
			}
			else
			{
				cleanableObject.RemoveDirtAmountPeriodically(cleaningPower * Time.deltaTime);
				if (cleanableObject.IsCleaned)
				{
					cleanableObjects.Remove(cleanableObject);
				}
			}
		}
		foreach (BurnableObject burnableObject in burnableObjects)
		{
			burnableObject.RemoveBurnAmount(cleaningPower * Time.deltaTime);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		CleanableObject componentInParent = other.GetComponentInParent<CleanableObject>();
		BurnableObject componentInParent2 = other.GetComponentInParent<BurnableObject>();
		if (componentInParent != null)
		{
			if (!componentInParent.IsCleaned && !cleanableObjects.Contains(componentInParent))
			{
				cleanableObjects.Add(componentInParent);
			}
		}
		else if (componentInParent2 != null && !burnableObjects.Contains(componentInParent2))
		{
			burnableObjects.Add(componentInParent2);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		CleanableObject componentInParent = other.GetComponentInParent<CleanableObject>();
		BurnableObject componentInParent2 = other.GetComponentInParent<BurnableObject>();
		if (componentInParent != null)
		{
			cleanableObjects.Remove(componentInParent);
		}
		else if (componentInParent2 != null)
		{
			burnableObjects.Remove(componentInParent2);
		}
	}

	private void FixedUpdate()
	{
		backupCheckTimer -= Time.fixedDeltaTime;
		if (backupCheckTimer <= 0f)
		{
			backupCheckTimer = backupCheckInterval;
			StartCoroutine(PerformBackupCheckCoroutine());
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (performBackupCheck)
		{
			OnTriggerEnter(other);
		}
	}

	private IEnumerator PerformBackupCheckCoroutine()
	{
		performBackupCheck = true;
		yield return new WaitForFixedUpdate();
		performBackupCheck = false;
	}

	public void StartCleaning()
	{
		isCleaning = true;
	}

	public void FinishCleaning()
	{
		isCleaning = false;
	}
}
