using UnityEngine;
using UnityEngine.Events;

namespace Cinemachine.Examples;

public class InvokeEventFromInspector : MonoBehaviour
{
	public UnityEvent Event = new UnityEvent();

	public void Invoke()
	{
		Event.Invoke();
	}
}
