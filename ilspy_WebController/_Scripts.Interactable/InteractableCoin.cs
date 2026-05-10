using System;
using FMODUnity;
using UnityEngine;
using _Scripts.Miscellaneous;
using _Scripts.Singletons;
using _Scripts.Spider;

namespace _Scripts.Interactable;

public class InteractableCoin : MonoBehaviour, IInteractable
{
	public class OnCoinCollectedEventArgs : EventArgs
	{
		public int coindId;

		public int coinAmount;
	}

	[Header("References")]
	[SerializeField]
	private CollectibleStyleChanger collectibleStyleChanger;

	[Header("Parameters")]
	[SerializeField]
	private int coinId;

	[SerializeField]
	private int coinAmount = 1;

	[SerializeField]
	private float collectionTime = 1f;

	[Header("Sounds")]
	[SerializeField]
	private EventReference collectSound;

	private bool flyToPlayer;

	private float timer;

	private Vector3 startPosition;

	private SpiderInteraction playerInteraction;

	private Color color;

	private float shadowFactor;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorId = Shader.PropertyToID("_Color");

	private static readonly int shadowFactorId = Shader.PropertyToID("_ShadowFactor");

	public int CoinId => coinId;

	public event EventHandler<OnCoinCollectedEventArgs> OnCoinCollected;

	public event Action<IInteractable> OnInteractableDestroyed;

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void Start()
	{
		if (!Singleton<SceneController>.Instance.IsStoryLevel)
		{
			base.gameObject.SetActive(value: false);
		}
		else if (Singleton<CoinController>.Instance != null && Singleton<CoinController>.Instance.CoinAlreadyCollected(coinId))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void FixedUpdate()
	{
		if (flyToPlayer)
		{
			timer += Time.fixedDeltaTime / collectionTime;
			base.transform.position = Vector3.Lerp(startPosition, Singleton<GameController>.Instance.Player.Root.position, timer);
			if (timer >= 1f)
			{
				OnInteract();
			}
		}
	}

	private void OnDestroy()
	{
		this.OnInteractableDestroyed?.Invoke(this);
	}

	public void OnPlayerEnter(SpiderInteraction spiderInteraction)
	{
		if (!flyToPlayer)
		{
			playerInteraction = ((spiderInteraction == null) ? Singleton<GameController>.Instance.Player.GetComponentInChildren<SpiderInteraction>() : spiderInteraction);
			flyToPlayer = true;
			startPosition = base.transform.position;
			timer = 0f;
		}
	}

	public void ShowInteractionPrompt(SpiderInteraction spiderInteraction, bool value)
	{
	}

	public void HideInteractionPrompt()
	{
	}

	public void OnInteract()
	{
		playerInteraction.RemoveInteractable(this);
		CollectibleVisuals component = collectibleStyleChanger.GetActiveGameObject().GetComponent<CollectibleVisuals>();
		if (component != null)
		{
			component.OnInteract();
		}
		this.OnCoinCollected?.Invoke(this, new OnCoinCollectedEventArgs
		{
			coindId = coinId,
			coinAmount = coinAmount
		});
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public Vector3 Position()
	{
		return base.transform.position;
	}

	public bool CanInteract()
	{
		return true;
	}

	public bool ShowInteractButton()
	{
		return false;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}
}
