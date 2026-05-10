using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Miscellaneous;

public class StorySandboxSwitcher : MonoBehaviour
{
	[SerializeField]
	private GameObject[] storyModeObjects;

	[SerializeField]
	private GameObject[] sandboxModeObjects;

	private void Start()
	{
		bool isStoryLevel = Singleton<SceneController>.Instance.IsStoryLevel;
		GameObject[] array = storyModeObjects;
		foreach (GameObject obj in array)
		{
			if (!isStoryLevel)
			{
				Object.Destroy(obj);
			}
		}
		array = sandboxModeObjects;
		foreach (GameObject obj2 in array)
		{
			if (isStoryLevel)
			{
				Object.Destroy(obj2);
			}
		}
	}
}
