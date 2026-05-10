using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FMODUnity;
using UnityEngine;

namespace _Scripts.Puzzles;

[SelectionBase]
public class SlidingDoor : MonoBehaviour
{
	private enum DoorState
	{
		Opening,
		Open,
		Closing,
		Closed
	}

	[SerializeField]
	private Transform door;

	[SerializeField]
	private Vector3 closedPosition;

	[SerializeField]
	private Vector3 openPosition;

	[SerializeField]
	private float speed;

	[SerializeField]
	private bool stayOpen;

	[SerializeField]
	private List<Activator> activatorsTrue;

	[SerializeField]
	private List<Activator> activatorsFalse;

	[SerializeField]
	private List<Activator> activatorsTrueOR;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter doorMoveSound;

	[SerializeField]
	private StudioEventEmitter doorOpenSound;

	[SerializeField]
	private StudioEventEmitter doorClosedSound;

	private DoorState state;

	private bool canMove;

	private Sequence openSequence;

	private Sequence closeSequence;

	private void OnDestroy()
	{
		door.DOKill();
	}

	private void Start()
	{
		canMove = false;
		state = DoorState.Closed;
		door.localPosition = closedPosition;
		foreach (Activator item in activatorsTrue)
		{
			if (item != null)
			{
				item.StateChangedEvent.AddListener(OnActivatorsChanged);
			}
		}
		foreach (Activator item2 in activatorsFalse)
		{
			if (item2 != null)
			{
				item2.StateChangedEvent.AddListener(OnActivatorsChanged);
			}
		}
		foreach (Activator item3 in activatorsTrueOR)
		{
			if (item3 != null)
			{
				item3.StateChangedEvent.AddListener(OnActivatorsChanged);
			}
		}
		StartCoroutine(EnableDoorCoroutine());
	}

	private IEnumerator EnableDoorCoroutine()
	{
		yield return null;
		canMove = true;
	}

	private void OpenDoor()
	{
		if (state != DoorState.Open && state != 0)
		{
			state = DoorState.Opening;
			doorMoveSound.Play();
			door.DOKill();
			door.DOLocalMove(openPosition, speed).SetSpeedBased(isSpeedBased: true).SetEase(Ease.InSine)
				.OnComplete(FinishDoorOpening);
		}
	}

	private void CloseDoor()
	{
		if (state != DoorState.Closed && state != DoorState.Closing && !stayOpen)
		{
			state = DoorState.Closing;
			doorMoveSound.Play();
			door.DOKill();
			door.DOLocalMove(closedPosition, speed).SetSpeedBased(isSpeedBased: true).SetEase(Ease.InSine)
				.OnComplete(FinishDoorClosing);
		}
	}

	private void FinishDoorOpening()
	{
		state = DoorState.Open;
		doorMoveSound.Stop();
		doorOpenSound.Play();
		if (stayOpen)
		{
			canMove = false;
		}
	}

	private void FinishDoorClosing()
	{
		state = DoorState.Closed;
		doorMoveSound.Stop();
		doorClosedSound.Play();
	}

	private void OnActivatorsChanged()
	{
		if (canMove)
		{
			if ((activatorsTrue.All((Activator x) => x.Activated) && activatorsFalse.All((Activator x) => !x.Activated)) || activatorsTrueOR.Count((Activator x) => x.Activated) > 0)
			{
				OpenDoor();
			}
			else
			{
				CloseDoor();
			}
		}
	}
}
