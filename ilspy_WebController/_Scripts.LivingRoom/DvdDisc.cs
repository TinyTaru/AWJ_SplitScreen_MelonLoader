using UnityEngine;
using _Scripts.General;
using _Scripts.Objects;

namespace _Scripts.LivingRoom;

[RequireComponent(typeof(MovableObject))]
public class DvdDisc : MonoBehaviour
{
	[SerializeField]
	private MovieType movieType;

	private MovableObject movableObject;

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
	}

	public void EnableColliders()
	{
		movableObject.SetColliderActive(value: true);
	}

	public void DisableColliders()
	{
		movableObject.SetColliderActive(value: false);
	}

	public void Eject(Vector3 force)
	{
		movableObject.GetRigidbody().AddForce(force, ForceMode.Impulse);
	}

	public MovieType GetMovieType()
	{
		return movieType;
	}
}
