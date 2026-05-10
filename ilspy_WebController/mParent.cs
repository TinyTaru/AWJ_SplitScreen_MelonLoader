using UnityEngine;
using UnityEngine.Animations.Rigging;

public class mParent : MonoBehaviour
{
	private enum Mode
	{
		Idle,
		Ground,
		Hand,
		Back
	}

	public GameObject mParentCon;

	private Mode m_Mode;

	public void Update()
	{
		if (m_Mode != 0)
		{
			MultiParentConstraint component = mParentCon.GetComponent<MultiParentConstraint>();
			WeightedTransformArray sourceObjects = component.data.sourceObjects;
			sourceObjects.SetWeight(0, (m_Mode == Mode.Ground) ? 1f : 0f);
			sourceObjects.SetWeight(1, (m_Mode == Mode.Hand) ? 1f : 0f);
			sourceObjects.SetWeight(2, (m_Mode == Mode.Back) ? 1f : 0f);
			component.data.sourceObjects = sourceObjects;
			m_Mode = Mode.Idle;
		}
	}

	public void Start()
	{
		m_Mode = Mode.Ground;
		Debug.Log("ground");
	}

	public void hand()
	{
		m_Mode = Mode.Hand;
		Debug.Log("hand");
	}

	public void back()
	{
		m_Mode = Mode.Back;
		Debug.Log("back");
	}
}
