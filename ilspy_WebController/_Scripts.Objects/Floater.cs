using UnityEngine;

namespace _Scripts.Objects;

public class Floater : MonoBehaviour
{
	private float waterHeight;

	private bool isUnderWater;

	private Collider waterTrigger;

	public bool IsUnderWater => isUnderWater;

	private void Update()
	{
		if (isUnderWater)
		{
			waterHeight = waterTrigger.bounds.max.y;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Water"))
		{
			waterHeight = base.transform.position.y;
			waterTrigger = other;
			isUnderWater = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Water"))
		{
			isUnderWater = false;
			waterTrigger = null;
		}
	}

	public float GetDifference()
	{
		float num = Mathf.Min(base.transform.position.y - waterHeight, 0f);
		return num * num * 0.01f;
	}
}
