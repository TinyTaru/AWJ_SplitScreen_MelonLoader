using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class HairBow : MonoBehaviour
{
	[SerializeField]
	private Transform rotationTransform;

	[SerializeField]
	private Transform sizeTransform;

	public void SetRotation(float value)
	{
		rotationTransform.localEulerAngles = new Vector3(0f, value, 0f);
	}

	public void SetSize(float value)
	{
		sizeTransform.localScale = Vector3.one * value;
	}

	public void SetPosition(float value)
	{
		Vector3 localPosition = rotationTransform.localPosition;
		localPosition.z = value;
		rotationTransform.localPosition = localPosition;
	}

	public void SetOffset(float value)
	{
		Vector3 localPosition = rotationTransform.localPosition;
		localPosition.y = value;
		rotationTransform.localPosition = localPosition;
	}
}
