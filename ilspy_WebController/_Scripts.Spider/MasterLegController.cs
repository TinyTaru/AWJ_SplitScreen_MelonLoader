using System.Collections.Generic;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Spider;

public class MasterLegController : MonoBehaviour
{
	public enum LegState
	{
		Walking,
		Jumping,
		Landing,
		Ball
	}

	[SerializeField]
	private float tipHeight;

	[SerializeField]
	private float newTargetDistance;

	[SerializeField]
	private float stepDistance;

	[Header("Ray Casting")]
	[SerializeField]
	private float sphereCastRadius;

	[SerializeField]
	private float rayCastOriginUpOffset;

	[SerializeField]
	private float rayCastLength;

	[Header("Body Movement")]
	[SerializeField]
	private BodyMovement bodyMovement;

	[Header("Animation")]
	[SerializeField]
	private float stepTime;

	[SerializeField]
	private float stepHeight;

	private List<LegController> legs;

	private LegState state;

	private float landingTimer;

	private bool legsMovedSinceLastReset;

	public LayerMask WhatIsGround => bodyMovement.WhatIsGround;

	public float TipHeight => tipHeight;

	public float NewTargetDistance => newTargetDistance;

	public float StepDistance => stepDistance;

	public float SphereCastRadius => sphereCastRadius;

	public float RayCastOriginUpOffset => rayCastOriginUpOffset;

	public float RayCastLength => rayCastLength;

	public BodyMovement Movement => bodyMovement;

	public float StepTime
	{
		get
		{
			if (!bodyMovement.IsSprinting)
			{
				return stepTime;
			}
			return stepTime / 2f;
		}
	}

	public float StepHeight => stepHeight;

	public List<LegController> Legs => legs ?? (legs = new List<LegController>());

	public LegState State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
		}
	}

	public bool LegsMovedSinceLastReset
	{
		get
		{
			return legsMovedSinceLastReset;
		}
		set
		{
			legsMovedSinceLastReset = value;
		}
	}

	public float LandingTimer
	{
		get
		{
			return landingTimer;
		}
		set
		{
			landingTimer = value;
		}
	}

	private void Start()
	{
		state = LegState.Walking;
	}

	private void Update()
	{
		if (Singleton<GameController>.Instance.State == GameController.GameState.Paused)
		{
			return;
		}
		switch (state)
		{
		case LegState.Jumping:
			landingTimer = Movement.LandingTime;
			break;
		case LegState.Landing:
			landingTimer -= Time.deltaTime;
			if (landingTimer < 0f)
			{
				state = LegState.Walking;
			}
			break;
		case LegState.Walking:
			break;
		}
	}

	public void ResetAllLegs(bool instant = false)
	{
		if (!legsMovedSinceLastReset && !instant)
		{
			return;
		}
		legsMovedSinceLastReset = false;
		foreach (LegController leg in Legs)
		{
			leg.ResetLeg(instant);
		}
	}
}
