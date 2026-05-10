using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Scripts.Singletons;

namespace _Scripts.UI.Settings;

public class SliderMoveSound : MonoBehaviour, IMoveHandler, IEventSystemHandler
{
	private Slider slider;

	private void Awake()
	{
		slider = GetComponent<Slider>();
	}

	public void OnMove(AxisEventData eventData)
	{
		if (eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right)
		{
			float value = slider.value;
			float minValue = slider.minValue;
			float maxValue = slider.maxValue;
			if (value > minValue && value < maxValue)
			{
				Singleton<MusicController>.Instance.ClickSound();
			}
		}
	}
}
