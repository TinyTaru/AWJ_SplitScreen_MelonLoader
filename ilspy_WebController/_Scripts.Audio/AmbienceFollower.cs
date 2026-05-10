using Dreamteck.Splines;
using FMODUnity;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Audio;

public class AmbienceFollower : MonoBehaviour
{
	[SerializeField]
	private SplineProjector splineProjector;

	[SerializeField]
	private StudioEventEmitter ambienceLoopSound;

	private void Start()
	{
		if (!(Singleton<GameController>.Instance == null))
		{
			splineProjector.projectTarget = Singleton<GameController>.Instance.Player.transform;
			splineProjector.targetObject = ambienceLoopSound.gameObject;
		}
	}
}
