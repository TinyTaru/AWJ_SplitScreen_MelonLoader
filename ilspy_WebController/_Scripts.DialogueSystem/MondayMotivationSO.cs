using UnityEngine;

namespace _Scripts.DialogueSystem;

[CreateAssetMenu(fileName = "New Monday Motivation", menuName = "FTG/Monday Motivation")]
public class MondayMotivationSO : ScriptableObject
{
	[TextArea(3, 50)]
	public string[] messages;
}
