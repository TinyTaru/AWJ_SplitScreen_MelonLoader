using UnityEngine;

[ExecuteInEditMode]
public class CameraMagnetProperty : MonoBehaviour
{
	[Range(0.1f, 50f)]
	public float MagnetStrength = 5f;

	[Range(0.1f, 50f)]
	public float Proximity = 5f;

	public Transform ProximityVisualization;

	[HideInInspector]
	public Transform myTransform;

	private void Start()
	{
		myTransform = base.transform;
	}

	private void Update()
	{
		if (ProximityVisualization != null)
		{
			ProximityVisualization.localScale = new Vector3(Proximity * 2f, Proximity * 2f, 1f);
		}
	}
}
