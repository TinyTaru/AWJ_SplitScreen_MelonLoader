using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;
using _Scripts.Objects;
using _Scripts.Singletons;

namespace _Scripts.LivingRoom;

public class Fish : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private float swimForce = 100f;

	[SerializeField]
	private float maxVelocityPC;

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
	private float uprightAngleThreshold = 10f;

	[SerializeField]
	private float minJumpForce = 50f;

	[SerializeField]
	private float maxJumpForce = 100f;

	[SerializeField]
	private float jumpCooldown = 10f;

	[SerializeField]
	private float maxJumpAngle = 10f;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private float minJumpTorque = 10f;

	[SerializeField]
	private float maxJumpTorque = 50f;

	[SerializeField]
	private float minFlopForce = 10f;

	[SerializeField]
	private float maxFlopForce = 20f;

	[SerializeField]
	private int flopAmountBeforeJump = 5;

	[SerializeField]
	private float flopCooldown = 3f;

	[SerializeField]
	private ParticleSystem airBubbleParticleSystem;

	[SerializeField]
	private float eatDuration;

	[SerializeField]
	private Transform mouthTransform;

	[SerializeField]
	private float foodSizeModifier = 1.5f;

	[SerializeField]
	private float foodSpeedModifier = 1.5f;

	[SerializeField]
	private float foodJumpModifier = 1.5f;

	[SerializeField]
	private FishFoodType[] likedFishFood;

	[SerializeField]
	private float animationDistance = 200f;

	[SerializeField]
	private Vector2 randomColorSaturationRange;

	[SerializeField]
	private Vector2 randomColorValueRange;

	[SerializeField]
	private SkinnedMeshRenderer skinnedMeshRenderer;

	[SerializeField]
	private float randomColorSaturationModifier = 0.75f;

	[SerializeField]
	private float randomColorValueModifier = 1.1f;

	[SerializeField]
	private Material defaultMaterial;

	[SerializeField]
	private Material rgbMaterial;

	[SerializeField]
	private Color defaultColor;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onEatingStartedEvent;

	[SerializeField]
	private UnityEvent onEatingFinishedEvent;

	[SerializeField]
	private UnityEvent onFlopPerformedEvent;

	[SerializeField]
	private UnityEvent onJumpPerformedEvent;

	private float maxVelocity;

	private bool isGrounded;

	private Rigidbody rb;

	private float currentRotationSpeed;

	private float targetRotationSpeed;

	private float rotationTimer;

	private bool isInWater;

	private float jumpTimer;

	private int flopCounter;

	private bool isEating;

	private FishFood currentFishFood;

	private float jumpMultiplier;

	private bool canFly;

	private Transform playerTransform;

	private float squareAnimationDistance;

	private bool isAnimating;

	private int underWaterCounter;

	private SpawnableObject spawnableObject;

	private static MaterialPropertyBlock mpb;

	private static readonly int color1Id = Shader.PropertyToID("_Color1");

	private static readonly int color2Id = Shader.PropertyToID("_Color2");

	private static readonly int color3Id = Shader.PropertyToID("_Color3");

	private static readonly int timestampId = Shader.PropertyToID("_Timestamp");

	public bool IsEating => isEating;

	public event Action OnFishFed;

	private void Awake()
	{
		spawnableObject = GetComponent<SpawnableObject>();
		if (spawnableObject != null)
		{
			spawnableObject.OnSetupDone += SpawnableObject_OnSetupDone;
		}
	}

	private void Start()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		rb = GetComponent<MovableObject>().GetRigidbody();
		playerTransform = Singleton<GameController>.Instance.Player.transform;
		squareAnimationDistance = animationDistance * animationDistance;
		rotationTimer = changeInterval;
		maxVelocity = maxVelocityPC;
		CalculateNewTargetRotationSpeed();
		jumpTimer = jumpCooldown;
		flopCounter = 0;
		jumpMultiplier = 1f;
		canFly = false;
		SetColors();
		currentFishFood = null;
		isAnimating = true;
		animator.enabled = true;
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
			if (isInWater || canFly)
			{
				HandleUprightOrientation();
			}
		}
		else if (!isInWater && !canFly)
		{
			jumpTimer -= Time.deltaTime;
			if (jumpTimer <= 0f)
			{
				if (flopCounter < flopAmountBeforeJump)
				{
					flopCounter++;
					Flop();
					jumpTimer = flopCooldown;
				}
				else
				{
					flopCounter = 0;
					Jump();
					jumpTimer = jumpCooldown;
				}
			}
		}
		else
		{
			HandleRotation();
			HandleUprightOrientation();
		}
	}

	private void StopEating()
	{
		isEating = false;
		switch (currentFishFood.FoodType)
		{
		case FishFoodType.Default:
			skinnedMeshRenderer.sharedMaterial = defaultMaterial;
			SetColors();
			base.transform.localScale = Vector3.one;
			maxVelocity = maxVelocityPC;
			jumpMultiplier = 1f;
			canFly = false;
			if (!isInWater)
			{
				ExitWater();
			}
			squareAnimationDistance = animationDistance * animationDistance;
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
			maxVelocity *= foodSpeedModifier;
			break;
		case FishFoodType.Jump:
			jumpMultiplier *= foodJumpModifier;
			break;
		case FishFoodType.Fly:
			canFly = true;
			if (!isInWater)
			{
				rb.useGravity = false;
			}
			break;
		case FishFoodType.Color:
			skinnedMeshRenderer.sharedMaterial = defaultMaterial;
			SetColors(isRandom: true);
			break;
		case FishFoodType.RGB:
			skinnedMeshRenderer.sharedMaterial = rgbMaterial;
			skinnedMeshRenderer.GetPropertyBlock(mpb);
			mpb.SetFloat(timestampId, Time.timeSinceLevelLoad);
			skinnedMeshRenderer.SetPropertyBlock(mpb);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		currentFishFood = null;
		this.OnFishFed?.Invoke();
		onEatingFinishedEvent?.Invoke();
	}

	private void SetColors(bool isRandom = false)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		Color.RGBToHSV(defaultColor, out var H, out var S, out var V);
		if (isRandom)
		{
			H = UnityEngine.Random.Range(0f, 1f);
			S = UnityEngine.Random.Range(randomColorSaturationRange.x, randomColorSaturationRange.y);
			V = UnityEngine.Random.Range(randomColorValueRange.x, randomColorValueRange.y);
		}
		float num = S * randomColorSaturationModifier;
		float s = num * randomColorSaturationModifier;
		float num2 = V * randomColorValueModifier;
		float v = num2 * randomColorValueModifier;
		Color value = Color.HSVToRGB(H, S, V, hdr: false);
		Color value2 = Color.HSVToRGB(H, num, num2, hdr: false);
		Color value3 = Color.HSVToRGB(H, s, v, hdr: false);
		skinnedMeshRenderer.GetPropertyBlock(mpb);
		mpb.SetColor(color1Id, value);
		mpb.SetColor(color2Id, value2);
		mpb.SetColor(color3Id, value3);
		skinnedMeshRenderer.SetPropertyBlock(mpb);
	}

	private void Flop()
	{
		Vector3 vector = Quaternion.Euler(UnityEngine.Random.Range(0.1f, maxJumpAngle), UnityEngine.Random.Range(0f, 360f), 0f) * Vector3.up;
		Vector3 force = vector * UnityEngine.Random.Range(minFlopForce, maxFlopForce) * jumpMultiplier;
		rb.AddForce(force, ForceMode.Impulse);
		Vector3 vector2 = Vector3.Cross(Vector3.up, vector) * UnityEngine.Random.Range(minJumpTorque, maxJumpTorque);
		rb.AddTorque(vector2, ForceMode.Impulse);
		onFlopPerformedEvent?.Invoke();
	}

	private void Jump()
	{
		Vector3 vector = Quaternion.Euler(UnityEngine.Random.Range(0.1f, maxJumpAngle), UnityEngine.Random.Range(0f, 360f), 0f) * Vector3.up;
		Vector3 force = vector * UnityEngine.Random.Range(minJumpForce, maxJumpForce) * jumpMultiplier;
		rb.AddForce(force, ForceMode.Impulse);
		Vector3 vector2 = Vector3.Cross(Vector3.up, vector) * UnityEngine.Random.Range(minJumpTorque, maxJumpTorque);
		rb.AddTorque(vector2, ForceMode.Impulse);
		onJumpPerformedEvent?.Invoke();
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
				zero += base.transform.forward * torque;
			}
			else
			{
				zero -= base.transform.forward * torque;
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
				zero += base.transform.right * torque;
			}
			else
			{
				zero -= base.transform.right * torque;
			}
		}
		rb.AddTorque(zero, ForceMode.Force);
	}

	private void HandleRotation()
	{
		if (rb.linearVelocity.sqrMagnitude < maxVelocity * maxVelocity)
		{
			rb.AddForce(base.transform.forward * swimForce, ForceMode.Force);
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
		}
	}

	public void ExitWater()
	{
		underWaterCounter--;
		underWaterCounter = Mathf.Max(0, underWaterCounter);
		if (underWaterCounter != 0)
		{
			return;
		}
		isInWater = false;
		if (!canFly)
		{
			if (rb != null)
			{
				rb.useGravity = true;
			}
			jumpTimer = jumpCooldown;
			flopCounter = 0;
		}
		airBubbleParticleSystem.Stop();
	}

	private void SpawnableObject_OnSetupDone()
	{
		UnityEngine.Object.FindFirstObjectByType<FeedFishQuest>().AddFishToList(this);
		spawnableObject.OnSetupDone -= SpawnableObject_OnSetupDone;
	}
}
