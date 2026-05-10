using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.UI.Save_Slots;

public class RenameSaveSlotButton : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	[Header("References")]
	[SerializeField]
	private SaveSlot saveSlot;

	public void OnSelect(BaseEventData eventData)
	{
		saveSlot.OnSaveSlotButtonSelected();
	}
}
