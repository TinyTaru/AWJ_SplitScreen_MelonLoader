using UnityEngine;
using _Scripts.General;

namespace _Scripts.LivingRoom;

public class VinylRecord : MonoBehaviour
{
	[SerializeField]
	private RecordType recordType;

	public RecordType GetRecordType()
	{
		return recordType;
	}
}
