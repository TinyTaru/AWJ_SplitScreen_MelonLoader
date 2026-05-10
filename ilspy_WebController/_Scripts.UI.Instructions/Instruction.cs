using TMPro;
using UnityEngine;

namespace _Scripts.UI.Instructions;

public class Instruction : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI iconText;

	[SerializeField]
	private TextMeshProUGUI textText;

	public void UpdateTextFields(InstructionSO instructionSo)
	{
		iconText.text = instructionSo.icon;
		textText.text = instructionSo.text;
	}
}
