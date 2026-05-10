using UnityEngine;

namespace _Scripts.Spider;

public class ThumbnailSpider : MonoBehaviour
{
	[Header("Body")]
	[SerializeField]
	private MeshRenderer[] bodies;

	[SerializeField]
	private Material bodyMaterial;

	[Header("Legs")]
	[SerializeField]
	private MeshRenderer[] legs;

	[SerializeField]
	private Material legMaterial;

	[Header("Joints")]
	[SerializeField]
	private MeshRenderer[] joints;

	[SerializeField]
	private Material jointMaterial;

	private void Start()
	{
		SetBodyColor();
		SetLegColor();
		SetJointColor();
	}

	public void SetBodyColor()
	{
		MeshRenderer[] array = bodies;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sharedMaterial = bodyMaterial;
		}
	}

	public void SetLegColor()
	{
		MeshRenderer[] array = legs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sharedMaterial = legMaterial;
		}
	}

	public void SetJointColor()
	{
		MeshRenderer[] array = joints;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sharedMaterial = jointMaterial;
		}
	}
}
