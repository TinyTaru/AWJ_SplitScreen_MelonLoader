using UnityEngine.Rendering.Universal;

namespace _Scripts.Miscellaneous.Christmas;

public class CustomClearFeature : ScriptableRendererFeature
{
	private CustomClearPass customClearPass;

	public override void Create()
	{
		customClearPass = new CustomClearPass
		{
			renderPassEvent = RenderPassEvent.BeforeRenderingOpaques
		};
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(customClearPass);
	}
}
