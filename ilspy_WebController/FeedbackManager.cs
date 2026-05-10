using MoreMountains.Feedbacks;
using UnityEngine;
using _Scripts.Singletons;

public class FeedbackManager : Singleton<FeedbackManager>
{
	[Header("Quit Screen Animations")]
	[SerializeField]
	public MMF_Player quitFlyUp;

	[SerializeField]
	public MMF_Player quitFlyDown;

	[Header("Splash Screen Animations")]
	[SerializeField]
	public MMF_Player logoFlyUp;

	[SerializeField]
	public MMF_Player logoFlyDown;

	[Header("Arachnophobia Screen Animations")]
	[SerializeField]
	public MMF_Player phobiaFlyUp;

	[SerializeField]
	public MMF_Player phobiaFlyUp2;

	[SerializeField]
	public MMF_Player phobiaDown;

	[Header("Main Menu Screen Animations")]
	[SerializeField]
	public MMF_Player mainFlyUp;

	[SerializeField]
	public MMF_Player mainFlyDown;

	[SerializeField]
	public MMF_Player mainReload;

	[Header("Pause Menu Screen Animations")]
	[SerializeField]
	public MMF_Player pauseFlyUp;

	[SerializeField]
	public MMF_Player pauseFlyDown;

	[SerializeField]
	public MMF_Player pauseReload;

	[SerializeField]
	public MMF_Player pauseFlyDownResetPlayer;

	public MMF_Player openWardrobeInstantly;
}
