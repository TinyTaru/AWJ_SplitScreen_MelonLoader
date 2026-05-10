using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.LivingRoom;

public class SpecialCouch : MonoBehaviour
{
	[SerializeField]
	private Rigidbody doorToAncientTemple;

	[SerializeField]
	private float ancientTempleMoveDuration;

	[SerializeField]
	private Transform endPositionForDoorToAncientTemple;

	[SerializeField]
	private MeshRenderer[] gridCells;

	[SerializeField]
	private Material defaultGridCellMaterial;

	[SerializeField]
	private Material codeInputMaterial;

	[SerializeField]
	private Material correctCodeMaterial;

	[SerializeField]
	private Material wrongCodeMaterial;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onCorrectCodeEvent;

	[SerializeField]
	private UnityEvent onWrongCodeEvent;

	private List<int> codeInput;

	private readonly List<int> correctCode = new List<int>
	{
		0, 2, 3, 7, 13, 19, 18, 12, 11, 10,
		16, 17, 21, 22, 24
	};

	private Coroutine wrongCodeCoroutine;

	private bool puzzleSolved;

	public event Action OnCorrectCode;

	private void Start()
	{
		puzzleSolved = false;
		codeInput = new List<int>();
	}

	private IEnumerator WrongCodeCoroutine()
	{
		MeshRenderer[] array = gridCells;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (meshRenderer.sharedMaterial == codeInputMaterial)
			{
				meshRenderer.sharedMaterial = wrongCodeMaterial;
			}
		}
		yield return new WaitForSeconds(0.5f);
		array = gridCells;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sharedMaterial = defaultGridCellMaterial;
		}
	}

	private void CorrectCodeForAncientTemple()
	{
		Debug.Log("Correct Code for Ancient Temple");
		this.OnCorrectCode?.Invoke();
		onCorrectCodeEvent?.Invoke();
		puzzleSolved = true;
		MeshRenderer[] array = gridCells;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (meshRenderer.sharedMaterial == codeInputMaterial)
			{
				meshRenderer.sharedMaterial = correctCodeMaterial;
			}
		}
		doorToAncientTemple.DOMove(endPositionForDoorToAncientTemple.position, ancientTempleMoveDuration);
	}

	public void ResetCode()
	{
		if (!puzzleSolved && codeInput.Count != 0)
		{
			Debug.Log("Wrong Code");
			codeInput = new List<int>();
			wrongCodeCoroutine = StartCoroutine(WrongCodeCoroutine());
			onWrongCodeEvent?.Invoke();
		}
	}

	public void InputCode(int index)
	{
		if (puzzleSolved || codeInput.Contains(index))
		{
			return;
		}
		if (wrongCodeCoroutine != null)
		{
			StopCoroutine(wrongCodeCoroutine);
			wrongCodeCoroutine = null;
			MeshRenderer[] array = gridCells;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].sharedMaterial = defaultGridCellMaterial;
			}
		}
		codeInput.Add(index);
		gridCells[index].sharedMaterial = codeInputMaterial;
		bool flag = true;
		if (codeInput.Count != correctCode.Count)
		{
			return;
		}
		for (int j = 0; j < correctCode.Count; j++)
		{
			if (codeInput[j] != correctCode[j])
			{
				flag = false;
			}
		}
		if (flag)
		{
			CorrectCodeForAncientTemple();
		}
	}
}
