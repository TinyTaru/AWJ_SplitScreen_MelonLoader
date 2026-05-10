using FMODUnity;
using UnityEngine;

namespace _Scripts.LivingRoom;

[CreateAssetMenu(fileName = "New Movie", menuName = "FTG/New Movie")]
public class Movie : ScriptableObject
{
	[Header("Start Screen")]
	public EventReference startScreenSoundEvent;

	public Texture2D startScreenTexture;

	[Header("Movie")]
	public EventReference scoreSoundEvent;

	public Frame[] frames;
}
