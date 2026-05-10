using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using _Scripts.Singletons;

namespace _Scripts.UI.Audio;

public class MenuSoundController : MonoBehaviour
{
	private GameObject lastSelected;

	private PlayerInput playerInput;

	private void Start()
	{
		playerInput = Object.FindObjectsByType<PlayerInput>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
	}

	public void PlayButtonHoverSound()
	{
		if (EventSystem.current.currentSelectedGameObject == lastSelected || lastSelected == null)
		{
			lastSelected = EventSystem.current.currentSelectedGameObject;
			return;
		}
		Singleton<MusicController>.Instance.SelectSound();
		lastSelected = EventSystem.current.currentSelectedGameObject;
	}

	public void PlayButtonClickSound()
	{
		if (EventSystem.current.currentSelectedGameObject == lastSelected && playerInput.currentControlScheme != "Keyboard And Mouse")
		{
			Singleton<MusicController>.Instance.ClickSound();
			lastSelected = EventSystem.current.currentSelectedGameObject;
		}
	}

	public void ChangePanel()
	{
		lastSelected = null;
	}
}
