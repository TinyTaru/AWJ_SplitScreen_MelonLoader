using TMPro;
using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class KeyboardCapHat : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro keyboardCapText;

	private static readonly string[] keyText = new string[9] { "Esc", "Ctrl", "Alt", "Del", "F4", "F", "?", "!", "Any" };

	public void SetKeyboardCapText(Color color)
	{
		keyboardCapText.color = color;
	}

	public void SetKeyText(float value)
	{
		int num = Mathf.RoundToInt(value - 1f);
		keyboardCapText.text = keyText[num];
	}
}
