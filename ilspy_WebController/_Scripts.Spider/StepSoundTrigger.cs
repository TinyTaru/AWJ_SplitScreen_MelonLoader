using FMODUnity;
using UnityEngine;
using _Scripts.Environment;

namespace _Scripts.Spider;

public class StepSoundTrigger : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter stepSound;

	private void OnTriggerEnter(Collider other)
	{
		if (!(stepSound == null))
		{
			EnvironmentMaterial componentInParent = other.GetComponentInParent<EnvironmentMaterial>();
			if (!(componentInParent == null))
			{
				stepSound.SetParameter("step_material", (float)componentInParent.Mat);
				stepSound.Play();
			}
		}
	}
}
