using UnityEngine;

namespace _Scripts.UI.TaskList;

[CreateAssetMenu(menuName = "FTG/New Task List", fileName = "New Tasks List", order = 0)]
public class TaskList : ScriptableObject
{
	public TaskDataSo[] tasks;
}
