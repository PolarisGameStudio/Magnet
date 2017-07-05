﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace Replay
{
	public class ReplayParticles : MonoBehaviour 
	{
		public List<TimelinedParticles> particles = new List<TimelinedParticles> ();

		[Header ("Record Rate")]
		public bool overrideRecordRate = false;
		[ShowIfAttribute ("overrideRecordRate")]
		public int recordRate = 120;

		public bool isReplaying = false;

		protected ParticleSystem particleSys;

		protected virtual void Start ()
		{
			particleSys = GetComponent<ParticleSystem> ();

			foreach (Transform child in transform)
				if (child.GetComponent<ParticleSystem> () != null)
					child.gameObject.AddComponent<ReplayParticles> ();

			ReplayManager.Instance.OnReplayTimeChange += Replay;
			ReplayManager.Instance.OnReplayStart += OnReplayStart;
			ReplayManager.Instance.OnReplayStop += OnReplayStop;

			ReplayManager.Instance.OnRecordingStart += () => 
			{
				if(particleSys != null)
					particles.Add (new TimelinedParticles (particleSys));
			};

			if (GetComponent<ParticlesAutoDestroy> () != null)
				Destroy (GetComponent<ParticlesAutoDestroy> ());
		}

		void OnEnable ()
		{
			StartCoroutine (Recording ());
		}

		IEnumerator Recording ()
		{
			yield return new WaitUntil (() => ReplayManager.Instance != null);

			while (true) 
			{
				int recordRate = ReplayManager.Instance.particlesRecordRate;

				if (overrideRecordRate)
					recordRate = this.recordRate;

				yield return new WaitForSeconds (1 / recordRate);

				if (ReplayManager.Instance.isRecording && !ReplayManager.Instance.noRecordStates.Contains (GlobalVariables.Instance.GameState)) 
					Record ();
			}
		}

		protected virtual void Record ()
		{
			particles.Add (new TimelinedParticles (particleSys));
		}

		public void OnReplayStart ()
		{
			particleSys.Play (true);
			particleSys.Pause (true);
		}

		public void OnReplayStop ()
		{

		}

		void Update ()
		{
			if (isReplaying)
				Replay (ReplayManager.Instance.GetReplayTime ());
		}


		public void Replay (float t)
		{
			if (t < particles [0].time)
				particleSys.Clear ();

			if (particles.Count == 0)
				return;

			foreach(var p in particles)
			{
				if(Mathf.Abs (p.time - t) < 0.008f)
				{
					particleSys.SetParticles (p.particles, p.particles.Length);
					break;
				}
			}
		}

		public void OnDestroy ()
		{
			ReplayManager.Instance.OnReplayTimeChange -= Replay;
			ReplayManager.Instance.OnReplayStart -= OnReplayStart;
			ReplayManager.Instance.OnReplayStop -= OnReplayStop;
		}
	}

	[Serializable]
	public class TimelinedParticles 
	{
		public float time;
		public ParticleSystem.Particle[] particles = new ParticleSystem.Particle[0];

		public TimelinedParticles (ParticleSystem _ps)
		{
			time = ReplayManager.Instance.GetCurrentTime ();

			particles = new ParticleSystem.Particle[_ps.particleCount];
			_ps.GetParticles (particles);
		}
	}
}

