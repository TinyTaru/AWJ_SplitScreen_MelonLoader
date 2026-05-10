using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class ThirdPersonController : MonoBehaviour
{
	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float MoveSpeed = 2f;

	[Tooltip("SetSprint speed of the character in m/s")]
	public float SprintSpeed = 5.335f;

	[Tooltip("How fast the character turns to face movement direction")]
	[Range(0f, 0.3f)]
	public float RotationSmoothTime = 0.12f;

	[Tooltip("Acceleration and deceleration")]
	public float SpeedChangeRate = 10f;

	[Space(10f)]
	[Tooltip("The height the player can jump")]
	public float JumpHeight = 1.2f;

	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	public float Gravity = -15f;

	[Space(10f)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float JumpTimeout = 0.5f;

	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float FallTimeout = 0.15f;

	[Header("Player Grounded")]
	[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
	public bool Grounded = true;

	[Tooltip("Useful for rough ground")]
	public float GroundedOffset = -0.14f;

	[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
	public float GroundedRadius = 0.28f;

	[Tooltip("What layers the character uses as ground")]
	public LayerMask GroundLayers;

	[Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
	public GameObject CinemachineCameraTarget;

	[Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 70f;

	[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -30f;

	[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
	public float CameraAngleOverride;

	[Tooltip("For locking the camera position on all axis")]
	public bool LockCameraPosition;

	private float _cinemachineTargetYaw;

	private float _cinemachineTargetPitch;

	private float _speed;

	private float _animationBlend;

	private float _targetRotation;

	private float _rotationVelocity;

	private float _verticalVelocity;

	private float _terminalVelocity = 53f;

	private float _jumpTimeoutDelta;

	private float _fallTimeoutDelta;

	private int _animIDSpeed;

	private int _animIDGrounded;

	private int _animIDJump;

	private int _animIDFreeFall;

	private int _animIDMotionSpeed;

	private Animator _animator;

	private CharacterController _controller;

	private StarterAssetsInputs _input;

	private GameObject _mainCamera;

	private const float _threshold = 0.01f;

	private bool _hasAnimator;

	private void Awake()
	{
		if (_mainCamera == null)
		{
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		}
	}

	private void Start()
	{
		_hasAnimator = TryGetComponent<Animator>(out _animator);
		_controller = GetComponent<CharacterController>();
		_input = GetComponent<StarterAssetsInputs>();
		AssignAnimationIDs();
		_jumpTimeoutDelta = JumpTimeout;
		_fallTimeoutDelta = FallTimeout;
	}

	private void Update()
	{
		_hasAnimator = TryGetComponent<Animator>(out _animator);
		JumpAndGravity();
		GroundedCheck();
		Move();
	}

	private void LateUpdate()
	{
		CameraRotation();
	}

	private void AssignAnimationIDs()
	{
		_animIDSpeed = Animator.StringToHash("Speed");
		_animIDGrounded = Animator.StringToHash("Grounded");
		_animIDJump = Animator.StringToHash("SetJump");
		_animIDFreeFall = Animator.StringToHash("FreeFall");
		_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
	}

	private void GroundedCheck()
	{
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - GroundedOffset, base.transform.position.z);
		Grounded = Physics.CheckSphere(position, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		if (_hasAnimator)
		{
			_animator.SetBool(_animIDGrounded, Grounded);
		}
	}

	private void CameraRotation()
	{
		if (_input.look.sqrMagnitude >= 0.01f && !LockCameraPosition)
		{
			_cinemachineTargetYaw += _input.look.x * Time.deltaTime;
			_cinemachineTargetPitch += _input.look.y * Time.deltaTime;
		}
		_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
		CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0f);
	}

	private void Move()
	{
		float num = (_input.sprint ? SprintSpeed : MoveSpeed);
		if (_input.move == Vector2.zero)
		{
			num = 0f;
		}
		float magnitude = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
		float num2 = 0.1f;
		float num3 = (_input.analogMovement ? _input.move.magnitude : 1f);
		if (magnitude < num - num2 || magnitude > num + num2)
		{
			_speed = Mathf.Lerp(magnitude, num * num3, Time.deltaTime * SpeedChangeRate);
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = num;
		}
		_animationBlend = Mathf.Lerp(_animationBlend, num, Time.deltaTime * SpeedChangeRate);
		Vector3 normalized = new Vector3(_input.move.x, 0f, _input.move.y).normalized;
		if (_input.move != Vector2.zero)
		{
			_targetRotation = Mathf.Atan2(normalized.x, normalized.z) * 57.29578f + _mainCamera.transform.eulerAngles.y;
			float y = Mathf.SmoothDampAngle(base.transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
			base.transform.rotation = Quaternion.Euler(0f, y, 0f);
		}
		Vector3 vector = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;
		_controller.Move(vector.normalized * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
		if (_hasAnimator)
		{
			_animator.SetFloat(_animIDSpeed, _animationBlend);
			_animator.SetFloat(_animIDMotionSpeed, num3);
		}
	}

	private void JumpAndGravity()
	{
		if (Grounded)
		{
			_fallTimeoutDelta = FallTimeout;
			if (_hasAnimator)
			{
				_animator.SetBool(_animIDJump, value: false);
				_animator.SetBool(_animIDFreeFall, value: false);
			}
			if (_verticalVelocity < 0f)
			{
				_verticalVelocity = -2f;
			}
			if (_input.jump && _jumpTimeoutDelta <= 0f)
			{
				_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				if (_hasAnimator)
				{
					_animator.SetBool(_animIDJump, value: true);
				}
			}
			if (_jumpTimeoutDelta >= 0f)
			{
				_jumpTimeoutDelta -= Time.deltaTime;
			}
		}
		else
		{
			_jumpTimeoutDelta = JumpTimeout;
			if (_fallTimeoutDelta >= 0f)
			{
				_fallTimeoutDelta -= Time.deltaTime;
			}
			else if (_hasAnimator)
			{
				_animator.SetBool(_animIDFreeFall, value: true);
			}
			_input.jump = false;
		}
		if (_verticalVelocity < _terminalVelocity)
		{
			_verticalVelocity += Gravity * Time.deltaTime;
		}
	}

	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f)
		{
			lfAngle += 360f;
		}
		if (lfAngle > 360f)
		{
			lfAngle -= 360f;
		}
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	private void OnDrawGizmosSelected()
	{
		Color color = new Color(0f, 1f, 0f, 0.35f);
		Color color2 = new Color(1f, 0f, 0f, 0.35f);
		if (Grounded)
		{
			Gizmos.color = color;
		}
		else
		{
			Gizmos.color = color2;
		}
		Gizmos.DrawSphere(new Vector3(base.transform.position.x, base.transform.position.y - GroundedOffset, base.transform.position.z), GroundedRadius);
	}
}
