using FMODUnity;
using UnityEngine;

namespace _Scripts.Office;

public class ShmoopMoveSound : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter moveSound;

	[SerializeField]
	private float soundInterval = 0.1f;

	private float timer;

	private void Start()
	{
		timer = soundInterval;
		moveSound.Play();
	}

	private void Update()
	{
		timer -= Time.deltaTime;
		if (timer <= 0f)
		{
			moveSound.Play();
			timer = soundInterval;
		}
	}
}
