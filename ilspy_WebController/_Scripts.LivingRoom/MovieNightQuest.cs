using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.General;
using _Scripts.Spider;

namespace _Scripts.LivingRoom;

public class MovieNightQuest : MonoBehaviour
{
	[SerializeField]
	private TvLivingRoom tvLivingRoom;

	[SerializeField]
	private BodyMovement darling;

	[SerializeField]
	private BodyMovement honey;

	[SerializeField]
	private Transform darlingMovieTransform;

	[SerializeField]
	private Transform honeyMovieTransform;

	[SerializeField]
	[QuestPopup(false)]
	private string questName;

	[SerializeField]
	[VariablePopup(false)]
	private string coupleWatchingTvVariableName;

	private bool bookTowerFinished;

	private SnackForMovie snack;

	private void Start()
	{
		bookTowerFinished = false;
		snack = SnackForMovie.None;
		if (tvLivingRoom != null)
		{
			tvLivingRoom.OnMovieStarted += TvLivingRoom_OnMovieStarted;
			tvLivingRoom.OnMovieFinished += TvLivingRoom_OnMovieFinished;
			tvLivingRoom.OnMovieStopped += TvLivingRoom_OnMovieStopped;
		}
	}

	private void OnDestroy()
	{
		if (tvLivingRoom != null)
		{
			tvLivingRoom.OnMovieStarted -= TvLivingRoom_OnMovieStarted;
			tvLivingRoom.OnMovieFinished -= TvLivingRoom_OnMovieFinished;
			tvLivingRoom.OnMovieStopped -= TvLivingRoom_OnMovieStopped;
		}
	}

	private void MovieStarted()
	{
		if (bookTowerFinished && tvLivingRoom != null)
		{
			DialogueLua.SetVariable(coupleWatchingTvVariableName, true);
			darling.SetLookAtTargetTransform(tvLivingRoom.transform);
			honey.SetLookAtTargetTransform(tvLivingRoom.transform);
		}
	}

	private void MovieFinished()
	{
		if (bookTowerFinished)
		{
			DialogueLua.SetVariable(coupleWatchingTvVariableName, false);
			darling.SetLookAtTargetTransform(honey.transform);
			honey.SetLookAtTargetTransform(darling.transform);
			if (snack != 0)
			{
				QuestLog.SetQuestState(questName, QuestState.Success);
			}
		}
	}

	private void MovieStopped()
	{
		DialogueLua.SetVariable(coupleWatchingTvVariableName, false);
		darling.SetLookAtTargetTransform(honey.transform);
		honey.SetLookAtTargetTransform(darling.transform);
	}

	public void StartQuest()
	{
	}

	public void ChipsPlaced()
	{
		snack = SnackForMovie.Chips;
		DialogueLua.SetVariable("LivingRoom.Snack", "Chips");
	}

	public void ChocolatePlaced()
	{
		snack = SnackForMovie.Chocolate;
		DialogueLua.SetVariable("LivingRoom.Snack", "Chocolate");
	}

	public void PizzaPlaced()
	{
		snack = SnackForMovie.Pizza;
		DialogueLua.SetVariable("LivingRoom.Snack", "Pizza");
	}

	public void BookTowerFinished()
	{
		bookTowerFinished = true;
		darling.Respawn(darlingMovieTransform);
		honey.Respawn(honeyMovieTransform);
	}

	private void TvLivingRoom_OnMovieStarted()
	{
		MovieStarted();
	}

	private void TvLivingRoom_OnMovieFinished()
	{
		MovieFinished();
	}

	private void TvLivingRoom_OnMovieStopped()
	{
		MovieStopped();
	}
}
