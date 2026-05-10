using UnityEngine;
using UnityEngine.Playables;

namespace Cinemachine.Examples;

public class GenericTrigger : MonoBehaviour
{
	public PlayableDirector timeline;

	private void Start()
	{
		timeline = GetComponent<PlayableDirector>();
	}

	private void OnTriggerExit(Collider c)
	{
		if (c.gameObject.CompareTag("Player"))
		{
			timeline.time = 27.0;
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.CompareTag("Player"))
		{
			timeline.Stop();
			timeline.Play();
		}
	}
}
