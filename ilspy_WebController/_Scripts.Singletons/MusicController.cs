using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using _Scripts.General;

namespace _Scripts.Singletons;

public class MusicController : Singleton<MusicController>
{
	private struct BusStruct
	{
		public Bus Music;

		public Bus Master;

		public Bus Sound;

		public Bus SoundGame;

		public Bus UI;

		public VCA Dialogue;

		public Bus Trailer;
	}

	private struct MusicStruct
	{
		public EventInstance Menu;

		public EventInstance Hub;

		public EventInstance Tutorial;

		public EventInstance Kitchen;

		public EventInstance Office;

		public EventInstance KidsRoom;

		public EventInstance LivingRoom;

		public EventInstance IslandForest;

		public EventInstance IslandDesert;

		public EventInstance KitchenRadio;

		public EventInstance OfficeRadio;

		public EventInstance KidsRoomRadio;

		public EventInstance LivingRoomRadio;
	}

	private struct SnapshotStruct
	{
		public EventInstance Pause;

		public EventInstance MuteMusic;

		public EventInstance MuteMovies;

		public EventInstance LivingRoomRecordPlayerOn;

		public EventInstance LivingRoomTvOn;

		public EventInstance LivingRoomMinigameOn;
	}

	public struct GeneralSoundStruct
	{
		public EventInstance Respawn;

		public EventInstance FireHurt;

		public EventInstance UnderwaterLoop;

		public EventInstance WindLoop;
	}

	private struct SpeakSoundStruct
	{
		public EventInstance Spider;

		public EventInstance Ant;

		public EventInstance Bee;

		public EventInstance Fly;
	}

	[Header("Music References")]
	[SerializeField]
	private EventReference menuMusicRef;

	[SerializeField]
	private EventReference hubMusicRef;

	[SerializeField]
	private EventReference tutorialMusicRef;

	[SerializeField]
	private EventReference kitchenMusicRef;

	[SerializeField]
	private EventReference officeMusicRef;

	[SerializeField]
	private EventReference kidsRoomMusicRef;

	[SerializeField]
	private EventReference livingRoomMusicRef;

	[SerializeField]
	private EventReference islandForestMusicRef;

	[SerializeField]
	private EventReference islandDesertMusicRef;

	[Header("Radio Music References")]
	[SerializeField]
	private EventReference kitchenRadioMusicRef;

	[SerializeField]
	private EventReference officeRadioMusicRef;

	[SerializeField]
	private EventReference kidsRoomRadioMusicRef;

	[SerializeField]
	private EventReference livingRoomRadioMusicRef;

	[Header("One Shot Sound References")]
	[SerializeField]
	private EventReference clickSoundRef;

	[SerializeField]
	private EventReference selectSoundRef;

	[SerializeField]
	private EventReference pageChangeSoundRef;

	[SerializeField]
	private EventReference hatEquipSoundRef;

	[SerializeField]
	private EventReference accessoryEquipSoundRef;

	[SerializeField]
	private EventReference shoeEquipSoundRef;

	[SerializeField]
	private EventReference eyeEquipSoundRef;

	[Header("Permanent Sound References")]
	[SerializeField]
	private EventReference respawnSoundRef;

	[SerializeField]
	private EventReference fireHurtSoundRef;

	[FormerlySerializedAs("underwaterAmpienceLoopRef")]
	[SerializeField]
	private EventReference underwaterAmbienceLoopRef;

	[SerializeField]
	private EventReference windLoopRef;

	[Header("Speak Sound References")]
	[SerializeField]
	private EventReference spiderSpeakLoopRef;

	[SerializeField]
	private EventReference antSpeakLoopRef;

	[SerializeField]
	private EventReference beeSpeakLoopRef;

	[SerializeField]
	private EventReference flySpeakLoopRef;

	[Header("Snapshot References")]
	[SerializeField]
	private EventReference pauseSnapshotRef;

	[SerializeField]
	private EventReference muteMusicSoundRef;

	[SerializeField]
	private EventReference muteMoviesSoundRef;

	[SerializeField]
	private EventReference livingRoomRecordPlayerOnSoundRef;

	[SerializeField]
	private EventReference livingRoomTvOnSoundRef;

	[SerializeField]
	private EventReference livingRoomMiniGameOnSoundRef;

