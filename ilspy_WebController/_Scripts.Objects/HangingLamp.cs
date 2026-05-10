using UnityEngine;

namespace _Scripts.Objects;

public class HangingLamp : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer[] lamps;

	[SerializeField]
	private int lightBulbMaterialIndex;

	[SerializeField]
	private Material lightBulbOnMaterial;

	[SerializeField]
	private Material lightBulbOffMaterial;

	[SerializeField]
	private GameObject lightRay;

	public void SwitchOn()
	{
		MeshRenderer[] array = lamps;
		foreach (MeshRenderer obj in array)
		{
			Material[] sharedMaterials = obj.sharedMaterials;
			sharedMaterials[lightBulbMaterialIndex] = lightBulbOnMaterial;
			obj.sharedMaterials = sharedMaterials;
		}
		lightRay.SetActive(value: true);
	}

	public void SwitchOff()
	{
		MeshRenderer[] array = lamps;
		foreach (MeshRenderer obj in array)
		{
			Material[] sharedMaterials = obj.sharedMaterials;
			sharedMaterials[lightBulbMaterialIndex] = lightBulbOffMaterial;
			obj.sharedMaterials = sharedMaterials;
		}
		lightRay.SetActive(value: false);
	}
}
