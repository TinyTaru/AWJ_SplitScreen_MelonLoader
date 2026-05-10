using Sirenix.Utilities;
using UnityEngine;

namespace _Scripts.Wardrobe;

public class AccessoryTransformHandler : MonoBehaviour
{
	[SerializeField]
	private Transform[] targets;

	public void SetSize(float value)
	{
		targets.ForEach(delegate(Transform x)
		{
			x.localScale = Vector3.one * value;
		});
	}

	public void SetRotation(float value)
	{
		targets.ForEach(delegate(Transform x)
		{
			x.localEulerAngles = new Vector3(0f, 0f, value);
		});
	}

	public void SetPosition(float value)
	{
		Transform[] array = targets;
		foreach (Transform obj in array)
		{
			Vector3 localPosition = obj.localPosition;
			localPosition.z = value;
			obj.localPosition = localPosition;
		}
	}

	public void SetOffset(float value)
	{
		Transform[] array = targets;
		foreach (Transform obj in array)
		{
			Vector3 localPosition = obj.localPosition;
			localPosition.y = value;
			obj.localPosition = localPosition;
		}
	}

	public void Flip(float value)
	{
		targets.ForEach(delegate(Transform x)
		{
			x.localEulerAngles = new Vector3(0f, 0f, value * 180f);
		});
	}
}
