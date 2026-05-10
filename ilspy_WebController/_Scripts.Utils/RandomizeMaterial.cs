using UnityEngine;

namespace _Scripts.Utils;

public class RandomizeMaterial : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer[] meshRenderers;

	[SerializeField]
	private Material[] materials;

	private void Start()
	{
		Material material = materials.RandomValue();
		MeshRenderer[] array = meshRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].material = material;
		}
	}
}
