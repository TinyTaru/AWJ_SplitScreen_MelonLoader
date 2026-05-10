using System;
using TMPro;
using UnityEngine;

namespace _Scripts.Emotes;

[Serializable]
public class EmoteSlice : WheelSlice
{
	[SerializeField]
	private TextMeshProUGUI emoteDisplayText;

	public override void Setup()
	{
		emoteDisplayText.transform.rotation = Quaternion.identity;
	}

	public void SetDisplayText(string text)
	{
		emoteDisplayText.text = text;
	}
}
