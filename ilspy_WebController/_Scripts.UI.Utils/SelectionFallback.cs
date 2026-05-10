using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Scripts.UI.Utils;

[RequireComponent(typeof(Selectable))]
[DisallowMultipleComponent]
public class SelectionFallback : MonoBehaviour
{
	[SerializeField]
	private InputActionReference navigationInputAction;

	private Selectable selectable;

	private void Awake()
	{
		selectable = GetComponent<Selectable>();
	}

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

	private void NavigationInputAction_OnPerformed(InputAction.CallbackContext obj)
	{
		if (EventSystem.current.currentSelectedGameObject == null)
		{
			selectable.Select();
			selectable.OnSelect(null);
		}
	}
}
