using UnityEngine;
using _Scripts.General;

namespace _Scripts.KidsRoom;

public class WantedObject : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The CharacterSO corresponding to this character")]
	private CharacterSO characterSo;

	public CharacterSO GetCharacterSo()
	{
		return characterSo;
	}
}
