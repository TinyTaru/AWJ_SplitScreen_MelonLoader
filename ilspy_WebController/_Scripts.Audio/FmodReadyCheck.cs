using System.Collections;
using FMODUnity;
using UnityEngine;
using _Scripts.Singletons;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.Audio;

public class FmodReadyCheck : MonoBehaviour
{
	[SerializeField]
	private LevelData sceneToLoadWhenReady;

	[SerializeField]
	private LevelData sceneToLoadAndroid;

	[SerializeField]
	private LevelData sceneToLoadIos;

	private IEnumerator Start()
	{
		while (!RuntimeManager.HaveAllBanksLoaded)
		{
			yield return null;
		}
		Singleton<SceneController>.Instance.LoadScene(sceneToLoadWhenReady.sceneName);
	}
}
