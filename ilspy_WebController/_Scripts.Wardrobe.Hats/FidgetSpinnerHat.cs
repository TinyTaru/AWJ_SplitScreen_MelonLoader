using UnityEngine;
using _Scripts.Spider;

namespace _Scripts.Wardrobe.Hats;

public class FidgetSpinnerHat : MonoBehaviour
{
	[SerializeField]
	private Transform rotator;

	[SerializeField]
	private MeshRenderer baseMeshRenderer;

	[SerializeField]
	private float maxRotationSpeed;

	[SerializeField]
	private float acceleration;

	[SerializeField]
	private float deceleration;

	private BodyMovement bodyMovement;

	private float rotationSpeed;

	private float maxSpeedMultiplier;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorID = Shader.PropertyToID("_Color");

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void Start()
	{
		bodyMovement = GetComponentInParent<BodyMovement>();
	}

	private void FixedUpdate()
	{
		if (!(rotator == null) || !(bodyMovement == null))
		{
			Vector3 eulerAngles = rotator.localRotation.eulerAngles;
			if (bodyMovement != null)
			{
				rotationSpeed += (bodyMovement.Rb.linearVelocity.magnitude * acceleration * maxSpeedMultiplier - deceleration) * Time.fixedDeltaTime;
				rotationSpeed = Mathf.Clamp(rotationSpeed, 0f, maxRotationSpeed * maxSpeedMultiplier);
				eulerAngles.y += rotationSpeed * Time.fixedDeltaTime;
			}
			rotator.localRotation = Quaternion.Euler(eulerAngles);
		}
	}

	public void SetBaseColor(Color color)
	{
		if (!(baseMeshRenderer == null))
		{
			if (mpb == null)
			{
				mpb = new MaterialPropertyBlock();
			}
			baseMeshRenderer.GetPropertyBlock(mpb);
			mpb.SetColor(colorID, color);
			baseMeshRenderer.SetPropertyBlock(mpb);
		}
	}

	public void SetSpeedMultiplier(float value)
	{
		maxSpeedMultiplier = value;
	}
}
