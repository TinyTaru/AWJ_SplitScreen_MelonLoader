using FMODUnity;
using UnityEngine;

namespace _Scripts.KidsRoom;

public class KidsPiano : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter keyPressedSound;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void PressKey(int keyIndex)
	{
		keyPressedSound.Play();
		keyPressedSound.SetParameter("note", keyIndex);
	}
}
