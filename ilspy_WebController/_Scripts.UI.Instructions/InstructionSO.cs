using UnityEngine;

namespace _Scripts.UI.Instructions;

[CreateAssetMenu(menuName = "FTG/New Instruction", fileName = "New Instruction", order = 0)]
public class InstructionSO : ScriptableObject
{
	public string icon;

	public string text;
}
