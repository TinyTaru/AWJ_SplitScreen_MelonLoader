using UnityEngine;

namespace Dustyroom;

public class FloatingMotion : MonoBehaviour
{
	public float verticalAmplitude = 1f;

	public float horizontalAmplitude;

	public bool startAtRandomOffset = true;

	[Space]
	public float speed = 1f;

	[Space]
	[Tooltip("In seconds")]
	public float startDelay;

	[Space]
	public bool worldSpace;

	private Vector3 _initialPosition;

	private float _offsetH;

	private float _offsetV;

	private bool _isMoving;

	private void Start()
	{
		Invoke("Initialize", startDelay);
	}

	private void Initialize()
	{
		_initialPosition = (worldSpace ? base.transform.position : base.transform.localPosition);
		if (startAtRandomOffset)
		{
			_offsetH = Random.value * 1000f;
			_offsetV = Random.value * 1000f;
		}
		_isMoving = true;
	}

	private void Update()
	{
		if (_isMoving)
		{
			Vector3 vector = new Vector3(Mathf.Sin(Time.timeSinceLevelLoad * speed * 0.5f + _offsetV + 100f), 0f, Mathf.Cos(Time.timeSinceLevelLoad * speed + _offsetV + 100f));
			Vector3 vector2 = Vector3.up * (Mathf.Sin(Time.timeSinceLevelLoad * speed + _offsetH) * verticalAmplitude) + vector * (Mathf.Sin(Time.timeSinceLevelLoad * speed + _offsetV) * horizontalAmplitude);
			Vector3 vector3 = _initialPosition + vector2 * Time.timeScale;
			if (worldSpace)
			{
				base.transform.position = vector3;
			}
			else
			{
				base.transform.localPosition = vector3;
			}
		}
	}
}
