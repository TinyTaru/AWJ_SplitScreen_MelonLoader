using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;
using _Scripts.KidsRoom;
using _Scripts.Objects;
using _Scripts.Singletons;

namespace _Scripts.LivingRoom;

[RequireComponent(typeof(MovableObject))]
public class Shroomp : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private ShmoopGroundTrigger shmoopGroundTrigger;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private ParticleSystem airBubbleParticleSystem;

	[SerializeField]
	private Transform mouthTransform;

	[SerializeField]
	private FishFoodType[] likedFishFood;

	[SerializeField]
	private SkinnedMeshRenderer skinnedMeshRenderer;

	[SerializeField]
	private Material defaultMaterialTop;

	[SerializeField]
	private Material defaultMaterialBottom;

	[SerializeField]
	private Material rgbMaterialTop;

	[SerializeField]
	private Material rgbMaterialBottom;

	[SerializeField]
	private Color defaultColor;

	[Header("Walking Parameters")]
	[SerializeField]
	private float runForce;

	[SerializeField]
	private float maxGroundVelocityPC = 5f;

	[SerializeField]
	private float maxGroundVelocityMobile = 3f;

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
	private float animationDistance = 200f;

	[Header("Swimming Parameters")]
	[SerializeField]
	private float swimForce;

	[SerializeField]
	private float maxSwimVelocityPC;

	[SerializeField]
	private float minSwimRotationSpeed;

	[SerializeField]
	private float maxSwimRotationSpeed;

	[SerializeField]
	private bool randomizeSwimRotationDirection = true;

	[SerializeField]
	private float swimSmoothing = 1f;

	[SerializeField]
	private float swimChangeInterval = 3f;

	[SerializeField]
	private float maxUprightTorque = 100f;

	[SerializeField]
	private float uprightAngleThreshold = 10f;

	[SerializeField]
	private float eatDuration;

	[SerializeField]
	private float foodSizeModifier = 1.5f;

	[SerializeField]
	private float foodSpeedModifier = 1.5f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onEatingStartedEvent;

	[SerializeField]
	private UnityEvent onEatingFinishedEvent;

	private Rigidbody rb;

	private FishFood currentFishFood;

	private Transform playerTransform;

	private float maxGroundVelocity;

	private bool isGrounded;

	private float currentGroundRotationSpeed;

	private float targetGroundRotationSpeed;

	private float groundRotationTimer;

	private float maxSwimVelocity;

	private float currentSwimRotationSpeed;

	private float targetSwimRotationSpeed;

	private float swimRotationTimer;

	private bool isInWater;

	private bool isEating;

	private float eatTimer;

	private bool canFly;

	private bool isAnimating;

	private float squareAnimationDistance;

	private int underWaterCounter;

	private BurnableObject burnableObject;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorId = Shader.PropertyToID("_Color");

	private static readonly int colorTopId = Shader.PropertyToID("_ShroompColorTop");

	private static readonly int colorBottomId = Shader.PropertyToID("_ShroompColorBottom");

	private static readonly int burnAmountId = Shader.PropertyToID("_BurnAmount");

	private static readonly int timestampId = Shader.PropertyToID("_Timestamp");

	public bool IsEating => isEating;

	private void Start()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		rb = GetComponent<MovableObject>().GetRigidbody();
		playerTransform = Singleton<GameController>.Instance.Player.transform;
		squareAnimationDistance = animationDistance * animationDistance;
		groundRotationTimer = changeInterval;
		shmoopGroundTrigger.OnGroundEnter += ShmoopGroundTrigger_OnGroundEnter;
		shmoopGroundTrigger.OnGroundExit += ShmoopGroundTrigger_OnGroundExit;
		maxGroundVelocity = maxGroundVelocityPC;
		swimRotationTimer = changeInterval;
		maxSwimVelocity = maxSwimVelocityPC;
		CalculateNewTargetGroundRotationSpeed();
		CalculateNewSwimTargetRotationSpeed();
		SetColors();
		canFly = false;
		currentFishFood = null;
		isAnimating = true;
		animator.enabled = true;
		burnableObject = GetComponent<BurnableObject>();
		Debug.Log($"burnableObject: {burnableObject}");
		if (burnableObject != null)
		{
			burnableObject.BurnAmountChanged += BurnableObject_OnBurnAmountChanged;
		}
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
		if (isEating)
		{
			if (isInWater)
			{
				HandleUprightOrientation();
			}
		}
		else if (isInWater)
		{
			HandleRotation();
			HandleUprightOrientation();
		}
		else if (isGrounded)
		{
			if (rb.linearVelocity.magnitude < maxGroundVelocity)
			{
				rb.AddForce(base.transform.forward * runForce, ForceMode.Force);
			}
			rb.rotation *= Quaternion.Euler(0f, currentGroundRotationSpeed * Time.fixedDeltaTime, 0f);
			groundRotationTimer -= Time.fixedDeltaTime;
			if (groundRotationTimer <= 0f)
			{
				groundRotationTimer = changeInterval;
				CalculateNewTargetGroundRotationSpeed();
			}
			currentGroundRotationSpeed = Mathf.Lerp(currentGroundRotationSpeed, targetGroundRotationSpeed, smoothing * Time.fixedDeltaTime);
		}
		else
		{
			rb.AddTorque(base.transform.forward * torque, ForceMode.Force);
		}
	}

	private void OnDestroy()
	{
		if (burnableObject != null)
		{
			burnableObject.BurnAmountChanged -= BurnableObject_OnBurnAmountChanged;
		}
	}

	private void CalculateNewTargetGroundRotationSpeed()
	{
		float num = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		if (!randomizeRotationDirection)
		{
			num = 1f;
		}
		targetGroundRotationSpeed = num * UnityEngine.Random.Range(minRotationSpeed, maxRotationSpeed);
	}

	private void CalculateNewSwimTargetRotationSpeed()
	{
		float num = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		if (!randomizeRotationDirection)
		{
			num = 1f;
		}
		targetSwimRotationSpeed = num * UnityEngine.Random.Range(minRotationSpeed, maxRotationSpeed);
	}

	private void StopEating()
	{
		isEating = false;
		switch (currentFishFood.FoodType)
		{
		case FishFoodType.Default:
			SetColors();
			base.transform.localScale = Vector3.one;
			squareAnimationDistance = animationDistance * animationDistance;
			maxGroundVelocity = maxGroundVelocityPC;
			maxSwimVelocity = maxSwimVelocityPC;
			canFly = false;
			break;
		case FishFoodType.Big:
			base.transform.localScale *= foodSizeModifier;
			squareAnimationDistance *= foodSizeModifier;
			break;
		case FishFoodType.Small:
			base.transform.localScale /= foodSizeModifier;
			squareAnimationDistance /= foodSizeModifier;
			break;
		case FishFoodType.Fast:
			maxGroundVelocity *= foodSpeedModifier;
			maxSwimVelocity *= foodSpeedModifier;
			break;
		case FishFoodType.Fly:
			canFly = true;
			if (!isInWater)
			{
				isInWater = true;
				rb.useGravity = false;
				animator.SetBool("IsInWater", value: true);
			}
			break;
		case FishFoodType.Color:
			SetColors(isRandom: true);
			break;
		case FishFoodType.RGB:
			SetRGBColor();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case FishFoodType.Jump:
			break;
		}
		currentFishFood = null;
		onEatingFinishedEvent?.Invoke();
	}

	private void SetRGBColor()
	{
		Material[] sharedMaterials = skinnedMeshRenderer.sharedMaterials;
		sharedMaterials[0] = rgbMaterialTop;
		sharedMaterials[1] = rgbMaterialBottom;
		skinnedMeshRenderer.sharedMaterials = sharedMaterials;
		skinnedMeshRenderer.GetPropertyBlock(mpb);
		mpb.SetFloat(timestampId, Time.timeSinceLevelLoad);
		skinnedMeshRenderer.SetPropertyBlock(mpb);
	}

	private void SetColors(bool isRandom = false)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		Material[] sharedMaterials = skinnedMeshRenderer.sharedMaterials;
		sharedMaterials[0] = defaultMaterialTop;
		sharedMaterials[1] = defaultMaterialBottom;
		skinnedMeshRenderer.sharedMaterials = sharedMaterials;
		Color.RGBToHSV(defaultColor, out var H, out var S, out var V);
		if (isRandom)
		{
			H = UnityEngine.Random.Range(0f, 1f);
			S = UnityEngine.Random.Range(0.5f, 0.7f);
			V = UnityEngine.Random.Range(0.7f, 0.9f);
		}
		Color value = Color.HSVToRGB(H, S, V, hdr: false);
		Color value2 = Color.HSVToRGB(H, 0.42f, 1f, hdr: false);
		skinnedMeshRenderer.GetPropertyBlock(mpb, 0);
		mpb.SetColor(colorId, value);
		skinnedMeshRenderer.SetPropertyBlock(mpb, 0);
		skinnedMeshRenderer.GetPropertyBlock(mpb, 1);
		mpb.SetColor(colorId, value2);
		skinnedMeshRenderer.SetPropertyBlock(mpb, 1);
	}

	private void HandleUprightOrientation()
	{
		Vector3 zero = Vector3.zero;
		float num = base.transform.rotation.eulerAngles.z % 360f;
		if (num > 180f)
		{
			num -= 360f;
		}
		float num2 = uprightAngleThreshold * uprightAngleThreshold;
		if (num * num > num2)
		{
			if (num < 0f)
			{
				zero += base.transform.forward * maxUprightTorque;
			}
			else
			{
				zero -= base.transform.forward * maxUprightTorque;
			}
		}
		float num3 = base.transform.rotation.eulerAngles.x % 360f;
		if (num3 > 180f)
		{
			num3 -= 360f;
		}
		if (num3 * num3 > num2)
		{
			if (num3 < 0f)
			{
				zero += base.transform.right * maxUprightTorque;
			}
			else
			{
				zero -= base.transform.right * maxUprightTorque;
			}
		}
		rb.AddTorque(zero, ForceMode.Force);
	}

	private void HandleRotation()
	{
		if (rb.linearVelocity.sqrMagnitude < maxSwimVelocity * maxSwimVelocity)
		{
			rb.AddForce(base.transform.forward * swimForce, ForceMode.Force);
		}
		rb.rotation *= Quaternion.Euler(0f, currentSwimRotationSpeed * Time.fixedDeltaTime, 0f);
		swimRotationTimer -= Time.fixedDeltaTime;
		if (swimRotationTimer <= 0f)
		{
			swimRotationTimer = changeInterval;
			CalculateNewSwimTargetRotationSpeed();
		}
		currentSwimRotationSpeed = Mathf.Lerp(currentSwimRotationSpeed, targetSwimRotationSpeed, smoothing * Time.fixedDeltaTime);
	}

	public void StartEating(FishFood fishFood)
	{
		if (!isEating && likedFishFood.Contains(fishFood.FoodType))
		{
			isEating = true;
			currentFishFood = fishFood;
			currentFishFood.StartEating();
			currentFishFood.transform.parent = base.transform;
			DOTween.Sequence().Append(currentFishFood.transform.DOLocalMove(mouthTransform.localPosition, 0.2f)).Join(currentFishFood.transform.DOScale(0f, eatDuration))
				.OnComplete(delegate
				{
					currentFishFood.Destroy();
					StopEating();
				});
			onEatingStartedEvent?.Invoke();
		}
	}

	public void EnterWater()
	{
		underWaterCounter++;
		underWaterCounter = Mathf.Max(0, underWaterCounter);
		if (underWaterCounter > 0)
		{
			isInWater = true;
			if (rb == null)
			{
				rb = GetComponent<MovableObject>().GetRigidbody();
			}
			rb.useGravity = false;
			airBubbleParticleSystem.Play();
			animator.SetBool("IsInWater", value: true);
		}
	}

	public void ExitWater()
	{
		underWaterCounter--;
		underWaterCounter = Mathf.Max(0, underWaterCounter);
		if (underWaterCounter == 0)
		{
			if (!canFly)
			{
				isInWater = false;
				rb.useGravity = true;
				animator.SetBool("IsInWater", value: false);
			}
			airBubbleParticleSystem.Stop();
		}
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

	private void BurnableObject_OnBurnAmountChanged(float burnAmount)
	{
		skinnedMeshRenderer.GetPropertyBlock(mpb, 0);
		mpb.SetFloat(burnAmountId, burnAmount);
		skinnedMeshRenderer.SetPropertyBlock(mpb, 0);
		skinnedMeshRenderer.GetPropertyBlock(mpb, 1);
		mpb.SetFloat(burnAmountId, burnAmount);
		skinnedMeshRenderer.SetPropertyBlock(mpb, 1);
	}
}
