using System;
using UnityEngine;

namespace _Scripts.Objects;

public class DelayedAction : MonoBehaviour
{
	private enum ActionType
	{
		EnableColliders,
		DisableColliders
	}

	[SerializeField]
	private ActionType actionToPerform;

	[SerializeField]
	private float delay;

	[SerializeField]
	[Space(10f)]
	private Collider[] collidersToEnable;

	[SerializeField]
	[Space(10f)]
	private Collider[] collidersToDisable;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void EnableColliders()
	{
		Collider[] array = collidersToEnable;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
	}

	private void DisableColliders()
	{
		Collider[] array = collidersToDisable;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
	}

	public void PerformAction()
	{
		switch (actionToPerform)
		{
		case ActionType.EnableColliders:
			Invoke("EnableColliders", delay);
			break;
		case ActionType.DisableColliders:
			Invoke("DisableColliders", delay);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
