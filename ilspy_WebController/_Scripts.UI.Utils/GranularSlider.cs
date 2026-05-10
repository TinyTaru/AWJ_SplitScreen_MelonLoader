using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Scripts.UI.Utils;

public class GranularSlider : Slider
{
	[SerializeField]
	private float defaultMoveStep = -1f;

	private float moveStep;

	protected override void Awake()
	{
		base.Awake();
		moveStep = defaultMoveStep;
	}

	public override void OnMove(AxisEventData eventData)
	{
		if (!IsActive() || !IsInteractable())
		{
			base.OnMove(eventData);
			return;
		}
		if (Mathf.Approximately(moveStep, -1f))
		{
			moveStep = (base.maxValue - base.minValue) / 10f;
		}
		float num = 0f;
		switch (eventData.moveDir)
		{
		case MoveDirection.Left:
			num = 0f - moveStep;
			break;
		case MoveDirection.Right:
			num = moveStep;
			break;
		default:
			base.OnMove(eventData);
			return;
		}
		value = Mathf.Clamp(value + num, base.minValue, base.maxValue);
		eventData.Use();
	}

	public void SetMoveStep(float newMoveStep)
	{
		moveStep = newMoveStep;
	}
}
