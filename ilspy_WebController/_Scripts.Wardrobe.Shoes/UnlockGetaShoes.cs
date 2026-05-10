using UnityEngine;
using _Scripts.CosmeticItems;
using _Scripts.Objects;
using _Scripts.Singletons;

namespace _Scripts.Wardrobe.Shoes;

public class UnlockGetaShoes : MonoBehaviour
{
	[SerializeField]
	private KitchenPlant bonsai;

	[SerializeField]
	private CosmeticItemShoeSo getaShoes;

	private void Start()
	{
		bonsai.OnPlantWateredEvent += Bonsai_OnPlantWateredEvent;
	}

	private void Bonsai_OnPlantWateredEvent(object sender, KitchenPlant.OnPlantWateredEventEventArgs e)
	{
		Singleton<CosmeticItemsController>.Instance.Unlock(getaShoes);
	}
}
