using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.TaskList;

[CreateAssetMenu(menuName = "FTG/New Task List Fonts", fileName = "New Tasks List Fonts", order = 0)]
public class TaskListFontsSo : SerializedScriptableObject
{
	public Dictionary<SystemLanguage, TMP_FontAsset> dictionary = new Dictionary<SystemLanguage, TMP_FontAsset>();
}
