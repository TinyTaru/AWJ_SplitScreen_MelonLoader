using FMODUnity;
using UnityEngine;

namespace _Scripts.Game;

public class CheckpointController : MonoBehaviour
{
	public static CheckpointController Instance;

	[SerializeField]
	private Transform checkpointContainer;

	[SerializeField]
	private StudioEventEmitter checkpointSound;

	private Checkpoint[] checkpoints;

	private Checkpoint activeCheckpoint;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		checkpoints = checkpointContainer.GetComponentsInChildren<Checkpoint>();
		activeCheckpoint = checkpoints[0];
		activeCheckpoint.GetComponent<MeshRenderer>().enabled = false;
	}

	private void Update()
	{
	}

	public void SetActiveCheckpoint(Checkpoint newCheckpoint)
	{
		Checkpoint[] array = checkpoints;
		foreach (Checkpoint checkpoint in array)
		{
			if (checkpoint == newCheckpoint)
			{
				checkpoint.GetComponent<MeshRenderer>().enabled = false;
				if (checkpoint != activeCheckpoint)
				{
					checkpointSound.Play();
				}
				activeCheckpoint = checkpoint;
			}
			else
			{
				checkpoint.GetComponent<MeshRenderer>().enabled = true;
			}
		}
	}
}
