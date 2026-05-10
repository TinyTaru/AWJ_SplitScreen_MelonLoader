using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Scripts.UI.Selections;

public class SelectionManager : MonoBehaviour
{
	[SerializeField]
	private Selectable initialSelection;

	[SerializeField]
	private bool alwaysSelectOnEnable;

	private Selectable lastSelection;

	private bool firstLoad = true;

	private Ray ray;

	private RaycastHit hit;

	public void UpdateLastSelection(Selectable item)
	{
		lastSelection = item;
	}

	public void Select()
	{
		if (lastSelection == null || alwaysSelectOnEnable || firstLoad)
		{
			initialSelection.Select();
			initialSelection.OnSelect(null);
		}
		else
		{
			lastSelection.Select();
			lastSelection.OnSelect(null);
		}
		firstLoad = false;
	}

	public void Deselect()
	{
		if (!(EventSystem.current == null) && EventSystem.current.currentSelectedGameObject != null)
		{
			lastSelection = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	public void UpdateSelection()
	{
		Selectable selectable = IsPointerOverUIElement();
		if (selectable != null && selectable != lastSelection)
		{
			Deselect();
			EventSystem.current.SetSelectedGameObject(selectable.gameObject);
			lastSelection = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
		}
		else
		{
			Select();
		}
	}

	public void Reset()
	{
		firstLoad = true;
	}

	private Selectable IsPointerOverUIElement()
	{
		if ((bool)lastSelection)
		{
			lastSelection.OnDeselect(null);
		}
		return IsPointerOverUIElement(GetEventSystemRaycastResults());
	}

	private Selectable IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)
	{
		for (int i = 0; i < eventSystemRaycastResults.Count; i++)
		{
			Selectable componentInParent = eventSystemRaycastResults[i].gameObject.GetComponentInParent<Selectable>();
			if (componentInParent != null)
			{
				return componentInParent;
			}
		}
		return null;
	}

	private static List<RaycastResult> GetEventSystemRaycastResults()
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = Mouse.current.position.ReadValue();
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, list);
		return list;
	}
}
