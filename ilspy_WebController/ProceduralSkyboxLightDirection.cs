using UnityEngine;

[ExecuteInEditMode]
public class ProceduralSkyboxLightDirection : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		Shader.SetGlobalVector("_SunDirection", base.transform.forward);
	}
}
