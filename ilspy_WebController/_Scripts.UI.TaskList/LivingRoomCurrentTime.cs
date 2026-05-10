using System;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.TaskList;

public class LivingRoomCurrentTime : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI timeText;

	private void Update()
	{
		timeText.text = $"{DateTime.Now:HH:mm}";
	}
}
