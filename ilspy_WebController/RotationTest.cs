using UnityEngine;

public class RotationTest : MonoBehaviour
{
	[SerializeField]
	private float rotationSpeed = 90f;

	private void Update()
	{
		base.transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f);
	}
}
