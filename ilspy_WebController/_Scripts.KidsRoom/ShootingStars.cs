using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.KidsRoom;

public class ShootingStars : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private int starsToPlace = 5;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onStarPlacedEvent;

	[SerializeField]
	private UnityEvent onAllStarsPlacedEvent;

	private int starsPlaced;

	private void Awake()
	{
		starsPlaced = 0;
	}

	public void PlaceStar()
	{
		starsPlaced++;
		onStarPlacedEvent?.Invoke();
		if (starsPlaced >= starsToPlace)
		{
			onAllStarsPlacedEvent?.Invoke();
		}
	}
}
