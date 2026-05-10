using UnityEngine;

[ExecuteInEditMode]
public class TreeMaterialFixer : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private Material treeBarkMaterial;

	[SerializeField]
	private Material treeLeavesMaterial;

	private void Update()
	{
		if (meshRenderer.sharedMaterials.Length == 1)
		{
			Material[] array = new Material[1] { treeBarkMaterial };
			if (meshRenderer.sharedMaterials != array)
			{
				meshRenderer.sharedMaterials = array;
			}
		}
		else if (meshRenderer.sharedMaterials.Length == 2)
		{
			Material[] array2 = new Material[2] { treeBarkMaterial, treeLeavesMaterial };
			if (meshRenderer.sharedMaterials != array2)
			{
				meshRenderer.sharedMaterials = array2;
			}
		}
	}
}
