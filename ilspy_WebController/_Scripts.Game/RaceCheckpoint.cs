using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Game;

public class RaceCheckpoint : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private MeshRenderer circle;

	[SerializeField]
	private GameObject particles;

	private KitchenRaceController controller;

	private bool isActive;

	private Vector3 defaultSize;

	private void Awake()
	{
		defaultSize = circle.transform.localScale;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (isActive && !(controller == null) && other.CompareTag("Player"))
		{
			controller.CheckpointReached();
			base.gameObject.SetActive(value: false);
			Singleton<MusicController>.Instance.PlaySound("event:/game/general/quest_progress_increment");
		}
	}

	public void DisplayAsFutureCheckpoint(Material material)
	{
		isActive = false;
		circle.sharedMaterial = material;
		circle.transform.localScale = defaultSize;
		particles.SetActive(value: false);
	}

	public void MakeCurrentCheckpoint(Material material)
	{
		isActive = true;
		circle.sharedMaterial = material;
		circle.transform.localScale = defaultSize;
		particles.SetActive(value: true);
	}

	public void AssignRaceController(KitchenRaceController newController)
	{
		controller = newController;
	}
}
