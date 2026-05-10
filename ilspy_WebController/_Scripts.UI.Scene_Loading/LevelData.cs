using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.UI.Scene_Loading;

[CreateAssetMenu(fileName = "LevelData", menuName = "FTG/Level Data", order = 0)]
public class LevelData : ScriptableObject
{
	public string levelName;

	[FormerlySerializedAs("levelImage")]
	public Sprite levelImageNormal;

	public Sprite levelImageArachnophobia;

	public string sceneName;

	public bool comingSoonSandbox;
}
