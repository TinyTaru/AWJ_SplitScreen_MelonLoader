using TMPro;
using UnityEngine;

namespace _Scripts.Office;

public class SafeHint : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro hintText;

	public void SetHint(string hint)
	{
		hintText.text = hint;
	}
}
