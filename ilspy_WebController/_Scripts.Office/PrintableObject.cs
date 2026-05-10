using UnityEngine;
using _Scripts.LevelSaving;

namespace _Scripts.Office;

[DisallowMultipleComponent]
public class PrintableObject : MonoBehaviour, IInitializable<PrintableObjectSaveData>, IHasSaveData<PrintableObjectSaveData>
{
	[SerializeField]
	private MeshRenderer[] meshRenderers;

	private Color color;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorId = Shader.PropertyToID("_Color");

	private void ApplyColor()
	{
		MeshRenderer[] array = meshRenderers;
		foreach (MeshRenderer obj in array)
		{
			if (mpb == null)
			{
				mpb = new MaterialPropertyBlock();
			}
			obj.GetPropertyBlock(mpb);
			mpb.SetColor(colorId, color);
			obj.SetPropertyBlock(mpb);
		}
	}

	private void AutoSetupMeshRenderers()
	{
		meshRenderers = GetComponentsInChildren<MeshRenderer>();
	}

	public void Initialize(PrintableObjectSaveData saveData)
	{
		SetColor(saveData.color);
	}

	public PrintableObjectSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Printable Object " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		PrintableObjectSaveData result = default(PrintableObjectSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.color = color;
		return result;
	}

	public void SetColor(Color newColor)
	{
		color = newColor;
		ApplyColor();
	}
}
