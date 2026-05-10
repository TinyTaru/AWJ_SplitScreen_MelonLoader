using System;
using System.Linq;
using UnityEngine;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.Spider;

public class LegController : MonoBehaviour
{
	[SerializeField]
	private bool debug;

	[SerializeField]
	private Transform target;

	[SerializeField]
	private Transform targetLocal;

	[SerializeField]
	private Transform targetJump;

	[SerializeField]
	private Transform center;

	[SerializeField]
	private Vector3 startingOffset;

	[SerializeField]
	private LegController[] opposingLegs;

	[SerializeField]
	private Side side;

	private MasterLegController masterLegController;

	private BodyMovement bodyMovement;

	private Vector3 oldTarget;

	private float lerp;

	private Vector3 rayCastPosition;

	private float scale;

	private bool resetLeg;

	private bool instantReset;

	public bool IsAnimating => lerp < 1f;

	private void Awake()
	{
		masterLegController = base.gameObject.GetComponentInParent<MasterLegController>();
	}

	private void Start()
	{
		resetLeg = false;
		masterLegController.Legs.Add(this);
		bodyMovement = masterLegController.Movement;
		UpdateScale(bodyMovement.transform.lossyScale.x);
		lerp = 1f;
		rayCastPosition = base.transform.position;
		if (Physics.Raycast(base.transform.position + base.transform.forward * startingOffset.z, -base.transform.up, out var hitInfo, 10f, masterLegController.WhatIsGround))
		{
			targetLocal.position = hitInfo.point + hitInfo.normal * masterLegController.TipHeight * scale;
			Vector3 forward = Vector3.Cross(base.transform.right, hitInfo.normal);
			targetLocal.rotation = Quaternion.LookRotation(forward, hitInfo.normal);
			targetLocal.parent = hitInfo.transform;
			oldTarget = targetLocal.position;
		}
		bodyMovement.OnScaleChanged += BodyMovement_OnScaleChanged;
	}

	private void BodyMovement_OnScaleChanged(object sender, BodyMovement.OnScaleChangedEventArgs e)
	{
		UpdateScale(e.scale);
	}

	private void Update()
	{
		if (bodyMovement.IsWardrobeSpider)
		{
			PerformWalking();
		}
		switch (masterLegController.State)
		{
		case MasterLegController.LegState.Walking:
			PerformLegAnimation(Time.deltaTime);
			break;
		case MasterLegController.LegState.Jumping:
			PerformJumpAnimation();
			break;
		case MasterLegController.LegState.Landing:
			PerformLegAnimation(Time.deltaTime);
			break;
		}
	}

	private void FixedUpdate()
	{
		if (targetLocal.parent == base.transform.parent)
		{
			ResetLeg(instant: true);
		}
		switch (masterLegController.State)
		{
		case MasterLegController.LegState.Walking:
			PerformWalking();
			break;
		case MasterLegController.LegState.Landing:
			PerformWalking();
			resetLeg = true;
			break;
		case MasterLegController.LegState.Jumping:
			break;
		}
	}

	private void UpdateScale(float newScale)
	{
		scale = newScale;
	}

	private void PerformWalking()
	{
		if (!(lerp < 1f))
		{
			CheckLegPosition();
		}
	}

	private void PerformLegAnimation(float deltaTime)
	{
		if (lerp < 1f)
		{
			resetLeg = false;
			target.rotation = targetLocal.rotation;
			Vector3 position = Vector3.Lerp(oldTarget, targetLocal.position, lerp);
			position += target.up * Mathf.Sin(lerp * MathF.PI) * masterLegController.StepHeight * scale;
			target.position = position;
			lerp += deltaTime / (masterLegController.StepTime * scale);
		}
		else
		{
			target.position = targetLocal.position;
			target.rotation = targetLocal.rotation;
		}
	}

	private void CheckLegPosition()
	{
		if (resetLeg)
		{
			rayCastPosition = base.transform.position + base.transform.forward * startingOffset.z + base.transform.up * masterLegController.RayCastOriginUpOffset;
		}
		else
		{
			rayCastPosition = base.transform.position + scale * base.transform.up * masterLegController.RayCastOriginUpOffset + scale * bodyMovement.transform.forward * bodyMovement.MoveVector.y * masterLegController.StepDistance;
		}
		Ray ray = new Ray(rayCastPosition, -base.transform.up);
		if (!opposingLegs.All((LegController leg) => !leg.IsAnimating))
		{
			return;
		}
		if (resetLeg)
		{
			if (CheckLegSphereCast(ray, out var hit) || instantReset)
			{
				StartLegAnimation(hit);
			}
		}
		else if (bodyMovement.State != BodyMovement.MovementState.Emote && (center.position - targetLocal.position).magnitude > masterLegController.NewTargetDistance * scale)
		{
			if (debug)
			{
				Debug.DrawLine(ray.origin, ray.origin + ray.direction * masterLegController.RayCastLength, Color.red, 1f);
			}
			if (CheckLegSphereCast(ray, out var hit2))
			{
				StartLegAnimation(hit2);
			}
		}
	}

	private void StartLegAnimation(RaycastHit hit)
	{
		if (hit.transform == null)
		{
			return;
		}
		oldTarget = targetLocal.position;
		targetLocal.position = hit.point + hit.normal * masterLegController.TipHeight * scale;
		if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Web"))
		{
			targetLocal.position -= hit.normal * (Singleton<WebController>.Instance.WebColliderRadius - 0.1f);
		}
		Vector3 forward = Vector3.Cross(base.transform.right, hit.normal);
		targetLocal.rotation = Quaternion.LookRotation(forward, hit.normal);
		targetLocal.parent = hit.transform;
		if (instantReset)
		{
			lerp = 1f;
			target.position = targetLocal.position;
			target.rotation = targetLocal.rotation;
			resetLeg = false;
			instantReset = false;
		}
		else
		{
			lerp = 0f;
			if (!resetLeg)
			{
				masterLegController.LegsMovedSinceLastReset = true;
			}
		}
	}

	private void PerformJumpAnimation()
	{
		if (targetJump == null || SettingsController.ArachnophobiaMode)
		{
			targetLocal.position = base.transform.position - base.transform.up * 1f * scale;
			oldTarget = targetLocal.position;
			target.position = targetLocal.position;
			target.rotation = base.transform.rotation;
		}
		else
		{
			targetLocal.position = targetJump.position;
			oldTarget = targetLocal.position;
			target.position = targetLocal.position;
			target.rotation = targetJump.rotation;
		}
	}

	private bool CheckLegSphereCast(Ray ray, out RaycastHit hit)
	{
		if (Physics.Raycast(ray, out hit, masterLegController.RayCastLength * scale, masterLegController.WhatIsGround))
		{
			return true;
		}
		for (int i = 1; i <= 4; i++)
		{
			if (Physics.SphereCast(ray, masterLegController.SphereCastRadius * scale * (float)i * 0.25f, out hit, masterLegController.RayCastLength * scale, masterLegController.WhatIsGround))
			{
				return true;
			}
		}
		hit = default(RaycastHit);
		return false;
	}

	public void ResetLeg(bool instant = false)
	{
		instantReset = instant;
		resetLeg = true;
	}
}
