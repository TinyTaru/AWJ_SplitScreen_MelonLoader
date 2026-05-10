using UnityEngine;

namespace _Scripts.Singletons;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	[SerializeField]
	private bool dontDestroyOnLoad;

	private static T instance;

	public static T Instance => instance;

	protected virtual void Awake()
	{
		if (instance == null)
		{
			instance = base.gameObject.GetComponent<T>();
			if (dontDestroyOnLoad)
			{
				base.transform.parent = null;
				Object.DontDestroyOnLoad(instance);
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
