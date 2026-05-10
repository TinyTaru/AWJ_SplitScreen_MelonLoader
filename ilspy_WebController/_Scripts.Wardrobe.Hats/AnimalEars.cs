using UnityEngine;

namespace _Scripts.Wardrobe.Hats;

public class AnimalEars : MonoBehaviour
{
	[Header("Left")]
	[SerializeField]
	private Transform originLeft;

	[SerializeField]
	private Transform rotationLeft;

	[Header("Right")]
	[SerializeField]
	private Transform originRight;

	[SerializeField]
	private Transform rotationRight;

	public void EnableEarLeft(float value)
	{
		originRight.gameObject.SetActive(value > 0f);
	}

	public void EnableEarRight(float value)
	{
		originLeft.gameObject.SetActive(value > 0f);
	}

	public void SetSizeLeft(float value)
	{
		originRight.localScale = Vector3.one * value;
	}

	public void SetSizeRight(float value)
	{
		originLeft.localScale = Vector3.one * value;
	}

	public void SetPosition(float value)
	{
		Vector3 localPosition = originLeft.localPosition;
		localPosition.z = value;
		originLeft.localPosition = localPosition;
		Vector3 localPosition2 = originRight.localPosition;
		localPosition2.z = value;
		originRight.localPosition = localPosition2;
	}

	public void SetOffset(float value)
	{
		Vector3 localPosition = rotationLeft.localPosition;
		localPosition.y = value;
		rotationLeft.localPosition = localPosition;
		Vector3 localPosition2 = rotationRight.localPosition;
		localPosition2.y = value;
		rotationRight.localPosition = localPosition2;
	}

	public void SetRotation(float value)
	{
		rotationLeft.localRotation = Quaternion.Euler(0f, 0f - value, 0f);
		rotationRight.localRotation = Quaternion.Euler(0f, value, 0f);
	}
}
