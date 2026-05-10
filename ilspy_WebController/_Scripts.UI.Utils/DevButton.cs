using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.UI.Utils;

public class DevButton : MonoBehaviour
{
	private int counter;

	private void OnEnable()
	{
		counter = 0;
	}

	public void IncreaseCounter()
	{
		counter++;
		Debug.Log(counter);
		if (counter == 10)
		{
			Singleton<ProfileController>.Instance.UnlockKitchen();
			Singleton<ProfileController>.Instance.UnlockOffice();
			Singleton<ProfileController>.Instance.UnlockKidsRoom();
			Singleton<ProfileController>.Instance.UnlockLivingRoom();
			Singleton<SceneController>.Instance.SetStoryLevelName("EA_Level_Hub");
		}
	}
}
