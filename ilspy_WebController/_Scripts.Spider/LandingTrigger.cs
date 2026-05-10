using UnityEngine;
using _Scripts.Extensions;

namespace _Scripts.Spider;

public class LandingTrigger : MonoBehaviour
{
	private BodyMovement bodyMovement;

	private void Start()
	{
		bodyMovement = GetComponentInParent<BodyMovement>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (bodyMovement.State == BodyMovement.MovementState.Jumping && !(bodyMovement.JumpTimer > 0f) && bodyMovement.WhatIsGround.Contains(other.gameObject.layer))
		{
			bodyMovement.PerformLanding();
		}
	}
}
