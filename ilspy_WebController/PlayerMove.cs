using System;
using Cinemachine.Utility;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
	public enum ForwardMode
	{
		Camera,
		Player,
		World
	}

	public float Speed;

	public float VelocityDamping;

	public float JumpTime;

	public ForwardMode InputForward;

	public bool RotatePlayer = true;

	public Action SpaceAction;

	public Action EnterAction;

	private Vector3 m_currentVleocity;

	private float m_currentJumpSpeed;

	private float m_restY;

	private void Reset()
	{
		Speed = 5f;
		InputForward = ForwardMode.Camera;
		RotatePlayer = true;
		VelocityDamping = 0.5f;
		m_currentVleocity = Vector3.zero;
		JumpTime = 1f;
		m_currentJumpSpeed = 0f;
	}

	private void OnEnable()
	{
		m_currentJumpSpeed = 0f;
		m_restY = base.transform.position.y;
		SpaceAction = (Action)Delegate.Remove(SpaceAction, new Action(Jump));
		SpaceAction = (Action)Delegate.Combine(SpaceAction, new Action(Jump));
	}

	private void Update()
	{
		Vector3 vector = InputForward switch
		{
			ForwardMode.Camera => Camera.main.transform.forward, 
			ForwardMode.Player => base.transform.forward, 
			_ => Vector3.forward, 
		};
		vector.y = 0f;
		vector = vector.normalized;
		if (!(vector.sqrMagnitude < 0.01f))
		{
			Quaternion quaternion = Quaternion.LookRotation(vector, Vector3.up);
			Vector3 vector2 = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			vector2 = quaternion * vector2;
			float deltaTime = Time.deltaTime;
			Vector3 initial = vector2 * Speed - m_currentVleocity;
			m_currentVleocity += Damper.Damp(initial, VelocityDamping, deltaTime);
			base.transform.position += m_currentVleocity * deltaTime;
			if (RotatePlayer && m_currentVleocity.sqrMagnitude > 0.01f)
			{
				Quaternion rotation = base.transform.rotation;
				Quaternion b = Quaternion.LookRotation((InputForward == ForwardMode.Player && Vector3.Dot(vector, m_currentVleocity) < 0f) ? (-m_currentVleocity) : m_currentVleocity);
				base.transform.rotation = Quaternion.Slerp(rotation, b, Damper.Damp(1f, VelocityDamping, deltaTime));
			}
			if (m_currentJumpSpeed != 0f)
			{
				m_currentJumpSpeed -= 10f * deltaTime;
			}
			Vector3 position = base.transform.position;
			position.y += m_currentJumpSpeed * deltaTime;
			if (position.y < m_restY)
			{
				position.y = m_restY;
				m_currentJumpSpeed = 0f;
			}
			base.transform.position = position;
			if (Input.GetKeyDown(KeyCode.Space) && SpaceAction != null)
			{
				SpaceAction();
			}
			if (Input.GetKeyDown(KeyCode.Return) && EnterAction != null)
			{
				EnterAction();
			}
		}
	}

	public void Jump()
	{
		m_currentJumpSpeed += 10f * JumpTime * 0.5f;
	}
}
