using UnityEngine;
using _Scripts.CosmeticItems;
using _Scripts.Emotes;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.Web;

[CreateAssetMenu(fileName = "New Web Color", menuName = "FTG/New Web Color")]
public class WebWheelOptionSo : WheelOptionSo
{
	public CosmeticItemWebSo cosmeticItemWebSo;

	public override void ExecuteSelection()
	{
		int itemIndex = Singleton<CosmeticItemsController>.Instance.GetItemIndex(cosmeticItemWebSo);
		Singleton<EmoteController>.Instance.CloseAllWheels();
		Singleton<WebController>.Instance.SetWebColor(itemIndex);
		SaveController.Save("WebColor", itemIndex, SaveData.Wardrobe);
		Singleton<GameController>.Instance.State = GameController.GameState.Running;
	}

	public void SetCosmeticItemWebSo(CosmeticItemWebSo newCosmeticItemWebSo)
	{
		cosmeticItemWebSo = newCosmeticItemWebSo;
	}
}
