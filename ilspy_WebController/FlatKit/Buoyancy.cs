using UnityEngine;

namespace FlatKit;

public class Buoyancy : MonoBehaviour
{
	[Tooltip("The object that contains a Water material.")]
	public Transform water;

	[Space]
	[Tooltip("Range of probing wave height for buoyancy rotation.")]
	public float size = 1f;

	[Tooltip("Max height of buoyancy going up and down.")]
	public float amplitude = 1f;

	[Space]
	[Tooltip("Optionally provide a separate material to get the wave parameters.")]
	public Material overrideWaterMaterial;

	private Material _material;

	private float _speed;

	private float _amplitude;

	private float _frequency;

	private float _direction;

	private Vector3 _originalPosition;

	private Quaternion _originalRotation;

	private void Start()
	{
		Renderer component = water.GetComponent<Renderer>();
		_material = ((overrideWaterMaterial != null) ? overrideWaterMaterial : component.sharedMaterial);
		_speed = _material.GetFloat("_WaveSpeed");
		_amplitude = _material.GetFloat("_WaveAmplitude");
		_frequency = _material.GetFloat("_WaveFrequency");
		_direction = _material.GetFloat("_WaveDirection");
		Transform transform = base.transform;
		_originalPosition = transform.position;
		_originalRotation = transform.rotation;
	}

	private void Update()
	{
		Vector3 position = base.transform.position;
		Vector3 positionOS = water.InverseTransformPoint(position);
		position.y = GetHeightOS(positionOS) + _originalPosition.y;
		base.transform.position = position;
		Vector3 normalWS = GetNormalWS(positionOS);
		base.transform.rotation = Quaternion.FromToRotation(Vector3.up, normalWS) * _originalRotation;
	}

	private Vector2 GradientNoiseDir(Vector2 p)
	{
		p = new Vector2(p.x % 289f, p.y % 289f);
		float num = (34f * p.x + 1f) * p.x % 289f + p.y;
		num = (34f * num + 1f) * num % 289f;
		num = num / 41f % 1f * 2f - 1f;
		return new Vector2(num - Mathf.Floor(num + 0.5f), Mathf.Abs(num) - 0.5f).normalized;
	}

	private float GradientNoise(Vector2 p)
	{
		Vector2 vector = new Vector2(Mathf.Floor(p.x), Mathf.Floor(p.y));
		Vector2 vector2 = new Vector2(p.x % 1f, p.y % 1f);
		float a = Vector3.Dot(GradientNoiseDir(vector), vector2);
		float b = Vector3.Dot(GradientNoiseDir(vector + Vector2.up), vector2 - Vector2.up);
		float a2 = Vector3.Dot(GradientNoiseDir(vector + Vector2.right), vector2 - Vector2.right);
		float b2 = Vector3.Dot(GradientNoiseDir(vector + Vector2.one), vector2 - Vector2.one);
		vector2 = vector2 * vector2 * vector2 * (vector2 * (vector2 * 6f - Vector2.one * 15f) + Vector2.one * 10f);
		return Mathf.Lerp(Mathf.Lerp(a, b, vector2.y), Mathf.Lerp(a2, b2, vector2.y), vector2.x);
	}

	private Vector3 GetNormalWS(Vector3 positionOS)
	{
		Vector3 vector = positionOS + Vector3.forward * size;
		vector.y = GetHeightOS(vector);
		Vector3 vector2 = positionOS + Vector3.right * size;
		vector2.y = GetHeightOS(vector);
		Vector3 normalized = Vector3.Cross(vector - positionOS, vector2 - positionOS).normalized;
		return water.TransformDirection(normalized);
	}

	private float SineWave(Vector3 positionOS, float offset)
	{
		float num = Time.timeSinceLevelLoad * 2f;
		float num2 = Mathf.Sin(offset + num * _speed + (positionOS.x * Mathf.Sin(offset + _direction) + positionOS.z * Mathf.Cos(offset + _direction)) * _frequency);
		if (_material.IsKeywordEnabled("_WAVEMODE_POINTY"))
		{
			num2 = 1f - Mathf.Abs(num2);
		}
		return num2 * _amplitude;
	}

	private float GetHeightOS(Vector3 positionOS)
	{
		float num = SineWave(positionOS, 0f);
		if (_material.IsKeywordEnabled("_WAVEMODE_GRID"))
		{
			num *= SineWave(positionOS, 1.57f);
		}
		return num * amplitude;
	}
}
