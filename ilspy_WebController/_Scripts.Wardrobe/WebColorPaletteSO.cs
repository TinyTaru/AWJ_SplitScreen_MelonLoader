using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Wardrobe;

[CreateAssetMenu(fileName = "New Web Color Palette", menuName = "FTG/New Web Color Palette")]
public class WebColorPaletteSO : ScriptableObject
{
	public List<Color> colors;
}
