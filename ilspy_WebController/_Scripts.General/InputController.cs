using UnityEngine;

namespace _Scripts.General;

public class InputController : MonoBehaviour
{
	public static InputController Instance;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Object.DontDestroyOnLoad(this);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
