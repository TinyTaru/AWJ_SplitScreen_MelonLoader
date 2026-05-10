using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.UI.Selections;

public class AutoDeselect : MonoBehaviour
{
	private void Start()
	{
		EventTrigger eventTrigger = GetComponent<EventTrigger>();
		if (eventTrigger == null)
		{
			eventTrigger = base.gameObject.AddComponent<EventTrigger>();
		}
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerEnter;
		entry.callback.AddListener(delegate
		{
			Deselect();
		});
		eventTrigger.triggers.Add(entry);
		EventTrigger.Entry entry2 = new EventTrigger.Entry();
		entry2.eventID = EventTriggerType.Select;
		entry2.callback.AddListener(delegate
		{
			Deselect();
		});
		eventTrigger.triggers.Add(entry2);
	}

	public void Deselect()
	{
		if (!EventSystem.current.alreadySelecting)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
	}
}
