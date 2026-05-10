using UnityEngine;

namespace _Scripts.UI.Utils;

public class UniformScale : MonoBehaviour
{
	private void Start()
	{
		PerformScaling();
	}

	public void PerformScaling()
	{
		base.transform.localScale = Vector3.one;
		base.transform.localScale = Vector3.one / base.transform.lossyScale.x;
	}
}
