using UnityEngine;
using _Scripts.Spider;

namespace _Scripts.Wardrobe.Hats;

public class SirenHat : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer rotator;

	[SerializeField]
	private MeshRenderer[] lights;

	[SerializeField]
	private float constantRotationSpeed;

	[SerializeField]
	private float rotationSpeed;

	private BodyMovement bodyMovement;

	private float rotationSpeedMultiplier;

	private static MaterialPropertyBlock mpb;

	private static int colorId = Shader.PropertyToID("_Color");

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

	private void Update()
	{
		if (Time.timeScale < 0.1f)
		{
			Vector3 eulerAngles = rotator.transform.localRotation.eulerAngles;
			eulerAngles.y += constantRotationSpeed * rotationSpeedMultiplier * Time.fixedUnscaledDeltaTime;
			rotator.transform.localRotation = Quaternion.Euler(eulerAngles);
		}
	}

	private void FixedUpdate()
	{
		Vector3 eulerAngles = rotator.transform.localRotation.eulerAngles;
		if (bodyMovement != null)
		{
			eulerAngles.y += (constantRotationSpeed + bodyMovement.Rb.linearVelocity.magnitude * rotationSpeed) * rotationSpeedMultiplier * Time.fixedDeltaTime;
		}
		else
		{
			eulerAngles.y += constantRotationSpeed * rotationSpeedMultiplier * Time.fixedDeltaTime;
		}
		rotator.transform.localRotation = Quaternion.Euler(eulerAngles);
	}

	private void SetLightColor(Color newColor)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		if (lights == null)
		{
			return;
		}
		MeshRenderer[] array = lights;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (!(meshRenderer == null))
			{
				meshRenderer.GetPropertyBlock(mpb);
				mpb.SetColor(colorId, newColor);
				meshRenderer.SetPropertyBlock(mpb);
			}
		}
	}

	private void SetRotatorColor(Color newColor)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		if (rotator != null)
		{
			for (int i = 0; i < rotator.sharedMaterials.Length; i++)
			{
				rotator.GetPropertyBlock(mpb, i);
				mpb.SetColor(colorId, newColor);
				rotator.SetPropertyBlock(mpb, i);
			}
		}
	}

	public void SetRotationSpeed(float value)
	{
		rotationSpeedMultiplier = value;
	}

	public void SetLightColors(Color color)
	{
		SetRotatorColor(color);
		SetLightColor(color);
	}
}
