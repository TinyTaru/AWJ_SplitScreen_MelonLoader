using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Objects;
using _Scripts.Singletons;
using _Scripts.Spider;

namespace _Scripts.LivingRoom;

[RequireComponent(typeof(MovableObject))]
public class AncientPotion : MonoBehaviour
{
	[SerializeField]
	private AncientPotionEffectSo ancientPotionEffectSo;

	[SerializeField]
	[QuestPopup(false)]
	private string questName;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onPotionDrankEvent;

	private MovableObject movableObject;

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
	}

	public void Drink()
	{
		QuestLog.SetQuestState(questName, QuestState.Success);
		if (!(Singleton<AncientPotionController>.Instance == null))
		{
			Singleton<AncientPotionController>.Instance.StartOrExtendEffect(ancientPotionEffectSo);
			onPotionDrankEvent?.Invoke();
			movableObject.DestroySafely();
		}
	}
}
