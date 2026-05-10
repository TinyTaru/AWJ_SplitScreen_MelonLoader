using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Spider;

public class AutoDisable : MonoBehaviour
{
	[SerializeField]
	private float disableDistanceSteam = 200f;

	[SerializeField]
	private float disableDistanceMobile = 100f;

	[SerializeField]
	private GameObject[] gameObjectsToDisable;

	[SerializeField]
	private Behaviour[] componentsToDisable;

	private Transform player;

	private bool isEnabled;

	private float disableDistance;

	private void Start()
	{
		disableDistance = disableDistanceSteam;
		player = Singleton<GameController>.Instance.Player.transform;
		EnableComponents();
	}

	private void Update()
	{
		float sqrMagnitude = (player.position - base.transform.position).sqrMagnitude;
		if (isEnabled)
		{
			if (sqrMagnitude > disableDistance * disableDistance)
			{
				isEnabled = false;
				EnableComponents();
			}
		}
		else if (sqrMagnitude < disableDistance * disableDistance)
		{
			isEnabled = true;
			EnableComponents();
		}
	}

	private void EnableComponents()
	{
		Behaviour[] array = componentsToDisable;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = isEnabled;
		}
		GameObject[] array2 = gameObjectsToDisable;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].SetActive(isEnabled);
		}
	}
}
