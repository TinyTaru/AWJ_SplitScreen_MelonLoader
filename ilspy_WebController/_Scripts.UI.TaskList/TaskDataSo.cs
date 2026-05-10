using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Serialization;
using _Scripts.CosmeticItems;

namespace _Scripts.UI.TaskList;

[CreateAssetMenu(menuName = "FTG/New Task Data", fileName = "New Tasks Data", order = 0)]
public class TaskDataSo : ScriptableObject
{
	public string text;

	[QuestPopup(false)]
	public string questName;

	[FormerlySerializedAs("unlockableSos")]
	public CosmeticItemSo[] CosmeticItemSos;

	public int coinAmount;

	public bool partOfTotalQuests = true;
}
