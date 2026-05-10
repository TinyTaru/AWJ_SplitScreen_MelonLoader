using UnityEngine;

namespace _Scripts.Debugging;

public class TestScript : MonoBehaviour
{
	private void Awake()
	{
		Debug.Log(base.name + " Awake");
	}

	private void OnEnable()
	{
		Debug.Log(base.name + " OnEnable");
	}

	private void Start()
	{
		Debug.Log(base.name + " Start");
	}

	private void Update()
	{
	}
}
