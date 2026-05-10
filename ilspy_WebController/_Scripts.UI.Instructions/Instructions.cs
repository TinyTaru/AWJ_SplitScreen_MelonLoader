using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.UI.Instructions;

public class Instructions : MonoBehaviour
{
	[SerializeField]
	private List<InstructionSO> instructionList;

	[Header("References")]
	[SerializeField]
	private Instruction instructionPrefab;

	private void UpdateInstructions()
	{
	}
}
