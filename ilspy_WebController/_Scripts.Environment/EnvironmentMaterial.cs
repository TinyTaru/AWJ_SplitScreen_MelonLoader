using UnityEngine;

namespace _Scripts.Environment;

[DisallowMultipleComponent]
public class EnvironmentMaterial : MonoBehaviour
{
	public enum EnvironmentMat
	{
		Grass,
		Wood,
		Web,
		Stone,
		Sand,
		Metal,
		Glass,
		Plastic,
		Paper,
		Snow,
		Cloth,
		Keyboard,
		Ceramic,
		MusicWeb
	}

	[SerializeField]
	private EnvironmentMat mat;

	public EnvironmentMat Mat => mat;

	public void SetEnvironmentMaterialToSnow()
	{
		mat = EnvironmentMat.Snow;
	}
}
