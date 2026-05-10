using System;
using UnityEngine;
using _Scripts.Objects;
using _Scripts.Singletons;

namespace _Scripts.KidsRoom;

[RequireComponent(typeof(MovableObject))]
public class Shmoop : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Transform gfx;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private ShmoopGroundTrigger shmoopGroundTrigger;

	[SerializeField]
	private SkinnedMeshRenderer skinnedMeshRenderer;

	[Header("Parameters")]
	[SerializeField]
	private float runForce;

	[SerializeField]
	private float maxVelocityPC;

	[SerializeField]
	private float maxVelocityMobile;

	[SerializeField]
	private float minRotationSpeed;

	[SerializeField]
	private float maxRotationSpeed;

	[SerializeField]
	private bool randomizeRotationDirection = true;

	[SerializeField]
	private float smoothing = 1f;

	[SerializeField]
	private float changeInterval = 3f;

	[SerializeField]
	private float torque = 100f;

	[SerializeField]
	private float animationDistance = 100f;

	private Rigidbody rb;

	private Transform playerTransform;

	private float maxVelocity;

	private bool isGrounded;

	private float currentRotationSpeed;

	private float targetRotationSpeed;

	private float rotationTimer;

	private bool isAnimating;

	private float squareAnimationDistance;

	private void Start()
	{
		rb = GetComponent<MovableObject>().GetRigidbody();
		rotationTimer = changeInterval;
		playerTransform = Singleton<GameController>.Instance.Player.transform;
		squareAnimationDistance = animationDistance * animationDistance;
		isAnimating = true;
		animator.enabled = true;
		shmoopGroundTrigger.OnGroundEnter += ShmoopGroundTrigger_OnGroundEnter;
		shmoopGroundTrigger.OnGroundExit += ShmoopGroundTrigger_OnGroundExit;
		maxVelocity = maxVelocityPC;
	}

	private void Update()
	{
		float sqrMagnitude = (playerTransform.position - base.transform.position).sqrMagnitude;
		if (isAnimating && sqrMagnitude > squareAnimationDistance)
		{
			isAnimating = false;
			animator.enabled = false;
		}
		else if (!isAnimating && sqrMagnitude < squareAnimationDistance)
		{
			isAnimating = true;
			animator.enabled = true;
		}
	}

	private void FixedUpdate()
	{
		if (isGrounded)
		{
			if (rb.linearVelocity.magnitude < maxVelocity)
			{
				rb.AddForce(base.transform.forward * runForce, ForceMode.Force);
			}
			rb.rotation *= Quaternion.Euler(0f, currentRotationSpeed * Time.fixedDeltaTime, 0f);
			rotationTimer -= Time.fixedDeltaTime;
			if (rotationTimer <= 0f)
			{
				rotationTimer = changeInterval;
				CalculateNewTargetRotationSpeed();
			}
			currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, targetRotationSpeed, smoothing * Time.fixedDeltaTime);
		}
		else
		{
			rb.AddTorque(base.transform.forward * torque, ForceMode.Force);
		}
	}

	private void CalculateNewTargetRotationSpeed()
	{
		float num = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		if (!randomizeRotationDirection)
		{
			num = 1f;
		}
		targetRotationSpeed = num * UnityEngine.Random.Range(minRotationSpeed, maxRotationSpeed);
	}

	public void ChangeMaterials(Material materialDark, Material materialLight)
	{
		Material[] sharedMaterials = skinnedMeshRenderer.sharedMaterials;
		sharedMaterials[0] = materialDark;
		sharedMaterials[1] = materialLight;
		skinnedMeshRenderer.sharedMaterials = sharedMaterials;
	}

	private void ShmoopGroundTrigger_OnGroundExit(object sender, EventArgs e)
	{
		isGrounded = false;
	}

	private void ShmoopGroundTrigger_OnGroundEnter(object sender, EventArgs e)
	{
		rb.angularVelocity = Vector3.zero;
		isGrounded = true;
	}
}
