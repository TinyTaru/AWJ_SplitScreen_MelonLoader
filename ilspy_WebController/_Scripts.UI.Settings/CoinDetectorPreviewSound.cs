using FMODUnity;
using UnityEngine;

namespace _Scripts.UI.Settings;

public class CoinDetectorPreviewSound : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter coinDetectorPreviewSound;

	public void PlayPreviewSound(int index)
	{
		coinDetectorPreviewSound.Play();
		coinDetectorPreviewSound.SetParameter("coindetector_sound", index);
	}
}
