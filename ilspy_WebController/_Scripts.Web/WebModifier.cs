using UnityEngine;

namespace _Scripts.Web;

public class WebModifier : MonoBehaviour
{
	[Header("Color Modification")]
	[SerializeField]
	private bool modifyColor;

	[SerializeField]
	private WebThread webThread;

	[SerializeField]
	private Color color;

	[Header("Thickness Modification")]
	[SerializeField]
	private bool modifyThickness;

	[SerializeField]
	private float thickness;

	[Header("Web Sound Modification")]
	[SerializeField]
	private bool suppressAttachSound;

	public bool HasColorModification(out WebThread modifiedWebThread, out Color modifiedColor)
	{
		modifiedWebThread = webThread;
		modifiedColor = color;
		return modifyColor;
	}

	public bool HasThicknessModification(out float modifiedThickness)
	{
		modifiedThickness = thickness;
		return modifyThickness;
	}

	public bool SuppressAttachSound()
	{
		return suppressAttachSound;
	}

	public void SetSuppressAttachSound(bool value)
	{
		suppressAttachSound = value;
	}
}
