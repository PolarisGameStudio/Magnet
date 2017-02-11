﻿using UnityEngine;
using System.Collections;
using DarkTonic.MasterAudio;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using GameAnalyticsSDK;

public class SoundsManager : Singleton<SoundsManager> 
{
	public PlaylistController playlistCont;

	[Header ("Menu Sounds")]
	[SoundGroupAttribute]
	public string menuSubmit;
	[SoundGroupAttribute]
	public string menuCancel;
	[SoundGroupAttribute]
	public string menuNavigation;
	[SoundGroupAttribute]
	public string gameStartSound;
	[SoundGroupAttribute]
	public string openMenuSound;
	[SoundGroupAttribute]
	public string closeMenuSound;
	[SoundGroupAttribute]
	public string gamepadDisconnectionSound;
	[SoundGroupAttribute]
	public string winSound;

	[Header ("Player Sounds")]
	[SoundGroupAttribute]
	public string onHoldSound;
	[SoundGroupAttribute]
	public string shootSound;
	[SoundGroupAttribute]
	public string cubeHitSound;
	[SoundGroupAttribute]
	public string dashHitSound;
	[SoundGroupAttribute]
	public string stunONSound;
	[SoundGroupAttribute]
	public string stunOFFSound;
	[SoundGroupAttribute]
	public string stunENDSound;
	[SoundGroupAttribute]
	public string dashSound;
	[SoundGroupAttribute]
	public string deathSound;

	[Header ("Cube Sounds")]
	[SoundGroupAttribute]
	public string wallHitSound;
	[SoundGroupAttribute]
	public string cubeSpawnSound;
	[SoundGroupAttribute]
	public string lastSecondsSound;
	[SoundGroupAttribute]
	public string cubeTrackingSound;

	[Header ("Explosion Sound")]
	[SoundGroupAttribute]
	public string explosionSound;

	[Header ("Sounds Options")]
	public Scrollbar soundsBar;
	public Scrollbar playlistBar;
	public bool soundsMute = false;
	public Toggle soundsMuteToggle;
	public bool musicMute = false;
	public Toggle musicMuteToggle;
	[SoundGroupAttribute]
	public string soundsVolumeTest;

	[Header ("Slow Mo Effect")]
	public bool slowMoEffectMusicEnabled = false;
	public Toggle toggleEnableSloMoEffect;
	public GameObject playListSource;
	public float timeScaleRatio = 1;
	public float slowMotweenDuration;

	private float initialSoundsVolume;
	private float initialPlaylistVolume;

	private bool canPlaySoundTest = true;

	private float previousVolumeSounds;
	private float previousVolumePlaylist;

	private bool loading = false;

	void Start ()
	{
		initialSoundsVolume = MasterAudio.MasterVolumeLevel;
		initialPlaylistVolume = MasterAudio.PlaylistMasterVolume;

		if (PlayerPrefs.HasKey ("SlowMotionEffectEnable") && SceneManager.GetActiveScene().name != "Scene Testing")
			LoadPlayersPrefs ();

		else if(SceneManager.GetActiveScene().name != "Scene Testing")
		{
			toggleEnableSloMoEffect.isOn = false;
		}


		GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<SlowMotionCamera> ().OnAllSlowMotionStop += StopSlowMoEffect;

		GlobalVariables.Instance.OnStartMode += SetGamePlaylist;
		GlobalVariables.Instance.OnMenu += SetMenuPlaylist;
		GlobalVariables.Instance.OnEndMode += () => MasterAudio.PlaySound(winSound, 1, 1, 1.5f);

		//MasterAudio.StartPlaylist ("Game");
		//MasterAudio.TriggerRandomPlaylistClip ();
	}

	public void SetGamePlaylist ()
	{
		if(playlistCont.PlaylistName != "Game")
		{
			MasterAudio.ChangePlaylistByName ("Game", false);
			playlistCont.PlayRandomSong ();
		}
	}

	public void SetMenuPlaylist ()
	{
		if(playlistCont.PlaylistName != "Menu Ambient")
		{
			MasterAudio.ChangePlaylistByName ("Menu Ambient", true);
		}
	}

	public void MenuSubmit ()
	{
		MasterAudio.PlaySound (menuSubmit);
	}

	public void MenuCancel ()
	{
		MasterAudio.PlaySound (menuCancel);
	}

	public void MenuNavigation ()
	{
		MasterAudio.PlaySound (menuNavigation);
	}

	public void SetSoundsVolume ()
	{
		if(!loading)
		{			
			float volume = soundsBar.value * initialSoundsVolume;
			
			MasterAudio.MasterVolumeLevel = volume;
			
			if(canPlaySoundTest)
			{
				MasterAudio.PlaySound3DAtVector3 (soundsVolumeTest, Vector3.zero);
				StartCoroutine (PlaySoundWait ());
			}
		}		
	}

	IEnumerator PlaySoundWait ()
	{
		canPlaySoundTest = false;

		yield return new WaitForSeconds (0.35f);

		canPlaySoundTest = true;
	}
		
	public void SetPlaylistVolume ()
	{
		if(!loading)
		{
			float volume = playlistBar.value * initialPlaylistVolume;
			
			MasterAudio.PlaylistMasterVolume = volume;		
		}
	}

	public void ToggleMuteSounds ()
	{
		if(soundsMute == false)
		{
			soundsMute = true;
			previousVolumeSounds = soundsBar.value;
			DOTween.To(()=> soundsBar.value, x=> soundsBar.value =x, 0, 0.2f);
		}
		else
		{
			soundsMute = false;
			DOTween.To(()=> soundsBar.value, x=> soundsBar.value =x, previousVolumeSounds, 0.2f);
		}
	}

