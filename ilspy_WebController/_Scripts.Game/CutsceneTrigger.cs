using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace _Scripts.Game;

public class CutsceneTrigger : MonoBehaviour
{
	[SerializeField]
	private PlayableDirector director;

	[SerializeField]
	private CinemachineDollyCart dollyCart;

	[SerializeField]
	private CinemachineVirtualCamera virtualCamera;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			if (dollyCart != null)
			{
				dollyCart.m_Position = 0f;
			}
			if (director != null)
			{
				director.Play();
			}
		}
	}
}
