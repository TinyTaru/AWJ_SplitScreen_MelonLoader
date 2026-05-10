using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.Singletons;

namespace _Scripts.Office;

public class RaceSettings : MonoBehaviour
{
	[SerializeField]
	private InputActionReference backInputAction;

	private void OnEnable()
	{
		backInputAction.action.performed += BackInputAction_OnPerformed;
	}

	private void OnDisable()
	{
		backInputAction.action.performed -= BackInputAction_OnPerformed;
	}

	private void BackInputAction_OnPerformed(InputAction.CallbackContext obj)
	{
		base.gameObject.SetActive(value: false);
		Singleton<GameController>.Instance.ContinueGame();
	}
}
