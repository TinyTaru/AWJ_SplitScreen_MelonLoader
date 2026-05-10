using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class CustomizationCutscene : MonoBehaviour
{
	[SerializeField]
	private PlayableDirector director;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference customizationCutsceneInputAction;

	private void OnEnable()
	{
		customizationCutsceneInputAction.action.performed += OnCustomizationCutscene;
	}

	private void OnDisable()
	{
		customizationCutsceneInputAction.action.performed -= OnCustomizationCutscene;
	}

	private void OnCustomizationCutscene(InputAction.CallbackContext ctx)
	{
		if (ctx.ReadValueAsButton())
		{
			director.Play();
		}
	}
}
