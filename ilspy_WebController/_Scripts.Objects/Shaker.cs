using UnityEngine;

namespace _Scripts.Objects;

public class Shaker : MonoBehaviour
{
	[Range(0f, 1f)]
	[SerializeField]
	private float shakeThreshold = 0.5f;

	[SerializeField]
	private ParticleSystem particles;

	private bool isEmitting;

	private void Start()
	{
		isEmitting = false;
	}

	private void Update()
	{
		bool flag = Vector3.Dot(base.transform.up, Vector3.down) > shakeThreshold;
		if (!isEmitting && flag)
		{
			isEmitting = true;
			particles.Play();
		}
		else if (isEmitting && !flag)
		{
			isEmitting = false;
			particles.Stop();
		}
	}
}
