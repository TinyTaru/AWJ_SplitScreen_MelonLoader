using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;
using _Scripts.Objects;
using _Scripts.Puzzles;

namespace _Scripts.LivingRoom;

public class DvdPlayer : MonoBehaviour
{
	[SerializeField]
	private Rigidbody rigidbodyCase;

	[SerializeField]
	private Transform discHandler;

	[SerializeField]
	private MagneticLock discHandlerMagneticLock;

	[SerializeField]
	private Transform discHandlerPositionInside;

	[SerializeField]
	private Transform discHandlerPositionOutside;

	[SerializeField]
	private float discShootForce = 100f;

	[SerializeField]
	private float discEjectActivateColliderDelay = 0.1f;

	[SerializeField]
	private float insertDiscDuration = 2f;

	[SerializeField]
	private float discHandlerActivateMagneticLockDelay = 2f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onInsertDiscStartedEvent;

	[SerializeField]
	private UnityEvent<MovieType> onInsertDiscFinishedEvent;

	[SerializeField]
	private UnityEvent onEjectDiscStartedEvent;

	[SerializeField]
	private UnityEvent<MovieType> onEjectDiscFinishedEvent;

	private DvdDisc currentDisc;

	private void Start()
	{
		discHandler.position = discHandlerPositionOutside.position;
	}

	private IEnumerator InsertDiscCoroutine()
	{
		MagneticObject connectedObject = discHandlerMagneticLock.ConnectedObject;
		if (!(connectedObject == null))
		{
			DvdDisc disc = connectedObject.GetComponent<DvdDisc>();
			if (!(disc == null))
			{
				onInsertDiscStartedEvent?.Invoke();
				disc.DisableColliders();
				yield return discHandler.DOMove(discHandlerPositionInside.position, insertDiscDuration).WaitForCompletion(returnCustomYieldInstruction: true);
				rigidbodyCase.isKinematic = false;
				currentDisc = disc;
				onInsertDiscFinishedEvent?.Invoke(disc.GetMovieType());
			}
		}
	}

	private IEnumerator EjectDiscCoroutine()
	{
		if (currentDisc == null)
		{
			Debug.Log("No disc to eject!");
			yield break;
		}
		Debug.Log("Start ejecting disc!");
		onEjectDiscStartedEvent?.Invoke();
		discHandlerMagneticLock.DisableMagneticLock();
		currentDisc.Eject(discHandlerPositionInside.forward * discShootForce);
		yield return new WaitForSeconds(discEjectActivateColliderDelay);
		if (currentDisc != null)
		{
			currentDisc.EnableColliders();
		}
		currentDisc = null;
		onEjectDiscFinishedEvent?.Invoke(MovieType.None);
		Debug.Log("Disc successfully ejected!");
		yield return new WaitForSeconds(discHandlerActivateMagneticLockDelay);
		discHandler.position = discHandlerPositionOutside.position;
		discHandlerMagneticLock.EnableMagneticLock();
		Debug.Log("MagneticLock for DiscHandler is active again!");
	}

	public void SetKinematic()
	{
		rigidbodyCase.isKinematic = true;
	}

	public void InsertDisc()
	{
		StartCoroutine(InsertDiscCoroutine());
	}

	public void PressButton()
	{
		Debug.Log("Button pressed!");
		StartCoroutine(EjectDiscCoroutine());
	}

	public void ReleaseButton()
	{
		Debug.Log("Button released!");
	}
}
