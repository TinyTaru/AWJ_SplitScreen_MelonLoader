using UnityEngine;

namespace _Scripts.Wardrobe;

[CreateAssetMenu(menuName = "FTG/New Outfit SO", fileName = "New Outfit SO", order = 0)]
public class OutfitSo : ScriptableObject
{
	[SerializeField]
	private TextAsset outfitFile;

	public int version;

	public new string name;

	public bool arachnophobiaMode;

	public bool bodyEnabled;

	public Color bodyColor;

	public float bodyFluffiness;

	public bool abdomenEnabled;

	public Color abdomenColor;

	public float abdomenFluffiness;

	public Color[] legColors;

	public float[] legSegmentFluffiness;

	public bool[] legsEnabled;

	public Color[] jointColors;

	public float[] jointSegmentFluffiness;

	public int eyeIndex;

	public Color eyeColorBase;

	public Color eyeColorLeft;

	public Color eyeColorRight;

	public float[] eyeEffects;

	public int hatIndex;

	public Color[] hatColors;

	public float[] hatEffects;

	public int accessoryIndex;

	public Color[] accessoryColors;

	public float[] accessoryEffects;

	public int shoeIndex;

	public Color[] shoeColors;

	public float[] shoeEffects;
}
