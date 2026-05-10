using UnityEngine;
using _Scripts.Spider;

namespace _Scripts.KidsRoom;

public class ToyCar : MonoBehaviour
{
	public bool HasPlayerSpider()
	{
		return GetComponentInChildren<BodyMovement>() != null;
	}
}
