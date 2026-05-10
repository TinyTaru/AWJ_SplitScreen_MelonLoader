using UnityEngine;

namespace _Scripts.Office;

public class HologramTile : MonoBehaviour
{
	private Hologram hologram;

	private void Awake()
	{
		hologram = GetComponentInParent<Hologram>();
	}

	public void PlaceTile()
	{
		if (hologram != null)
		{
			hologram.PlaceTile();
		}
	}
}
