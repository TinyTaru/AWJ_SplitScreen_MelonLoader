using UnityEngine;

namespace _Scripts.Objects;

[RequireComponent(typeof(MovableObject))]
[DisallowMultipleComponent]
public class BlendableObject : MonoBehaviour
{
	[SerializeField]
	private Color particleColor = Color.white;

	private MovableObject movableObject;

	public Color ParticleColor => particleColor;

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
	}

	public MovableObject GetMovableObject()
	{
		return movableObject;
	}
}
