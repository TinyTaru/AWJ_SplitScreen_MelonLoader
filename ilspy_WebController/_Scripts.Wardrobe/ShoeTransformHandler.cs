using UnityEngine;

namespace _Scripts.Wardrobe;

public class ShoeTransformHandler : MonoBehaviour
{
	[SerializeField]
	private Transform targets;

	public void SetSize(float value)
	{
		targets.localScale = new Vector3(value, 1f, value);
	}

	public void SetRotation(float value)
	{
		targets.localEulerAngles = new Vector3(0f, value, 0f);
	}
}
