using UnityEngine;
using _Scripts.Singletons;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.Tutorial;

public class TutorialDone : MonoBehaviour
{
	[SerializeField]
	private LevelData nextLevelDemo;

	[SerializeField]
	private LevelData nextLevelEarlyAccess;

	public void LoadNextLevel()
	{
		Singleton<SceneController>.Instance.LoadSpecificStoryLevel(nextLevelEarlyAccess.sceneName);
	}
}