	private BusStruct busList;

	private MusicStruct musicList;

	private SnapshotStruct snapshotList;

	private GeneralSoundStruct generalSoundList;

	private SpeakSoundStruct speakSounds;

	public GeneralSoundStruct GeneralSoundList => generalSoundList;

	protected override void Awake()
	{
		base.Awake();
		if (Singleton<MusicController>.Instance == this)
		{
			Initialize();
		}
	}

	private void Start()
	{
		SetHalloweenParameter(0f);
		SetChristmasParameter(0f);
	}

	private void OnEnable()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		}
		SceneManager.sceneLoaded += SceneManager_OnSceneLoaded;
	}

	private void OnDisable()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		busList.Music.setPaused(!hasFocus);
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		busList.Music.setPaused(pauseStatus);
	}

	public void ClickSound()
	{
		RuntimeManager.CreateInstance(clickSoundRef).start();
	}

	public void SelectSound()
	{
		RuntimeManager.CreateInstance(selectSoundRef).start();
	}

	public void PageChangeSound()
	{
		EventInstance eventInstance = RuntimeManager.CreateInstance(pageChangeSoundRef);
		eventInstance.setParameterByName("menu_index", 1f);
		eventInstance.start();
	}

	public EventInstance PlaySound(EventReference soundReference)
	{
		EventInstance result = RuntimeManager.CreateInstance(soundReference);
		result.start();
		return result;
	}

	public void PlaySound(string soundName)
	{
		RuntimeManager.CreateInstance(soundName).start();
	}

	public void PlayOneShot3D(EventReference soundReference, Vector3 position)
	{
		RuntimeManager.PlayOneShot(soundReference, position);
	}

	public void PlaySoundNoReturn(string soundName)
	{
		PlaySound(soundName);
	}

	public void PlaySpeakSound(string soundName)
	{
		EventInstance eventInstance = soundName.ToLower() switch
		{
			"ant" => Singleton<MusicController>.Instance.speakSounds.Ant, 
			"bee" => Singleton<MusicController>.Instance.speakSounds.Bee, 
			"fly" => Singleton<MusicController>.Instance.speakSounds.Fly, 
			_ => Singleton<MusicController>.Instance.speakSounds.Spider, 
		};
		eventInstance.getPlaybackState(out var state);
		if (state != 0)
		{
			eventInstance.start();
		}
	}

	public static void StopAllSpeakSounds()
	{
		Singleton<MusicController>.Instance.speakSounds.Ant.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		Singleton<MusicController>.Instance.speakSounds.Bee.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		Singleton<MusicController>.Instance.speakSounds.Fly.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		Singleton<MusicController>.Instance.speakSounds.Spider.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	public void PlayHatSound(string hatType)
	{
		EventInstance eventInstance = RuntimeManager.CreateInstance(hatEquipSoundRef);
		eventInstance.setParameterByNameWithLabel("hat_type", hatType);
		eventInstance.start();
	}

	public void PlayAccessorySound(string accessoryType)
	{
		EventInstance eventInstance = RuntimeManager.CreateInstance(accessoryEquipSoundRef);
		eventInstance.setParameterByNameWithLabel("accessory_type", accessoryType);
		eventInstance.start();
	}

	public void PlayShoeSound(string shoeType)
	{
		EventInstance eventInstance = RuntimeManager.CreateInstance(shoeEquipSoundRef);
		eventInstance.setParameterByNameWithLabel("shoe_type", shoeType);
		eventInstance.start();
	}

	public void PlayEyeSound(string eyeType)
	{
	}

	public void StartWindLoop()
	{
		Singleton<MusicController>.Instance.generalSoundList.WindLoop.getPlaybackState(out var state);
		if (state == PLAYBACK_STATE.STOPPED)
		{
			Singleton<MusicController>.Instance.generalSoundList.WindLoop.start();
		}
	}

	public void StopWindLoop()
	{
		Singleton<MusicController>.Instance.generalSoundList.WindLoop.getPlaybackState(out var state);
		if (state == PLAYBACK_STATE.PLAYING)
		{
			Singleton<MusicController>.Instance.generalSoundList.WindLoop.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
	}

	public void SetWindLoopVolume(float volume)
	{
		Singleton<MusicController>.Instance.generalSoundList.WindLoop.setParameterByName("volume", volume);
	}

	public void SetWindLoopSpeed(float speed)
	{
		Singleton<MusicController>.Instance.generalSoundList.WindLoop.setParameterByName("wind_speed", speed);
	}

	public void SetHalloweenParameter(float value)
	{
		RuntimeManager.StudioSystem.setParameterByName("halloween", value);
	}

	public void SetChristmasParameter(float value)
	{
		RuntimeManager.StudioSystem.setParameterByName("christmas", value);
	}

	private void Initialize()
	{
		InitializeBusList();
		InitializeMusicList();
		InitializeSnapShotList();
		InitializeGeneralSounds();
		InitializeSpeakSounds();
	}

	public void Pause()
	{
		snapshotList.Pause.start();
		busList.SoundGame.setPaused(paused: true);
	}

	public void Continue()
	{
		Singleton<MusicController>.Instance.snapshotList.Pause.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		Singleton<MusicController>.Instance.busList.SoundGame.setPaused(paused: false);
	}

	private void InitializeBusList()
	{
		Singleton<MusicController>.Instance.busList.Master = RuntimeManager.GetBus("bus:/");
		Singleton<MusicController>.Instance.busList.Music = RuntimeManager.GetBus("bus:/music");
		Singleton<MusicController>.Instance.busList.Sound = RuntimeManager.GetBus("bus:/sounds");
		Singleton<MusicController>.Instance.busList.SoundGame = RuntimeManager.GetBus("bus:/sounds/game");
		Singleton<MusicController>.Instance.busList.UI = RuntimeManager.GetBus("bus:/ui");
		Singleton<MusicController>.Instance.busList.Dialogue = RuntimeManager.GetVCA("vca:/dialog_volume");
		Singleton<MusicController>.Instance.busList.Trailer = RuntimeManager.GetBus("bus:/idle_trailer");
		UpdateBusList();
	}

	private void UpdateBusList()
	{
		Singleton<MusicController>.Instance.busList.Master.setVolume(SettingsController.MasterVolume);
		Singleton<MusicController>.Instance.busList.Music.setVolume(SettingsController.MusicVolume);
		Singleton<MusicController>.Instance.busList.Sound.setVolume(SettingsController.SoundVolume);
		Singleton<MusicController>.Instance.busList.UI.setVolume(SettingsController.UiVolume);
		Singleton<MusicController>.Instance.busList.Dialogue.setVolume(SettingsController.DialogueVolume);
		Singleton<MusicController>.Instance.busList.Trailer.setVolume(SettingsController.TrailerVolume);
	}

	private void InitializeMusicList()
	{
		Singleton<MusicController>.Instance.musicList.Menu = RuntimeManager.CreateInstance(menuMusicRef);
		Singleton<MusicController>.Instance.musicList.Hub = RuntimeManager.CreateInstance(hubMusicRef);
		Singleton<MusicController>.Instance.musicList.Tutorial = RuntimeManager.CreateInstance(tutorialMusicRef);
		Singleton<MusicController>.Instance.musicList.Kitchen = RuntimeManager.CreateInstance(kitchenMusicRef);
		Singleton<MusicController>.Instance.musicList.Office = RuntimeManager.CreateInstance(officeMusicRef);
		Singleton<MusicController>.Instance.musicList.KidsRoom = RuntimeManager.CreateInstance(kidsRoomMusicRef);
		Singleton<MusicController>.Instance.musicList.LivingRoom = RuntimeManager.CreateInstance(livingRoomMusicRef);
		Singleton<MusicController>.Instance.musicList.IslandForest = RuntimeManager.CreateInstance(islandForestMusicRef);
		Singleton<MusicController>.Instance.musicList.IslandDesert = RuntimeManager.CreateInstance(islandDesertMusicRef);
		Singleton<MusicController>.Instance.musicList.KitchenRadio = RuntimeManager.CreateInstance(kitchenRadioMusicRef);
		Singleton<MusicController>.Instance.musicList.OfficeRadio = RuntimeManager.CreateInstance(officeRadioMusicRef);
		Singleton<MusicController>.Instance.musicList.KidsRoomRadio = RuntimeManager.CreateInstance(kidsRoomRadioMusicRef);
		Singleton<MusicController>.Instance.musicList.LivingRoomRadio = RuntimeManager.CreateInstance(livingRoomRadioMusicRef);
	}

	private void PlayNewTrack(EventInstance musicInstance)
	{
		StopMusic(new List<EventInstance> { musicInstance });
		musicInstance.getPlaybackState(out var state);
		if (state != 0)
		{
			musicInstance.start();
		}
	}

	private void StopMusic(List<EventInstance> exceptions)
	{
		if (!exceptions.Contains(Singleton<MusicController>.Instance.musicList.Menu))
		{
			Singleton<MusicController>.Instance.musicList.Menu.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
		if (!exceptions.Contains(Singleton<MusicController>.Instance.musicList.Hub))
		{
			Singleton<MusicController>.Instance.musicList.Hub.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
		if (!exceptions.Contains(Singleton<MusicController>.Instance.musicList.Tutorial))
		{
			Singleton<MusicController>.Instance.musicList.Tutorial.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
		if (!exceptions.Contains(Singleton<MusicController>.Instance.musicList.Kitchen))
		{
			Singleton<MusicController>.Instance.musicList.Kitchen.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
		if (!exceptions.Contains(Singleton<MusicController>.Instance.musicList.Office))
		{
			Singleton<MusicController>.Instance.musicList.Office.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
		if (!exceptions.Contains(Singleton<MusicController>.Instance.musicList.KidsRoom))
		{
			Singleton<MusicController>.Instance.musicList.KidsRoom.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
		if (!exceptions.Contains(Singleton<MusicController>.Instance.musicList.LivingRoom))
		{
			Singleton<MusicController>.Instance.musicList.LivingRoom.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
		if (!exceptions.Contains(Singleton<MusicController>.Instance.musicList.IslandForest))
		{
			Singleton<MusicController>.Instance.musicList.IslandForest.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
		if (!exceptions.Contains(Singleton<MusicController>.Instance.musicList.IslandDesert))
		{
			Singleton<MusicController>.Instance.musicList.IslandDesert.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
	}

	public void StartMusic(LevelMusic levelMusic)
	{
		Continue();
		switch (levelMusic)
		{
		case LevelMusic.Menu:
			PlayNewTrack(Singleton<MusicController>.Instance.musicList.Menu);
			break;
		case LevelMusic.Hub:
			PlayNewTrack(Singleton<MusicController>.Instance.musicList.Hub);
			break;
		case LevelMusic.Tutorial:
			PlayNewTrack(Singleton<MusicController>.Instance.musicList.Tutorial);
			break;
		case LevelMusic.Kitchen:
			PlayNewTrack(Singleton<MusicController>.Instance.musicList.Kitchen);
			break;
		case LevelMusic.Office:
			PlayNewTrack(Singleton<MusicController>.Instance.musicList.Office);
			break;
		case LevelMusic.KidsRoom:
			PlayNewTrack(Singleton<MusicController>.Instance.musicList.KidsRoom);
			break;
		case LevelMusic.LivingRoom:
			PlayNewTrack(Singleton<MusicController>.Instance.musicList.LivingRoom);
			break;
		case LevelMusic.IslandForest:
			PlayNewTrack(Singleton<MusicController>.Instance.musicList.IslandForest);
			break;
		case LevelMusic.IslandDesert:
			PlayNewTrack(Singleton<MusicController>.Instance.musicList.IslandDesert);
			break;
		default:
			throw new ArgumentOutOfRangeException("levelMusic", levelMusic, null);
		}
	}

	public void StartRadioMusic()
	{
		if (!(Singleton<GameController>.Instance == null))
		{
			switch (Singleton<GameController>.Instance.Music)
			{
			case LevelMusic.Kitchen:
				Singleton<MusicController>.Instance.musicList.KitchenRadio.start();
				break;
			case LevelMusic.Office:
				Singleton<MusicController>.Instance.musicList.OfficeRadio.start();
				break;
			case LevelMusic.KidsRoom:
				Singleton<MusicController>.Instance.musicList.KidsRoomRadio.start();
				break;
			case LevelMusic.LivingRoom:
				Singleton<MusicController>.Instance.musicList.LivingRoomRadio.start();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case LevelMusic.Menu:
			case LevelMusic.Hub:
			case LevelMusic.Tutorial:
			case LevelMusic.IslandForest:
			case LevelMusic.IslandDesert:
				break;
			}
		}
	}

	public void StopRadioMusic()
	{
		Singleton<MusicController>.Instance.musicList.KitchenRadio.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		Singleton<MusicController>.Instance.musicList.OfficeRadio.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		Singleton<MusicController>.Instance.musicList.KidsRoomRadio.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	public void StopMusicTrack(LevelMusic levelMusic)
	{
		switch (levelMusic)
		{
		case LevelMusic.Menu:
			Singleton<MusicController>.Instance.musicList.Menu.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			break;
		case LevelMusic.Hub:
			Singleton<MusicController>.Instance.musicList.Hub.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			break;
		case LevelMusic.Tutorial:
			Singleton<MusicController>.Instance.musicList.Tutorial.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			break;
		case LevelMusic.Kitchen:
			Singleton<MusicController>.Instance.musicList.Kitchen.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			break;
		case LevelMusic.Office:
			Singleton<MusicController>.Instance.musicList.Office.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			break;
		case LevelMusic.KidsRoom:
			Singleton<MusicController>.Instance.musicList.KidsRoom.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			break;
		case LevelMusic.LivingRoom:
			Singleton<MusicController>.Instance.musicList.LivingRoom.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			break;
		case LevelMusic.IslandForest:
			Singleton<MusicController>.Instance.musicList.IslandForest.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			break;
		case LevelMusic.IslandDesert:
			Singleton<MusicController>.Instance.musicList.IslandDesert.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			break;
		default:
			throw new ArgumentOutOfRangeException("levelMusic", levelMusic, null);
		}
	}

	public void StopMusic()
	{
		Singleton<MusicController>.Instance.musicList.Menu.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	public void SetRadioVolume(float value)
	{
		if (!(Singleton<GameController>.Instance == null))
		{
			switch (Singleton<GameController>.Instance.Music)
			{
			case LevelMusic.Kitchen:
				Singleton<MusicController>.Instance.musicList.KitchenRadio.setParameterByName("radio_volume", value);
				break;
			case LevelMusic.Office:
				Singleton<MusicController>.Instance.musicList.OfficeRadio.setParameterByName("radio_volume", value);
				break;
			case LevelMusic.KidsRoom:
				Singleton<MusicController>.Instance.musicList.KidsRoomRadio.setParameterByName("radio_volume", value);
				break;
			case LevelMusic.LivingRoom:
				Singleton<MusicController>.Instance.musicList.LivingRoomRadio.setParameterByName("radio_volume", value);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case LevelMusic.Menu:
			case LevelMusic.Hub:
			case LevelMusic.Tutorial:
			case LevelMusic.IslandForest:
			case LevelMusic.IslandDesert:
				break;
			}
		}
	}

	public void SetRadioFrequency(float value)
	{
		if (!(Singleton<GameController>.Instance == null))
		{
			switch (Singleton<GameController>.Instance.Music)
			{
			case LevelMusic.Kitchen:
				Singleton<MusicController>.Instance.musicList.KitchenRadio.setParameterByName("radio_frequency", value);
				break;
			case LevelMusic.Office:
				Singleton<MusicController>.Instance.musicList.OfficeRadio.setParameterByName("radio_frequency", value);
				break;
			case LevelMusic.KidsRoom:
				Singleton<MusicController>.Instance.musicList.KidsRoomRadio.setParameterByName("radio_frequency", value);
				break;
			case LevelMusic.LivingRoom:
				Singleton<MusicController>.Instance.musicList.LivingRoomRadio.setParameterByName("radio_frequency", value);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case LevelMusic.Menu:
			case LevelMusic.Hub:
			case LevelMusic.Tutorial:
			case LevelMusic.IslandForest:
			case LevelMusic.IslandDesert:
				break;
			}
		}
	}

	public void SetKitchenWebCount(float value)
	{
		Singleton<MusicController>.Instance.musicList.Kitchen.setParameterByName("web_count", value / 200f);
	}

	private void InitializeSnapShotList()
	{
		Singleton<MusicController>.Instance.snapshotList.Pause = RuntimeManager.CreateInstance(pauseSnapshotRef);
		Singleton<MusicController>.Instance.snapshotList.MuteMusic = RuntimeManager.CreateInstance(muteMusicSoundRef);
		Singleton<MusicController>.Instance.snapshotList.MuteMovies = RuntimeManager.CreateInstance(muteMoviesSoundRef);
		Singleton<MusicController>.Instance.snapshotList.LivingRoomRecordPlayerOn = RuntimeManager.CreateInstance(livingRoomRecordPlayerOnSoundRef);
		Singleton<MusicController>.Instance.snapshotList.LivingRoomTvOn = RuntimeManager.CreateInstance(livingRoomTvOnSoundRef);
		Singleton<MusicController>.Instance.snapshotList.LivingRoomMinigameOn = RuntimeManager.CreateInstance(livingRoomMiniGameOnSoundRef);
	}

	public void SetLivingRoomRecordPlayerOnState(bool isOn)
	{
		if (isOn)
		{
			Singleton<MusicController>.Instance.snapshotList.LivingRoomRecordPlayerOn.getPlaybackState(out var state);
			if (state != 0)
			{
				Singleton<MusicController>.Instance.snapshotList.LivingRoomRecordPlayerOn.start();
			}
		}
		else
		{
			Singleton<MusicController>.Instance.snapshotList.LivingRoomRecordPlayerOn.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
	}

	public void SetLivingRoomTvOnState(bool isOn)
	{
		if (isOn)
		{
			Singleton<MusicController>.Instance.snapshotList.LivingRoomTvOn.getPlaybackState(out var state);
			if (state != 0)
			{
				Singleton<MusicController>.Instance.snapshotList.LivingRoomTvOn.start();
			}
		}
		else
		{
			Singleton<MusicController>.Instance.snapshotList.LivingRoomTvOn.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
	}

	public void SetLivingRoomMiniGameOnState(bool isOn)
	{
		if (isOn)
		{
			Singleton<MusicController>.Instance.snapshotList.LivingRoomMinigameOn.getPlaybackState(out var state);
			if (state != 0)
			{
				Singleton<MusicController>.Instance.snapshotList.LivingRoomMinigameOn.start();
			}
		}
		else
		{
			Singleton<MusicController>.Instance.snapshotList.LivingRoomMinigameOn.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
	}

	private void InitializeGeneralSounds()
	{
		Singleton<MusicController>.Instance.generalSoundList.Respawn = RuntimeManager.CreateInstance(respawnSoundRef);
		Singleton<MusicController>.Instance.generalSoundList.FireHurt = RuntimeManager.CreateInstance(fireHurtSoundRef);
		Singleton<MusicController>.Instance.generalSoundList.UnderwaterLoop = RuntimeManager.CreateInstance(underwaterAmbienceLoopRef);
		Singleton<MusicController>.Instance.generalSoundList.WindLoop = RuntimeManager.CreateInstance(windLoopRef);
	}

	private void InitializeSpeakSounds()
	{
		Singleton<MusicController>.Instance.speakSounds.Spider = RuntimeManager.CreateInstance(spiderSpeakLoopRef);
		Singleton<MusicController>.Instance.speakSounds.Ant = RuntimeManager.CreateInstance(antSpeakLoopRef);
		Singleton<MusicController>.Instance.speakSounds.Bee = RuntimeManager.CreateInstance(beeSpeakLoopRef);
		Singleton<MusicController>.Instance.speakSounds.Fly = RuntimeManager.CreateInstance(flySpeakLoopRef);
	}

	private void StopAllSounds()
	{
		Singleton<MusicController>.Instance.busList.SoundGame.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		Singleton<MusicController>.Instance.busList.Sound.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	public void StartUnderwater()
	{
		Singleton<MusicController>.Instance.generalSoundList.UnderwaterLoop.start();
		Singleton<MusicController>.Instance.generalSoundList.UnderwaterLoop.setVolume(0.5f);
	}

	public void StopUnderwater()
	{
		Singleton<MusicController>.Instance.generalSoundList.UnderwaterLoop.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	public void MuteSoundBus(bool mute)
	{
		Singleton<MusicController>.Instance.busList.Sound.setMute(mute);
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		UpdateBusList();
	}

	private void SceneManager_OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		SetHalloweenParameter(0f);
		SetChristmasParameter(0f);
		StopAllSounds();
		MuteSoundBus(SceneManager.GetActiveScene().name == "MainMenu");
	}
}
