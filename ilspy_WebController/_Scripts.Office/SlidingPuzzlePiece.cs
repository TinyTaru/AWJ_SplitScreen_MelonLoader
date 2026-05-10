using System;
using UnityEngine;

namespace _Scripts.Office;

public class SlidingPuzzlePiece : MonoBehaviour
{
	private enum Number
	{
		None,
		Debug,
		Real
	}

	[SerializeField]
	private GameObject realNumber;

	[SerializeField]
	private GameObject debugNumber;

	[SerializeField]
	private Number numberToDisplay;

	[SerializeField]
	private GameObject slidingCollider;

	[SerializeField]
	private GameObject brokenCollider;

	private void Start()
	{
		ShowNumber();
	}

	private void ShowNumber()
	{
		switch (numberToDisplay)
		{
		case Number.None:
			realNumber.SetActive(value: false);
			debugNumber.SetActive(value: false);
			break;
		case Number.Debug:
			realNumber.SetActive(value: false);
			debugNumber.SetActive(value: true);
			break;
		case Number.Real:
			realNumber.SetActive(value: true);
			debugNumber.SetActive(value: false);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void ActivateBrokenCollider()
	{
		slidingCollider.SetActive(value: false);
		brokenCollider.SetActive(value: true);
	}
}
