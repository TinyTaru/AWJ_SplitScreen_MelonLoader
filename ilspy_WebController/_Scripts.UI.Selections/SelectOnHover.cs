using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Scripts.UI.Selections;

[RequireComponent(typeof(Selectable))]
public class SelectOnHover : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IDeselectHandler
{
	private Selectable selectable;

	private void Awake()
	{
		selectable = GetComponent<Selectable>();
	}

	public void Select()
	{
		if (!EventSystem.current.alreadySelecting && selectable.interactable)
		{
			EventSystem.current.SetSelectedGameObject(base.gameObject);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Select();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		GetComponent<Selectable>().OnPointerExit(null);
	}
}
