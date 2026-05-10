using UnityEngine;

namespace _Scripts.Wardrobe.Eyes;

public class EyeCustomization : MonoBehaviour
{
	[SerializeField]
	private bool isArachnophobia;

	[Space(10f)]
	[SerializeField]
	private Transform originLeft;

	[SerializeField]
	private Transform originRight;

	[SerializeField]
	private Transform sizeLeft;

	[SerializeField]
	private Transform sizeRight;

	[Space(10f)]
	[SerializeField]
	private float defaultDistance = 0.25f;

	[Space(10f)]
	[SerializeField]
	private float defaultAngle = 14f;

	private const float defaultForwardArachnophobia = 0.5f;

	private const float positionFactorArachnophobia = 0.5f;

	private const float offsetFactorArachnophobia = 0.5f;

	public void SetSize(float value)
	{
		if (!isArachnophobia)
		{
			originLeft.localScale = Vector3.one * value;
			originRight.localScale = Vector3.one * value;
		}
		else
		{
			sizeLeft.localScale = Vector3.one * value;
			sizeRight.localScale = Vector3.one * value;
		}
	}

	public void SetDistance(float value)
	{
		if (!isArachnophobia)
		{
			Vector3 localPosition = originLeft.localPosition;
			localPosition.x = (0f - defaultDistance) * value;
			originLeft.localPosition = localPosition;
			Vector3 localPosition2 = originRight.localPosition;
			localPosition2.x = defaultDistance * value;
			originRight.localPosition = localPosition2;
		}
		else
		{
			float y = (0f - defaultAngle) * value;
			originLeft.localRotation = Quaternion.Euler(0f, y, 0f);
			float y2 = defaultAngle * value;
			originRight.localRotation = Quaternion.Euler(0f, y2, 0f);
		}
	}

	public void EnableLeftEye(float value)
	{
		originRight.gameObject.SetActive(value > 0.5f);
	}

	public void EnableRightEye(float value)
	{
		originLeft.gameObject.SetActive(value > 0.5f);
	}

	public void SetPosition(float value)
	{
		if (!isArachnophobia)
		{
			Vector3 localPosition = originLeft.localPosition;
			localPosition.z = value;
			originLeft.localPosition = localPosition;
			Vector3 localPosition2 = originRight.localPosition;
			localPosition2.z = value;
			originRight.localPosition = localPosition2;
		}
		else
		{
			Vector3 localPosition3 = sizeLeft.localPosition;
			localPosition3.z = 0.5f + value * 0.5f;
			sizeLeft.localPosition = localPosition3;
			Vector3 localPosition4 = sizeRight.localPosition;
			localPosition4.z = 0.5f + value * 0.5f;
			sizeRight.localPosition = localPosition4;
		}
	}

	public void SetOffset(float value)
	{
		if (!isArachnophobia)
		{
			Vector3 localPosition = originLeft.localPosition;
			localPosition.y = value;
			originLeft.localPosition = localPosition;
			Vector3 localPosition2 = originRight.localPosition;
			localPosition2.y = value;
			originRight.localPosition = localPosition2;
		}
		else
		{
			Vector3 localPosition3 = sizeLeft.localPosition;
			localPosition3.y = value * 0.5f;
			sizeLeft.localPosition = localPosition3;
			Vector3 localPosition4 = sizeRight.localPosition;
			localPosition4.y = value * 0.5f;
			sizeRight.localPosition = localPosition4;
		}
	}
}
