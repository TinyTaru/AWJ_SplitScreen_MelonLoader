using MPUIKIT;
using UnityEngine;

namespace _Scripts.UI.Animations;

public class BorderGradient : MonoBehaviour
{
	[Header("MPImage")]
	[SerializeField]
	private MPImage border;

	[SerializeField]
	private float gradientRotationSpeed;

	[SerializeField]
	private bool enabledOnStart;

	private void OnEnable()
	{
		if (enabledOnStart)
		{
			EnableGradient();
		}
	}

	private void Update()
	{
		GradientEffect gradientEffect = border.GradientEffect;
		gradientEffect.Rotation -= gradientRotationSpeed * Time.unscaledDeltaTime;
		border.GradientEffect = gradientEffect;
	}

	private void OnDisable()
	{
		DisableGradient();
	}

	public void EnableGradient()
	{
		GradientEffect gradientEffect = border.GradientEffect;
		gradientEffect.Enabled = true;
		border.GradientEffect = gradientEffect;
	}

	public void DisableGradient()
	{
		GradientEffect gradientEffect = border.GradientEffect;
		gradientEffect.Enabled = false;
		border.GradientEffect = gradientEffect;
	}
}
