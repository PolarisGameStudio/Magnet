﻿// Audio spectrum component
// By Keijiro Takahashi, 2013
// https://github.com/keijiro/unity-audio-spectrum
using UnityEngine;
using System.Collections;
using DarkTonic.MasterAudio;

public class AudioSpectrum : Singleton<AudioSpectrum>
{
	public PlaylistController playlistController;
	private AudioSource audioSource;

    #region Band type definition
    public enum BandType 
	{
        FourBand,
        FourBandVisual,
        EightBand,
        TenBand,
        TwentySixBand,
        ThirtyOneBand
    };

	public enum LevelsType
	{
		Basic,
		Peak,
		Mean
	};

    static float[][] middleFrequenciesForBands = 
	{
        new float[]{ 125.0f, 500, 1000, 2000 },
        new float[]{ 250.0f, 400, 600, 800 },
        new float[]{ 63.0f, 125, 500, 1000, 2000, 4000, 6000, 8000 },
        new float[]{ 31.5f, 63, 125, 250, 500, 1000, 2000, 4000, 8000, 16000 },
        new float[]{ 25.0f, 31.5f, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000 },
        new float[]{ 20.0f, 25, 31.5f, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000, 10000, 12500, 16000, 20000 },
    };
    static float[] bandwidthForBands = 
	{
        1.414f, // 2^(1/2)
        1.260f, // 2^(1/3)
        1.414f, // 2^(1/2)
        1.414f, // 2^(1/2)
        1.122f, // 2^(1/6)
        1.122f  // 2^(1/6)
    };
    #endregion

    #region Public variables
    public int numberOfSamples = 1024;
    public BandType bandType = BandType.TenBand;
    public float fallSpeed = 0.08f;
    public float sensibility = 8.0f;
    #endregion

    #region Private variables
	public float[] rawSpectrum;
	public float[] levels;
	public float[] peakLevels;
	public float[] meanLevels;

	public float[] levelsNormalized;
	public float[] peakLevelsNormalized;
	public float[] meanLevelsNormalized;

	public float[] levelsHighest;
	public float[] peakLevelsHighest;
	public float[] meanLevelsHighest;
    #endregion

    #region Public property
    public float[] Levels 
	{
        get { return levels; }
    }

    public float[] PeakLevels 
	{
        get { return peakLevels; }
    }
    
    public float[] MeanLevels 
	{
        get { return meanLevels; }
    }


	public float[] LevelsNormalized 
	{
		get { return levelsNormalized; }
	}

	public float[] PeakLevelsNormalized 
	{
		get { return peakLevelsNormalized; }
	}

	public float[] MeanLevelsNormalized 
	{
		get { return meanLevelsNormalized; }
	}
    #endregion

	void Awake ()
    {
		if (playlistController == null)
			playlistController = FindObjectOfType<PlaylistController> ();

		audioSource = playlistController.ActiveAudioSource;
		playlistController.SongChanged += OnSongChanged;

        CheckBuffers ();

		StartCoroutine (Spectrum ());
    }

	IEnumerator Spectrum ()
	{
		while(true)
		{
			CheckBuffers ();
			
			audioSource.GetSpectrumData (rawSpectrum, 0, FFTWindow.BlackmanHarris);
			
			float[] middlefrequencies = middleFrequenciesForBands [(int)bandType];
			var bandwidth = bandwidthForBands [(int)bandType];
			
			var falldown = fallSpeed * Time.unscaledDeltaTime;
			var filter = Mathf.Exp (-sensibility * Time.unscaledDeltaTime);
			
			for (var bi = 0; bi < levels.Length; bi++) 
			{
				int imin = FrequencyToSpectrumIndex (middlefrequencies [bi] / bandwidth);
				int imax = FrequencyToSpectrumIndex (middlefrequencies [bi] * bandwidth);
				
				var bandMax = 0.0f;
				for (var fi = imin; fi <= imax; fi++) 
				{
					bandMax = Mathf.Max (bandMax, rawSpectrum [fi]);
				}
				
				levels [bi] = bandMax;
				peakLevels [bi] = Mathf.Max (peakLevels [bi] - falldown, bandMax);
				meanLevels [bi] = bandMax - (bandMax - meanLevels [bi]) * filter;
				
				//Highest
				if (levels [bi] > levelsHighest [bi])
					levelsHighest [bi] = levels [bi];
				
				if (peakLevels [bi] > peakLevelsHighest [bi])
					peakLevelsHighest [bi] = peakLevels [bi];
				
				if (meanLevels [bi] > meanLevelsHighest [bi])
					meanLevelsHighest [bi] = meanLevels [bi];
				
				//Normalized Levels
				levelsNormalized [bi] = levels [bi] / levelsHighest [bi];
				peakLevelsNormalized [bi] = peakLevels [bi] / peakLevelsHighest [bi];
				meanLevelsNormalized [bi] = meanLevels [bi] / meanLevelsHighest [bi];
			}

			yield return new WaitForEndOfFrame ();
			//yield return new WaitForEndOfFrame ();
		}

	}

