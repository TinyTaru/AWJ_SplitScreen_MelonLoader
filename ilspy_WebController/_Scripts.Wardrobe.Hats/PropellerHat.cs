using UnityEngine;
using _Scripts.Spider;

namespace _Scripts.Wardrobe.Hats;

public class PropellerHat : MonoBehaviour
{
	[SerializeField]
	private Transform propellerTransform;

	[SerializeField]
	private MeshRenderer[] propellerMeshRenderers;

	[SerializeField]
	private float speedFactor;

	private BodyMovement bodyMovement;

	private Hat hat;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorID = Shader.PropertyToID("_Color");

	private static readonly int burnAmountId = Shader.PropertyToID("_BurnAmount");

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		hat = GetComponentInParent<Hat>();
	}

	private void Start()
	{
		bodyMovement = GetComponentInParent<BodyMovement>();
	}

	private void OnEnable()
	{
		hat.BurnAmountChanged += Hat_OnBurnAmountChanged;
	}

	private void OnDisable()
	{
		hat.BurnAmountChanged -= Hat_OnBurnAmountChanged;
	}

	private void Update()
	{
		if (!(propellerTransform == null))
		{
			Vector3 eulerAngles = propellerTransform.localRotation.eulerAngles;
			if (bodyMovement != null)
			{
				eulerAngles.y += bodyMovement.Rb.linearVelocity.magnitude * speedFactor * Time.deltaTime;
			}
			else
			{
				eulerAngles.y += 10f * speedFactor * Time.unscaledDeltaTime;
			}
			propellerTransform.localRotation = Quaternion.Euler(eulerAngles);
		}
	}

	public void SetPropellerColor(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		MeshRenderer[] array = propellerMeshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (!(meshRenderer == null))
			{
				meshRenderer.GetPropertyBlock(mpb);
				mpb.SetColor(colorID, color);
				meshRenderer.SetPropertyBlock(mpb);
			}
		}
	}

	public void SetPropellerSize(float value)
	{
		propellerTransform.localScale = Vector3.one * value;
	}

	private void Hat_OnBurnAmountChanged(float burnAmount)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		MeshRenderer[] array = propellerMeshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (!(meshRenderer == null))
			{
				meshRenderer.GetPropertyBlock(mpb);
				mpb.SetFloat(burnAmountId, burnAmount);
				meshRenderer.SetPropertyBlock(mpb);
			}
		}
	}
}
