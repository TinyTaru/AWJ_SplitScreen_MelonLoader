using StylizedWater;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BuoyantObject : MonoBehaviour
{
	private Color red = new Color(0.92f, 0.25f, 0.2f);

	private Color green = new Color(0.2f, 0.92f, 0.51f);

	private Color blue = new Color(0.2f, 0.67f, 0.92f);

	private Color orange = new Color(0.97f, 0.79f, 0.26f);

	private float steepness;

	private float wavelength;

	private float speed;

	private float[] directions = new float[4];

	[Header("Water Object")]
	public StylizedWaterURP water;

	[Header("Buoyancy")]
	[Range(1f, 5f)]
	public float strength = 1f;

	[Range(0.2f, 5f)]
	public float objectDepth = 1f;

	public float velocityDrag = 0.99f;

	public float angularDrag = 0.5f;

	[Header("Effectors")]
	public Transform[] effectors;

	private Rigidbody rb;

	private Vector3[] effectorProjections;

	private void Awake()
	{
		steepness = water.GetWaveSteepness();
		wavelength = water.GetWaveLength();
		speed = water.GetWaveSpeed();
		directions = water.GetWaveDirections();
		rb = GetComponent<Rigidbody>();
		rb.useGravity = false;
		effectorProjections = new Vector3[effectors.Length];
		for (int i = 0; i < effectors.Length; i++)
		{
			effectorProjections[i] = effectors[i].position;
		}
	}

	private void OnDisable()
	{
		rb.useGravity = true;
	}

	private void FixedUpdate()
	{
		int num = effectors.Length;
		for (int i = 0; i < num; i++)
		{
			Vector3 position = effectors[i].position;
			effectorProjections[i] = position;
			effectorProjections[i].y = water.transform.position.y + GerstnerWaveDisplacement.GetWaveDisplacement(position, steepness, wavelength, speed, directions).y;
			rb.AddForceAtPosition(Physics.gravity / num, position, ForceMode.Acceleration);
			float y = effectorProjections[i].y;
			float y2 = position.y;
			if (y2 < y)
			{
				float num2 = Mathf.Clamp01(y - y2) / objectDepth;
				float num3 = Mathf.Abs(Physics.gravity.y) * num2 * strength;
				rb.AddForceAtPosition(Vector3.up * num3, position, ForceMode.Acceleration);
				rb.AddForce(-rb.linearVelocity * velocityDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
				rb.AddTorque(-rb.angularVelocity * angularDrag * Time.fixedDeltaTime, ForceMode.Impulse);
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (effectors == null)
		{
			return;
		}
		for (int i = 0; i < effectors.Length; i++)
		{
			if (!Application.isPlaying && effectors[i] != null)
			{
				Gizmos.color = green;
				Gizmos.DrawSphere(effectors[i].position, 0.06f);
				continue;
			}
			if (effectors[i] == null)
			{
				break;
			}
			if (effectors[i].position.y < effectorProjections[i].y)
			{
				Gizmos.color = red;
			}
			else
			{
				Gizmos.color = green;
			}
			Gizmos.DrawSphere(effectors[i].position, 0.06f);
			Gizmos.color = orange;
			Gizmos.DrawSphere(effectorProjections[i], 0.06f);
			Gizmos.color = blue;
			Gizmos.DrawLine(effectors[i].position, effectorProjections[i]);
		}
	}
}
