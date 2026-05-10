using UnityEngine;
using _Scripts.Spider;

namespace _Scripts.Wardrobe.Hats;

public class ShmoopHat : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private SkinnedMeshRenderer skinnedMeshRenderer;

	[SerializeField]
	private MeshRenderer legMeshRenderer;

	private BodyMovement bodyMovement;

	private Hat hat;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorID = Shader.PropertyToID("_Color");

	private static readonly int burnAmountId = Shader.PropertyToID("_BurnAmount");

	private void Awake()
	{
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
		if (!(bodyMovement == null))
		{
			if (bodyMovement.MoveVector.magnitude > 0.1f)
			{
				animator.SetBool("Run", value: true);
			}
			else
			{
				animator.SetBool("Run", value: false);
			}
		}
	}

	public void SetColor1(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		skinnedMeshRenderer.GetPropertyBlock(mpb, 1);
		mpb.SetColor(colorID, color);
		skinnedMeshRenderer.SetPropertyBlock(mpb, 1);
		if (legMeshRenderer != null)
		{
			legMeshRenderer.GetPropertyBlock(mpb, 0);
			mpb.SetColor(colorID, color);
			legMeshRenderer.SetPropertyBlock(mpb, 0);
		}
	}

	public void SetColor2(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		skinnedMeshRenderer.GetPropertyBlock(mpb, 0);
		mpb.SetColor(colorID, color);
		skinnedMeshRenderer.SetPropertyBlock(mpb, 0);
	}

	public void SetEyeBaseColor(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		skinnedMeshRenderer.GetPropertyBlock(mpb, 3);
		mpb.SetColor(colorID, color);
		skinnedMeshRenderer.SetPropertyBlock(mpb, 3);
	}

	public void SetEyeColor(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		skinnedMeshRenderer.GetPropertyBlock(mpb, 2);
		mpb.SetColor(colorID, color);
		skinnedMeshRenderer.SetPropertyBlock(mpb, 2);
	}

	public void EnableLegs(float value)
	{
		legMeshRenderer.gameObject.SetActive(value > 0f);
	}

	private void Hat_OnBurnAmountChanged(float burnAmount)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		for (int i = 0; i < skinnedMeshRenderer.materials.Length; i++)
		{
			skinnedMeshRenderer.GetPropertyBlock(mpb, i);
			mpb.SetFloat(burnAmountId, burnAmount);
			skinnedMeshRenderer.SetPropertyBlock(mpb, i);
		}
		if (legMeshRenderer != null)
		{
			legMeshRenderer.GetPropertyBlock(mpb, 0);
			mpb.SetFloat(burnAmountId, burnAmount);
			legMeshRenderer.SetPropertyBlock(mpb, 0);
		}
	}
}
