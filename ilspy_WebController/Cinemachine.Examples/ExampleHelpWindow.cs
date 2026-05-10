using UnityEngine;

namespace Cinemachine.Examples;

[AddComponentMenu("")]
public class ExampleHelpWindow : MonoBehaviour
{
	public string m_Title;

	[TextArea(10, 50)]
	public string m_Description;

	private bool mShowingHelpWindow = true;

	private const float kPadding = 40f;

	private void OnGUI()
	{
		if (mShowingHelpWindow)
		{
			Vector2 vector = GUI.skin.label.CalcSize(new GUIContent(m_Description));
			Vector2 vector2 = vector * 0.5f;
			float maxWidth = Mathf.Min((float)Screen.width - 40f, vector.x);
			float x = (float)Screen.width * 0.5f - maxWidth * 0.5f;
			float y = (float)Screen.height * 0.4f - vector2.y;
			Rect screenRect = new Rect(x, y, maxWidth, vector.y);
			GUILayout.Window(400, screenRect, delegate(int id)
			{
				DrawWindow(id, maxWidth);
			}, m_Title);
		}
	}

	private void DrawWindow(int id, float maxWidth)
	{
		GUILayout.BeginVertical(GUI.skin.box);
		GUILayout.Label(m_Description);
		GUILayout.EndVertical();
		if (GUILayout.Button("Got it!"))
		{
			mShowingHelpWindow = false;
		}
	}
}