	public void ToggleMuteMusic ()
	{
		if(musicMute == false)
		{
			musicMute = true;
			previousVolumePlaylist = playlistBar.value;
			DOTween.To(()=> playlistBar.value, x=> playlistBar.value =x, 0, 0.2f);
		}
		else
		{
			musicMute = false;
			DOTween.To(()=> playlistBar.value, x=> playlistBar.value =x, previousVolumePlaylist, 0.2f);
		}
	}

	public void StartSlowMoEffect (float whichSlowFactor)
	{
		if(slowMoEffectMusicEnabled) 
		{
			AudioSource[] audioSourceComponents = playListSource.GetComponents<AudioSource> ();
			AudioSource playlistAudioSource = new AudioSource ();

			for (int i = 0; i < audioSourceComponents.Length; i++) {
				if (audioSourceComponents [i].volume != 0)
					playlistAudioSource = audioSourceComponents [i];
			}

			if(playlistAudioSource != null)
				playlistAudioSource.DOPitch ((playlistAudioSource.pitch / whichSlowFactor) * timeScaleRatio, slowMotweenDuration).SetEase (Ease.OutQuad);
		}
	}

	public void StopSlowMoEffect ()
	{
		if(slowMoEffectMusicEnabled) 
		{
			AudioSource[] audioSourceComponents = playListSource.GetComponents<AudioSource> ();
			AudioSource playlistAudioSource = new AudioSource ();

			for (int i = 0; i < audioSourceComponents.Length; i++) {
				if (audioSourceComponents [i].volume != 0)
					playlistAudioSource = audioSourceComponents [i];
			}

			playlistAudioSource.DOPitch (1, slowMotweenDuration).SetEase (Ease.OutQuad);
		}
	}

	public void ToggleSlowMoEffect ()
	{
		if(slowMoEffectMusicEnabled == true)
		{
			slowMoEffectMusicEnabled = false;
		}
		else
		{
			slowMoEffectMusicEnabled = true;
		}
	}

	public override void OnDestroy ()
	{
		if(SceneManager.GetActiveScene().name != "Scene Testing")
			SavePlayerPrefs ();
		
		//Debug.Log ("Data Saved");

		base.OnDestroy ();
	}

	void SavePlayerPrefs ()
	{
		Debug.Log ("Data Saved");

		PlayerPrefs.SetInt ("SlowMotionEffectEnable", slowMoEffectMusicEnabled ? 1 : 0);

		if(soundsMute)
		{
			PlayerPrefs.SetInt ("SoundsMute", 1);
			PlayerPrefs.SetFloat ("PreviousVolumeSounds", (float)previousVolumeSounds);
		}
		else
		{
			PlayerPrefs.SetInt ("SoundsMute", 0);
			PlayerPrefs.SetFloat ("SoundsVolume", (float)soundsBar.value);
		}

		if(musicMute)
		{
			PlayerPrefs.SetInt ("MusicMute", 1);
			PlayerPrefs.SetFloat ("PreviousVolumePlaylist", (float)previousVolumePlaylist);
		}
		else
		{
			PlayerPrefs.SetInt ("MusicMute", 0);
			PlayerPrefs.SetFloat ("MusicVolume", (float)playlistBar.value);
		}
	}

	void LoadPlayersPrefs ()
	{
		Debug.Log ("Data Loaded");
		loading = true;

		if(PlayerPrefs.GetInt ("SlowMotionEffectEnable") == 1)
		{
			slowMoEffectMusicEnabled = true;
			toggleEnableSloMoEffect.isOn = true;
		}
		else
		{
			slowMoEffectMusicEnabled = false;
			toggleEnableSloMoEffect.isOn = false;
		}

		if(PlayerPrefs.GetInt ("SoundsMute") == 1)
		{
			soundsMute = true;
			previousVolumeSounds = PlayerPrefs.GetFloat("PreviousVolumeSounds");
			soundsBar.value = 0;
			MasterAudio.MasterVolumeLevel = 0;
			soundsMuteToggle.isOn = true;
		}
		else
		{
			soundsMute = false;
			soundsBar.value = PlayerPrefs.GetFloat("SoundsVolume");
			MasterAudio.MasterVolumeLevel = PlayerPrefs.GetFloat("SoundsVolume");
			soundsMuteToggle.isOn = false;
		}
	
		if(PlayerPrefs.GetInt ("MusicMute") == 1)
		{
			musicMute = true;
			previousVolumePlaylist = PlayerPrefs.GetFloat("PreviousVolumePlaylist");
			playlistBar.value = 0;
			MasterAudio.PlaylistMasterVolume = 0;
			musicMuteToggle.isOn = true;
		}
		else
		{
			playlistBar.value = PlayerPrefs.GetFloat("MusicVolume");
			MasterAudio.PlaylistMasterVolume = PlayerPrefs.GetFloat("MusicVolume");
			musicMuteToggle.isOn = false;
		}

		loading = false;
	}

	void OnApplicationQuit ()
	{
		float soundsMuteTemp = soundsMute ? 1f : 0f;
		float musicMuteTemp = musicMute ? 1f : 0f;

		GameAnalytics.NewDesignEvent ("Menu:" + "Options:" + "Sounds:" + "SoundsMute", soundsMuteTemp);
		GameAnalytics.NewDesignEvent ("Menu:" + "Options:" + "Sounds:" + "MuteMusic", musicMuteTemp);
	}
}