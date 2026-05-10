using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace _Scripts.UI.Utils;

public class EventSystemFix : MonoBehaviour
{
	[SerializeField]
	private InputActionAsset inputActionAsset;

	[SerializeField]
	private InputActionReference point;

	[SerializeField]
	private InputActionReference leftClick;

	[SerializeField]
	private InputActionReference middleClick;

	[SerializeField]
	private InputActionReference rightClick;

	[SerializeField]
	private InputActionReference scrollWheel;

	[SerializeField]
	private InputActionReference move;

	[SerializeField]
	private InputActionReference submit;

	[SerializeField]
	private InputActionReference cancel;

	private InputSystemUIInputModule module;

	private void Awake()
	{
		StartCoroutine(InitializeInputModuleCoroutine());
	}

	private IEnumerator InitializeInputModuleCoroutine()
	{
		module = base.gameObject.AddComponent<InputSystemUIInputModule>();
		module.actionsAsset = inputActionAsset;
		module.point = point;
		module.leftClick = leftClick;
		module.middleClick = middleClick;
		module.rightClick = rightClick;
		module.scrollWheel = scrollWheel;
		module.move = move;
		module.submit = submit;
		module.cancel = cancel;
		yield return null;
		module.enabled = false;
		yield return null;
		module.enabled = true;
		yield return null;
	}
}
