using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.LevelSaving;
using _Scripts.Objects;
using _Scripts.Puzzles;

namespace _Scripts.Office;

public class SlidingPuzzle : MonoBehaviour, IInitializable<SlidingPuzzleSaveData>, IHasSaveData<SlidingPuzzleSaveData>
{
	[Header("References")]
	[SerializeField]
	private Rigidbody baseRigidbody;

	[SerializeField]
	private GameObject magneticLocksSliding;

	[SerializeField]
	private GameObject magneticLocksBroken;

	[SerializeField]
	private MagneticLock[] magneticLocks;

	[SerializeField]
	private MovableObject[] puzzlePieces;

	[SerializeField]
	private MagneticLock finalMagneticLock;

	[SerializeField]
	private MagneticObject finalPuzzlePiece;

	[SerializeField]
	private GameObject puzzleSolved;

	[Header("Parameters")]
	[SerializeField]
	private float moveThreshold = 0.2f;

	[SerializeField]
	private float stopThreshold = 0.1f;

	[SerializeField]
	private float breakThreshold = 300f;

	[SerializeField]
	private float breakDelay = 10f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onBreakEvent;

	[SerializeField]
	private UnityEvent onPuzzleFinishedEvent;

	private bool isMoving;

	private bool isSolved;

	private bool isCompleted;

	private bool isBroken;

	private int[] tilePositions;

	private bool[] brokenPiecesInPlace = new bool[15];

	private void Start()
	{
		RandomizePuzzlePieces();
		finalMagneticLock.gameObject.SetActive(value: false);
		finalMagneticLock.SetLockableObjects(new List<MagneticObject> { finalPuzzlePiece });
		baseRigidbody.GetComponent<MovableObject>().OnForwardCollisionCheck += BaseMovableObject_OnForwardCollisionCheck;
	}

