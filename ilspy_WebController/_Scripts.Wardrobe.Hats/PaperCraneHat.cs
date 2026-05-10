using System;
using UnityEngine;
using _Scripts.Spider;

namespace _Scripts.Wardrobe.Hats;

public class PaperCraneHat : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private MeshRenderer[] wings;

	[SerializeField]
	private float minSpeedThreshold = 20f;

	[SerializeField]
	private float maxSpeedThreshold = 40f;

	[SerializeField]
	private float speedFactor = 2f;

	private BodyMovement bodyMovement;

	private bool isJumping;

	private Hat hat;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorID = Shader.PropertyToID("_Color");

	private static readonly int burnAmountId = Shader.PropertyToID("_BurnAmount");

	private void Awake()
	{
		hat = GetComponentInParent<Hat>();
	}

	private void OnEnable()
	{
		bodyMovement = GetComponentInParent<BodyMovement>();
		if (bodyMovement != null)
		{
			BodyMovement obj = bodyMovement;
			obj.OnJumpInitialized = (Action)Delegate.Combine(obj.OnJumpInitialized, new Action(BodyMovement_OnJumpInitialized));
			BodyMovement obj2 = bodyMovement;
			obj2.OnLandingPerformed = (Action)Delegate.Combine(obj2.OnLandingPerformed, new Action(BodyMovement_OnLandingPerformed));
		}
		hat.BurnAmountChanged += Hat_OnBurnAmountChanged;
	}

	private void OnDisable()
	{
		if (bodyMovement != null)
		{
			BodyMovement obj = bodyMovement;
			obj.OnJumpInitialized = (Action)Delegate.Remove(obj.OnJumpInitialized, new Action(BodyMovement_OnJumpInitialized));
			BodyMovement obj2 = bodyMovement;
			obj2.OnLandingPerformed = (Action)Delegate.Remove(obj2.OnLandingPerformed, new Action(BodyMovement_OnLandingPerformed));
		}
		hat.BurnAmountChanged -= Hat_OnBurnAmountChanged;
	}

	private void Update()
	{
		if (isJumping)
		{
			float magnitude = bodyMovement.Rb.linearVelocity.magnitude;
			float speed = 1f + Mathf.InverseLerp(minSpeedThreshold, maxSpeedThreshold, magnitude) * speedFactor;
			animator.speed = speed;
		}
	}

	public void SetColor2(Color color)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		MeshRenderer[] array = wings;
		foreach (MeshRenderer obj in array)
		{
			obj.GetPropertyBlock(mpb);
			mpb.SetColor(colorID, color);
			obj.SetPropertyBlock(mpb);
		}
	}

	private void BodyMovement_OnLandingPerformed()
	{
		isJumping = false;
		animator.SetBool("IsJumping", isJumping);
	}

	private void BodyMovement_OnJumpInitialized()
	{
		isJumping = true;
		animator.SetBool("IsJumping", isJumping);
	}

	private void Hat_OnBurnAmountChanged(float burnAmount)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		MeshRenderer[] array = wings;
		foreach (MeshRenderer obj in array)
		{
			obj.GetPropertyBlock(mpb);
			mpb.SetFloat(burnAmountId, burnAmount);
			obj.SetPropertyBlock(mpb);
		}
	}
}
