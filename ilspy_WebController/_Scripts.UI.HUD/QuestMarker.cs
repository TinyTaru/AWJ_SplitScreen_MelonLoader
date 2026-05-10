using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UI.HUD;

public class QuestMarker : MonoBehaviour
{
	[SerializeField]
	private float scaleFactor;

	private UnityEngine.Camera mainCamera;

	private float lossyScaleFactor;

	private void Start()
	{
		mainCamera = Singleton<CameraController>.Instance.MainCamera;
		lossyScaleFactor = base.transform.lossyScale.x;
	}

	private void Update()
	{
		PerformScaling();
	}

	private void PerformScaling()
	{
		float num = Mathf.Max(Vector3.Distance(base.transform.position, mainCamera.transform.position) * scaleFactor * lossyScaleFactor, lossyScaleFactor);
		base.transform.localScale = Vector3.one * num;
	}
}
