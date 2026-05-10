using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Objects;

namespace _Scripts.KidsRoom;

public class JigsawPuzzle : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Transform puzzlePiecesContainer;

	[SerializeField]
	private Transform finalPositionsContainer;

	[SerializeField]
	private Transform magneticLockContainer;

	[Header("Parameters")]
	[SerializeField]
	private int rows = 4;

	[SerializeField]
	private int columns = 5;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onPuzzleFinishedEvent;

	private MovableObject[] puzzlePieces;

	private MagneticLock[] magneticLocks;

	private int pieceCounter;

	private void Awake()
	{
		puzzlePieces = puzzlePiecesContainer.GetComponentsInChildren<MovableObject>();
		magneticLocks = magneticLockContainer.GetComponentsInChildren<MagneticLock>();
	}

	private void Start()
	{
		StartCoroutine(SetupCoroutine());
	}

	private IEnumerator SetupCoroutine()
	{
		yield return null;
		for (int i = 0; i < magneticLocks.Length; i++)
		{
			if (i != 0 && i != 4 && i != 15 && i != 19)
			{
				magneticLocks[i].DisableMagneticLock();
			}
		}
	}

	private void MovePuzzlePiecesToFinalPositions()
	{
		puzzlePieces = puzzlePiecesContainer.GetComponentsInChildren<MovableObject>();
		for (int i = 0; i < puzzlePieces.Length; i++)
		{
			puzzlePieces[i].transform.position = finalPositionsContainer.GetChild(i).position;
			puzzlePieces[i].transform.rotation = finalPositionsContainer.GetChild(i).rotation;
		}
	}

	private List<int> GetNeighborIndices(int index)
	{
		List<int> list = new List<int>();
		int num = index / columns;
		int num2 = index % columns;
		if (num2 > 0)
		{
			list.Add(index - 1);
		}
		if (num2 < columns - 1)
		{
			list.Add(index + 1);
		}
		if (num > 0)
		{
			list.Add(index - columns);
		}
		if (num < rows - 1)
		{
			list.Add(index + columns);
		}
		return list;
	}

	public void ActivateNeighboringMagneticLock(MagneticLock magneticLock)
	{
		int index = magneticLocks.ToList().IndexOf(magneticLock);
		foreach (int neighborIndex in GetNeighborIndices(index))
		{
			magneticLocks[neighborIndex].EnableMagneticLock();
		}
		pieceCounter++;
		if (pieceCounter >= columns * rows)
		{
			onPuzzleFinishedEvent?.Invoke();
		}
	}
}
