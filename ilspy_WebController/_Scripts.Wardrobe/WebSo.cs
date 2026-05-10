using UnityEngine;
using UnityEngine.Serialization;
using _Scripts.Web;

namespace _Scripts.Wardrobe;

[CreateAssetMenu(menuName = "FTG/New Web SO", fileName = "New Web SO", order = 0)]
public class WebSo : ScriptableObject
{
	public WebThread webThread;

	[FormerlySerializedAs("webMaterial")]
	public Material webThreadMaterial;

	public Material webTargetMaterial;

	public Material webParticlesMaterial;

	public Material webIndicatorMaterial;

	public Color webColor;

	public Sprite webSprite;

	public string webSound;
}
