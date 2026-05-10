using UnityEngine;

namespace _Scripts.KidsRoom;

[RequireComponent(typeof(LineRenderer))]
public class CannonTrajectory : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private StarShooter starShooter;

	[SerializeField]
	private Rigidbody cannonballPrefab;

	[SerializeField]
	private Transform shootPoint;

	[Header("Parameters")]
	[SerializeField]
	private int resolution = 30;

	[SerializeField]
	private float timeStep = 0.05f;

	private LineRenderer lineRenderer;

	private Vector3 gravityVector;

	private float mass;

	private float drag;

	private void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		gravityVector = Physics.gravity;
		mass = cannonballPrefab.mass;
		drag = cannonballPrefab.linearDamping;
	}

	private void Update()
	{
		if (lineRenderer.enabled)
		{
			SimulateTrajectory();
		}
	}

	private void SimulateTrajectory()
	{
		Vector3 initialVelocity = shootPoint.forward.normalized * starShooter.ShootForce / mass;
		lineRenderer.positionCount = resolution;
		for (int i = 0; i < resolution; i++)
		{
			float time = (float)i * timeStep;
			Vector3 vector = CalculatePosition(initialVelocity, time);
			lineRenderer.SetPosition(i, shootPoint.position + vector);
		}
	}

	private Vector3 CalculatePosition(Vector3 initialVelocity, float time)
	{
		if (Mathf.Approximately(drag, 0f))
		{
			return initialVelocity * time + gravityVector * (0.5f * (time * time));
		}
		float num = Mathf.Exp((0f - drag) * time);
		float num2 = 1f / drag;
		return (initialVelocity - gravityVector * num2) * ((1f - num) * num2) + gravityVector * (time * num2);
	}

	public void ShowLineRenderer(bool show)
	{
		lineRenderer.enabled = show;
	}
}
