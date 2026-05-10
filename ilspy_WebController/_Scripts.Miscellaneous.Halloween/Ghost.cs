using UnityEngine;
using _Scripts.LevelSaving;
using _Scripts.Objects;
using _Scripts.Singletons;
using _Scripts.Spider;

namespace _Scripts.Miscellaneous.Halloween;

[DisallowMultipleComponent]
public class Ghost : MonoBehaviour, IInitializable<KitchenHalloweenGhostSaveData>, IHasSaveData<KitchenHalloweenGhostSaveData>
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private GameObject collectibleHat;

	private Transform player;

	private MovableObject movableObject;

	private Animator animator;

	private Rigidbody rb;

	private bool active;

	private Color color;

	private float spawnTime;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorId = Shader.PropertyToID("_Color");

	private static readonly int spawnTimeId = Shader.PropertyToID("_SpawnTime");

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void Start()
	{
		player = Singleton<GameController>.Instance.Player.transform;
		active = true;
		if (animator != null)
		{
			animator.enabled = true;
		}
		ApplyMpb();
	}

	private void FixedUpdate()
	{
		if (!active)
		{
			return;
		}
		bool num = GetComponentInChildren<BodyMovement>() != null;
		bool flag = movableObject != null && movableObject.ConnectedWebJointCount() > 0;
		if (num || flag)
		{
			active = false;
			if (animator != null)
			{
				animator.enabled = false;
			}
		}
		else if (player != null)
		{
			Vector3 worldPosition = new Vector3(player.position.x, base.transform.position.y, player.position.z);
			base.transform.LookAt(worldPosition);
		}
	}

	private void ApplyMpb()
	{
		if (!(meshRenderer == null))
		{
			meshRenderer.GetPropertyBlock(mpb);
			mpb.SetColor(colorId, color);
			mpb.SetFloat(spawnTimeId, spawnTime);
			meshRenderer.SetPropertyBlock(mpb);
		}
	}

	public void Initialize(KitchenHalloweenGhostSaveData saveData)
	{
		spawnTime = saveData.spawnTime;
		color = saveData.color;
		ApplyMpb();
	}

	public KitchenHalloweenGhostSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Ghost " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		KitchenHalloweenGhostSaveData result = default(KitchenHalloweenGhostSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.spawnTime = spawnTime;
		result.color = color;
		return result;
	}

	public Color GetColor()
	{
		return color;
	}

	public float GetSpawnTime()
	{
		return spawnTime;
	}

	public void SetVelocity(Vector3 velocity)
	{
		if (rb != null)
		{
			rb.linearVelocity = velocity;
		}
	}

	public void SetCurrentTimeAsSpawnTime()
	{
		spawnTime = Time.time;
	}

	public void SetColor(Color newColor)
	{
		color = newColor;
		ApplyMpb();
	}

	public void ActivateCollectibleHat()
	{
		if (collectibleHat != null)
		{
			collectibleHat.SetActive(value: true);
		}
	}

	public void DestroySafely()
	{
		if (movableObject != null)
		{
			movableObject.DestroySafely();
		}
	}
}
