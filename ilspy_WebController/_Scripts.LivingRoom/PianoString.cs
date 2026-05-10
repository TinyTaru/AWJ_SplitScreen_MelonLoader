using UnityEngine;
using _Scripts.Web;

namespace _Scripts.LivingRoom;

public class PianoString : MonoBehaviour
{
	[SerializeField]
	private int stringId;

	[SerializeField]
	private Material defaultMaterial;

	[SerializeField]
	private Material currentWebTargetMaterial;

	[SerializeField]
	private Material mainWebAttachedMaterial;

	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private WebModifier webModifier;

	[SerializeField]
	private Transform shmoopSpawnPoint;

	[SerializeField]
	private Transform shmoopEndPoint;

	[SerializeField]
	private Transform minigameCollider;

	[SerializeField]
	private Transform realCollider;

	private bool isCurrentWebTarget;

	private bool isMainWebAttached;

	private void UpdateMaterial()
	{
		meshRenderer.sharedMaterial = (isMainWebAttached ? mainWebAttachedMaterial : (isCurrentWebTarget ? currentWebTargetMaterial : defaultMaterial));
	}

	public Transform GetShmoopSpawnPoint()
	{
		return shmoopSpawnPoint;
	}

	public Transform GetShmoopEndPoint()
	{
		return shmoopEndPoint;
	}

	public void SwitchToMinigameCollider(bool gameStarted)
	{
		minigameCollider.gameObject.SetActive(gameStarted);
		realCollider.gameObject.SetActive(!gameStarted);
		webModifier.SetSuppressAttachSound(gameStarted);
	}

	public void SetCurrentWebTarget(bool value)
	{
		isCurrentWebTarget = value;
		UpdateMaterial();
	}

	public void SetMainWebAttached(bool value)
	{
		isMainWebAttached = value;
		UpdateMaterial();
	}
}