	private void FixedUpdate()
	{
		if (isBroken)
		{
			return;
		}
		float magnitude = baseRigidbody.linearVelocity.magnitude;
		if (magnitude > moveThreshold && !isMoving)
		{
			MagneticLock[] array = magneticLocks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DisableMagneticLock();
			}
			isMoving = true;
		}
		else if (magnitude < stopThreshold && isMoving)
		{
			MagneticLock[] array = magneticLocks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].EnableMagneticLock();
			}
			isMoving = false;
		}
		CheckPositions();
	}

	private IEnumerator PermanentlyLockPiecesCoroutine()
	{
		Debug.Log("PermanentlyLockPiecesCoroutine");
		for (int i = 0; i < magneticLocks.Length - 1; i++)
		{
			magneticLocks[i].DisableMagneticLock();
		}
		yield return null;
		for (int j = 0; j < magneticLocks.Length - 1; j++)
		{
			MagneticLock magneticLock = magneticLocks[j];
			Rigidbody rigidbody = puzzlePieces[j].GetRigidbody();
			rigidbody.isKinematic = true;
			rigidbody.position = magneticLock.transform.position;
			rigidbody.rotation = magneticLock.transform.rotation;
		}
		yield return null;
		for (int k = 0; k < magneticLocks.Length - 1; k++)
		{
			Rigidbody rigidbody2 = puzzlePieces[k].GetRigidbody();
			FixedJoint fixedJoint = baseRigidbody.gameObject.AddComponent<FixedJoint>();
			fixedJoint.connectedBody = rigidbody2;
			fixedJoint.breakForce = float.PositiveInfinity;
			fixedJoint.breakTorque = float.PositiveInfinity;
			fixedJoint.enableCollision = false;
			rigidbody2.isKinematic = false;
		}
		for (int l = 0; l < magneticLocks.Length; l++)
		{
			Object.Destroy(magneticLocks[l].gameObject);
		}
		Debug.Log("Destroy(magneticLocksBroken);");
		Object.Destroy(magneticLocksBroken);
		finalMagneticLock.gameObject.SetActive(value: true);
	}

	private IEnumerator ActivateMagneticLocksAfterBreakingCoroutine()
	{
		yield return new WaitForSeconds(breakDelay);
		magneticLocksSliding.SetActive(value: false);
		magneticLocksBroken.SetActive(value: true);
	}

	private IEnumerator PuzzleCompleteCoroutine()
	{
		yield return null;
		yield return null;
		MovableObject[] array = puzzlePieces;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DisableSafely();
		}
		finalPuzzlePiece.GetComponent<MovableObject>().DisableSafely();
		FixedJoint[] componentsInChildren = baseRigidbody.GetComponentsInChildren<FixedJoint>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i]);
		}
		puzzleSolved.SetActive(value: true);
		isCompleted = true;
	}

	private int CountInversions(int index)
	{
		int num = 0;
		int num2 = tilePositions[index];
		for (int i = index + 1; i < 16; i++)
		{
			int num3 = tilePositions[i];
			if (num2 > num3 && num2 != 15)
			{
				num++;
			}
		}
		return num;
	}

	private int SumInversions()
	{
		int num = 0;
		for (int i = 0; i < magneticLocks.Length; i++)
		{
			num += CountInversions(i);
		}
		return num;
	}

	private void RandomizePuzzlePieces()
	{
		tilePositions = new int[16];
		List<int> list = new List<int>
		{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11, 12, 13, 14, 15
		};
		for (int i = 0; i < puzzlePieces.Length; i++)
		{
			MovableObject puzzlePiece = puzzlePieces[i];
			int num = list[Random.Range(0, list.Count)];
			PositionPiece(puzzlePiece, num);
			tilePositions[num] = i;
			list.Remove(num);
		}
		tilePositions[list[0]] = 15;
		int num2 = list[0] / 4;
		int inversions = SumInversions();
		if (!IsSolvable(inversions, num2))
		{
			if (num2 == 0)
			{
				SwapPieces(14, 15);
			}
			else
			{
				SwapPieces(0, 1);
			}
		}
	}

	private bool IsSolvable(int inversions, int emptyRow)
	{
		return (inversions + emptyRow + 1) % 2 == 0;
	}

	private void SwapPieces(int index1, int index2)
	{
		PositionPiece(puzzlePieces[tilePositions[index1]], index2);
		PositionPiece(puzzlePieces[tilePositions[index2]], index1);
		int num = tilePositions[index1];
		tilePositions[index1] = tilePositions[index2];
		tilePositions[index2] = num;
	}

	private void PositionPiece(MovableObject puzzlePiece, int positionIndex)
	{
		float x = positionIndex % 4 * 4 - 6;
		float z = 6 - positionIndex / 4 * 4;
		Vector3 localPosition = new Vector3(x, 1f, z);
		puzzlePiece.transform.localPosition = localPosition;
	}

	private void CheckPositions()
	{
		if (isSolved)
		{
			return;
		}
		for (int i = 0; i < puzzlePieces.Length; i++)
		{
			if (Vector3.Distance(puzzlePieces[i].transform.position, magneticLocks[i].transform.position) > 0.5f)
			{
				return;
			}
		}
		PuzzleFinished();
	}

	private void PuzzleFinished()
	{
		Debug.Log("Sliding Puzzle solved! You are a genius!");
		isSolved = true;
		StartCoroutine(PermanentlyLockPiecesCoroutine());
		onPuzzleFinishedEvent?.Invoke();
	}

	private void Break()
	{
		if (!isSolved && !isBroken)
		{
			Debug.Log("Break");
			isBroken = true;
			MagneticLock[] array = magneticLocks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DisableMagneticLock();
			}
			MovableObject[] array2 = puzzlePieces;
			foreach (MovableObject obj in array2)
			{
				obj.GetComponent<SlidingPuzzlePiece>().ActivateBrokenCollider();
				Object.Destroy(obj.GetComponent<ConfigurableJoint>());
			}
			StartCoroutine(ActivateMagneticLocksAfterBreakingCoroutine());
		}
	}

	public void Initialize(SlidingPuzzleSaveData slidingPuzzleSaveData)
	{
		isSolved = slidingPuzzleSaveData.isSolved;
		isBroken = slidingPuzzleSaveData.isBroken;
		isCompleted = slidingPuzzleSaveData.isCompleted;
		if (isCompleted)
		{
			MovableObject[] array = puzzlePieces;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DisableSafely();
			}
			finalPuzzlePiece.GetComponent<MovableObject>().DisableSafely();
			FixedJoint[] componentsInChildren = baseRigidbody.GetComponentsInChildren<FixedJoint>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i]);
			}
			puzzleSolved.SetActive(value: true);
		}
		else if (isSolved)
		{
			StartCoroutine(PermanentlyLockPiecesCoroutine());
			onPuzzleFinishedEvent?.Invoke();
		}
		else if (isBroken)
		{
			MagneticLock[] array2 = magneticLocks;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].DisableMagneticLock();
			}
			MovableObject[] array = puzzlePieces;
			foreach (MovableObject obj in array)
			{
				obj.GetComponent<SlidingPuzzlePiece>().ActivateBrokenCollider();
				Object.Destroy(obj.GetComponent<ConfigurableJoint>());
			}
			magneticLocksSliding.SetActive(value: false);
			magneticLocksBroken.SetActive(value: true);
		}
	}

	public SlidingPuzzleSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Sliding Puzzle " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		SlidingPuzzleSaveData result = default(SlidingPuzzleSaveData);
		result.id = id;
		result.isSolved = isSolved;
		result.isBroken = isBroken;
		result.isCompleted = isCompleted;
		return result;
	}

	public void PlaceBrokenPiece(int index)
	{
		if (!brokenPiecesInPlace[index])
		{
			brokenPiecesInPlace[index] = true;
			Debug.Log($"Place Broken Piece {index + 1} - total pieces placed: {brokenPiecesInPlace.Count((bool x) => x)}");
			if (brokenPiecesInPlace.Count((bool x) => x) == 15)
			{
				PuzzleFinished();
			}
		}
	}

	public void CompletePuzzle()
	{
		StartCoroutine(PuzzleCompleteCoroutine());
	}

	private void BaseMovableObject_OnForwardCollisionCheck(object sender, MovableObject.OnForwardCollisionCheckEventArgs e)
	{
		float magnitude = e.other.relativeVelocity.magnitude;
		float magnitude2 = e.other.impulse.magnitude;
		if (!isBroken && magnitude2 * magnitude > breakThreshold)
		{
			Break();
		}
	}
}
