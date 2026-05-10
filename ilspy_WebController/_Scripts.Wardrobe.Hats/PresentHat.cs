using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class PresentHat : MonoBehaviour
{
	[SerializeField]
	private GameObject lid;

	public void EnableLid(float value)
	{
		lid.SetActive(value > 0f);
	}
}