    void Update ()
    {
		return;
		
        CheckBuffers ();

		audioSource.GetSpectrumData (rawSpectrum, 0, FFTWindow.BlackmanHarris);

        float[] middlefrequencies = middleFrequenciesForBands [(int)bandType];
        var bandwidth = bandwidthForBands [(int)bandType];

		var falldown = fallSpeed * Time.unscaledDeltaTime;
		var filter = Mathf.Exp (-sensibility * Time.unscaledDeltaTime);

        for (var bi = 0; bi < levels.Length; bi++) 
		{
            int imin = FrequencyToSpectrumIndex (middlefrequencies [bi] / bandwidth);
            int imax = FrequencyToSpectrumIndex (middlefrequencies [bi] * bandwidth);

            var bandMax = 0.0f;
            for (var fi = imin; fi <= imax; fi++) 
			{
                bandMax = Mathf.Max (bandMax, rawSpectrum [fi]);
            }

            levels [bi] = bandMax;
            peakLevels [bi] = Mathf.Max (peakLevels [bi] - falldown, bandMax);
            meanLevels [bi] = bandMax - (bandMax - meanLevels [bi]) * filter;

			//Highest
			if (levels [bi] > levelsHighest [bi])
				levelsHighest [bi] = levels [bi];

			if (peakLevels [bi] > peakLevelsHighest [bi])
				peakLevelsHighest [bi] = peakLevels [bi];
			
			if (meanLevels [bi] > meanLevelsHighest [bi])
				meanLevelsHighest [bi] = meanLevels [bi];

			//Normalized Levels
			levelsNormalized [bi] = levels [bi] / levelsHighest [bi];
			peakLevelsNormalized [bi] = peakLevels [bi] / peakLevelsHighest [bi];
			meanLevelsNormalized [bi] = meanLevels [bi] / meanLevelsHighest [bi];
        }
    }

	void CheckBuffers ()
	{
		if (rawSpectrum == null || rawSpectrum.Length != numberOfSamples) 
		{
			rawSpectrum = new float[numberOfSamples];
		}

		var bandCount = middleFrequenciesForBands [(int)bandType].Length;
		if (levels == null || levels.Length != bandCount) 
		{
			levels = new float[bandCount];
			peakLevels = new float[bandCount];
			meanLevels = new float[bandCount];

			levelsHighest = new float[bandCount];
			peakLevelsHighest = new float[bandCount];
			meanLevelsHighest = new float[bandCount];

			levelsNormalized = new float[bandCount];
			peakLevelsNormalized = new float[bandCount];
			meanLevelsNormalized = new float[bandCount];

			for(int i = 0; i < bandCount; i++)
			{
				levelsHighest [i] = 0.0001f;
				peakLevelsHighest [i] = 0.0001f;
				meanLevelsHighest [i] = 0.0001f;
			}
		}
	}

	int FrequencyToSpectrumIndex (float f)
	{
		var i = Mathf.FloorToInt (f / AudioSettings.outputSampleRate * 2.0f * rawSpectrum.Length);
		return Mathf.Clamp (i, 0, rawSpectrum.Length - 1);
	}

	void OnSongChanged (string songName)
	{
		audioSource = playlistController.ActiveAudioSource;
	}
}