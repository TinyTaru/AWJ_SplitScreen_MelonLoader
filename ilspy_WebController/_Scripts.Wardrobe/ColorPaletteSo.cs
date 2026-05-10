using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Wardrobe;

[CreateAssetMenu(fileName = "New Color Palette", menuName = "FTG/New Color Palette")]
public class ColorPaletteSo : ScriptableObject
{
	public string displayName;

	public List<Material> materials;

	public List<Color> colors;

	public void UpdateColors()
	{
		colors = new List<Color>();
		foreach (Material material in materials)
		{
			if (!(material == null))
			{
				if (material.HasColor("_ColorLight"))
				{
					Color color = material.GetColor("_ColorLight");
					Color.RGBToHSV(color, out var H, out var S, out var V);
					material.SetColor("_ColorShadow", Color.HSVToRGB(H, S, V * 0.5f));
					colors.Add(color);
				}
				else if (material.HasColor("_Color"))
				{
					Color color2 = material.GetColor("_Color");
					colors.Add(color2);
				}
			}
		}
	}
}
