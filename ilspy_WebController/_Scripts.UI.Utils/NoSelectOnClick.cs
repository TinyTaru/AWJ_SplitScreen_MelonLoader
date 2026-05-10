using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Scripts.UI.Utils;

[RequireComponent(typeof(Selectable))]
[DisallowMultipleComponent]
public class NoSelectOnClick : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private InputActionReference navigationInputAction;

	private void OnEnable()
	{
		if (navigationInputAction != null)
		{
			navigationInputAction.action.performed += NavigationInputAction_OnPerformed;
		}
	}

	private void OnDisable()
	{
		if (navigationInputAction != null)
		{
			navigationInputAction.action.performed -= NavigationInputAction_OnPerformed;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		EventSystem.current.SetSelectedGameObject(null);
	}

	private void NavigationInputAction_OnPerformed(InputAction.CallbackContext obj)
	{
		if (EventSystem.current.currentSelectedGameObject == base.gameObject)
		{
			SelectionFallback selectionFallback = Object.FindFirstObjectByType<SelectionFallback>();
			if (selectionFallback == null)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(selectionFallback.gameObject);
			}
		}
	}
}
