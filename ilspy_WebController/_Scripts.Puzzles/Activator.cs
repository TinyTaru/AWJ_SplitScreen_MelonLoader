using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Puzzles;

public class Activator : MonoBehaviour
{
	[HideInInspector]
	public UnityEvent StateChangedEvent;

	public UnityEvent OnActivate;

	public UnityEvent OnDeactivate;

	public bool Activated { get; private set; }

	protected void SetActivated(bool value)
	{
		Activated = value;
		if (Activated)
		{
			OnActivate.Invoke();
		}
		else
		{
			OnDeactivate.Invoke();
		}
		StateChangedEvent.Invoke();
	}
}
