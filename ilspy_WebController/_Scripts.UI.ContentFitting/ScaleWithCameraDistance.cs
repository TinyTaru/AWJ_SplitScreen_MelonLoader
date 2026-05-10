using UnityEngine;
using _Scripts.Singletons;
using _Scripts.Utils;

namespace _Scripts.UI.ContentFitting;

public class ScaleWithCameraDistance : MonoBehaviour
{
	[SerializeField]
	private float scaleFactor;

	[SerializeField]
	private AnimatorUpdateMode updateMode;

	private UnityEngine.Camera mainCamera;

	private float lossyScaleFactor;

	private float screenFactor;

	private void Awake()
	{
		mainCamera = Singleton<CameraController>.Instance.MainCamera;
		lossyScaleFactor = base.transform.lossyScale.x;
		screenFactor = ((Screen.height > Screen.width) ? ((float)Screen.width / 1920f) : 1f);
	}

	private void OnEnable()
	{
		base.transform.localScale = CalculateTargetScale();
	}

	private void Update()
	{
		if (updateMode == AnimatorUpdateMode.Normal)
		{
			Vector3 b = CalculateTargetScale();
			base.transform.localScale = _Scripts.Utils.Utils.ExponentialDecay(base.transform.localScale, b, 10f, Time.deltaTime);
		}
	}

	private void FixedUpdate()
	{
		if (updateMode == AnimatorUpdateMode.Fixed)
		{
			Vector3 b = CalculateTargetScale();
			base.transform.localScale = _Scripts.Utils.Utils.ExponentialDecay(base.transform.localScale, b, 10f, Time.fixedDeltaTime);
		}
	}

	private Vector3 CalculateTargetScale()
	{
		float num = Vector3.Distance(base.transform.position, mainCamera.transform.position) * scaleFactor * lossyScaleFactor * screenFactor;
		return Vector3.one * num;
	}
}
