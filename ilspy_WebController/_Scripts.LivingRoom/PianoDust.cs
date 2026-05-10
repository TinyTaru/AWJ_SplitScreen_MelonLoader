using System.Collections;
using UnityEngine;

namespace _Scripts.LivingRoom;

public class PianoDust : MonoBehaviour
{
	[SerializeField]
	private RenderTexture renderTexture;

	[SerializeField]
	private UnityEngine.Camera dustCamera;

	public RenderTexture Texture => renderTexture;

	private void Awake()
	{
		dustCamera.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
		dustCamera.targetTexture = renderTexture;
		Shader.SetGlobalTexture("_GlobalEffectRT", renderTexture);
		Shader.SetGlobalFloat("_OrthographicCamSize", dustCamera.orthographicSize);
		Shader.SetGlobalVector("_Position", dustCamera.transform.position);
	}

	private void Start()
	{
		StartCoroutine(ResetDustCoroutine());
	}

	private IEnumerator ResetDustCoroutine()
	{
		dustCamera.backgroundColor = Color.clear;
		dustCamera.clearFlags = CameraClearFlags.Color;
		yield return null;
		dustCamera.clearFlags = CameraClearFlags.Nothing;
	}
}
