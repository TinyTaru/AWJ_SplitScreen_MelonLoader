using UnityEngine;

namespace _Scripts.Objects;

public class DisablePicking : MonoBehaviour
{
	private void Start()
	{
		Object.Destroy(this);
	}
}
