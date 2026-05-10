using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoModeHandler : MonoBehaviour
{
	[SerializeField]
	private InputActionReference playerInput;

	[SerializeField]
	private InputActionReference photoInput;

	private void OnEnable()
	{
		photoInput.action.performed += OnExit;
	}

	private void OnDisable()
	{
		photoInput.action.performed -= OnExit;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnExit(InputAction.CallbackContext ctx)
	{
	}
}
