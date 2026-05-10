using UnityEngine;

namespace _Scripts.Office;

[CreateAssetMenu(fileName = "New 3D Model", menuName = "FTG/New 3D Model")]
public class Blueprint3DModelSo : ScriptableObject
{
	public string modelName;

	public Sprite modelSprite;

	public PrintableObject model;

	public GameObject hologram;

	public float printTime = 10f;

	public float modelHeight = 30f;
}
