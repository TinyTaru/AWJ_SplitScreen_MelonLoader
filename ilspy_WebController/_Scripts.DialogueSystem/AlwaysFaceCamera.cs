using UnityEngine;

namespace _Scripts.DialogueSystem;

public class AlwaysFaceCamera : MonoBehaviour
{
	[SerializeField]
	private bool yAxisOnly;

	[SerializeField]
	private bool rotate180;

	[SerializeField]
	private AnimatorUpdateMode updateMode;

	private float rotationSpeed = 180f;

	private UnityEngine.Camera mainCamera;

	private void Awake()
	{
		mainCamera = UnityEngine.Camera.main;
	}

	private void OnEnable()
	{
		base.transform.rotation = CalculateTargetRotation();
	}

	private void Update()
	{
		if (updateMode == AnimatorUpdateMode.Normal)
		{
			Quaternion to = CalculateTargetRotation();
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, rotationSpeed * Time.deltaTime);
		}
	}

	private void FixedUpdate()
	{
		if (updateMode == AnimatorUpdateMode.Fixed)
		{
			Quaternion to = CalculateTargetRotation();
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, rotationSpeed * Time.fixedDeltaTime);
		}
	}

	private Quaternion CalculateTargetRotation()
	{
		if (rotate180)
		{
			if (yAxisOnly)
			{
				return Quaternion.Euler(base.transform.rotation.eulerAngles.x, (mainCamera.transform.rotation.eulerAngles + 180f * Vector3.up).y, base.transform.rotation.eulerAngles.z);
			}
			return Quaternion.LookRotation(-mainCamera.transform.forward, mainCamera.transform.up);
		}
		if (yAxisOnly)
		{
			return Quaternion.Euler(base.transform.rotation.eulerAngles.x, mainCamera.transform.rotation.eulerAngles.y, base.transform.rotation.eulerAngles.z);
		}
		return mainCamera.transform.rotation;
	}
}
