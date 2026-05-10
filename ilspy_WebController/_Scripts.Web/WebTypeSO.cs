using UnityEngine;
using _Scripts.Emotes;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.Web;

[CreateAssetMenu(fileName = "New Web Type", menuName = "FTG/New Web Type")]
public class WebTypeSO : WheelOptionSo
{
	[TextArea(1, 3)]
	public string displayName;

	public WebType webType;

	public override void ExecuteSelection()
	{
		Singleton<WebController>.Instance.SetWebType(webType);
		Singleton<GameController>.Instance.State = GameController.GameState.Running;
	}
}
