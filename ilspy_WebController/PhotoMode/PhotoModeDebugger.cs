using UnityEngine;

namespace PhotoMode;

public class PhotoModeDebugger : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup[] cameraOffsetCanvases;

	[SerializeField]
	private CanvasGroup[] postProcessingCanvases;

	[SerializeField]
	private CanvasGroup[] filterCanvases;

	internal void PlayerAvailability(bool available)
	{
		if (!available)
		{
			DebugMessage("Player Object", null);
		}
	}

	internal void VolumeAvailability(bool available)
	{
		SetBlock(postProcessingCanvases, available);
		if (!available)
		{
			DebugMessage("Volume Component", postProcessingCanvases);
		}
	}

	internal void BlitAvailability(bool available)
	{
		SetBlock(filterCanvases, available);
		if (!available)
		{
			DebugMessage("Blit Asset", filterCanvases);
		}
	}

	internal void CameraOffsetAvailability(bool available)
	{
		SetBlock(cameraOffsetCanvases, available);
		if (!available)
		{
			DebugMessage("Camera Offset", cameraOffsetCanvases);
		}
	}

	private void SetBlock(CanvasGroup[] allCanvas, bool state)
	{
		foreach (CanvasGroup obj in allCanvas)
		{
			obj.interactable = state;
			obj.alpha = (state ? 1f : 0.2f);
		}
	}

	private void DebugMessage(string identifier, CanvasGroup[] allCanvas)
	{
		string text = "<b>Photo Mode Debug:</b> \n <b>" + identifier + "</b> is not set.";
		string text2 = string.Empty;
		if (allCanvas != null)
		{
			text2 = " UI block" + ((allCanvas.Length > 1) ? "s" : string.Empty) + " disabled: ";
			for (int i = 0; i < allCanvas.Length; i++)
			{
				text2 = text2 + ((i > 0) ? ", " : string.Empty) + "<b><i>" + allCanvas[i].name + "</i></b>";
			}
		}
		Debug.LogWarning(text + text2);
	}
}
