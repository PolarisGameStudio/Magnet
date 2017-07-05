﻿/* IN-GAME REPLAY - @madebyfeno - <feno@ironequal.com>
 * You can use it in commercial projects (and non-commercial project of course), modify it and share it.
 * Do not resell the resources of this project as-is or even modified. 
 * TL;DR: Do what the fuck you want but don't re-sell it
 * 
 * ironequal.com
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.AI;


namespace Replay
{

	[Serializable]
	public class TimelinedVector3
	{
		public AnimationCurve x;
		public AnimationCurve y;
		public AnimationCurve z;

		public void Add (Vector3 v)
		{
			float time = ReplayManager.Instance.GetCurrentTime ();
			x.AddKey (time, v.x);
			y.AddKey (time, v.y);
			z.AddKey (time, v.z);
		}

		public Vector3 Get (float _time)
		{
			return new Vector3 (x.Evaluate (_time), y.Evaluate (_time), z.Evaluate (_time));
		}
	}

	[Serializable]
	public class TimelinedQuaternion
	{
		public AnimationCurve x;
		public AnimationCurve y;
		public AnimationCurve z;
		public AnimationCurve w;

		public void Add (Quaternion v)
		{
			float time = ReplayManager.Instance.GetCurrentTime ();
			x.AddKey (time, v.x);
			y.AddKey (time, v.y);
			z.AddKey (time, v.z);
			w.AddKey (time, v.w);
		}

		public Quaternion Get (float _time)
		{
			return new Quaternion (x.Evaluate (_time), y.Evaluate (_time), z.Evaluate (_time), w.Evaluate (_time));
		}
	}

	[Serializable]
	public class RecordData
	{
		public TimelinedVector3 position;
		public TimelinedQuaternion rotation;
		public TimelinedVector3 scale;
		public AnimationCurve enabled;

		public void Add (Transform t)
		{
			position.Add (t.position);
			rotation.Add (t.rotation);
			scale.Add (t.localScale);
		}

		public void Set (float _time, Transform _transform)
		{
			_transform.position = position.Get (_time);
			_transform.rotation = rotation.Get (_time);
			_transform.localScale = scale.Get (_time);

			SetEnable (_time, _transform.gameObject);
		}

		public void AddEnable (bool enable)
		{
			float time = ReplayManager.Instance.GetCurrentTime ();
			enabled.AddKey (time, enable ? 1f : 0f);
		}

		public void SetEnable (float _time, GameObject _gameobject)
		{
			if(enabled.Evaluate (_time) > 0.5f && !_gameobject.activeSelf)
				_gameobject.SetActive (true);

			if(enabled.Evaluate (_time) < 0.5f && _gameobject.activeSelf)
				_gameobject.SetActive (false);
		}
	}

	public class ReplayEntity : MonoBehaviour
	{
		[Header ("States")]
		public bool isRecording = false;

		[Header ("Settings")]
		public bool recordPosition = true;
		public bool recordRotation = true;
		public bool recordScale = true;
		public bool recordEnable = true;

		[Header ("Data")]
		public RecordData data = new RecordData ();

		private Rigidbody rigidBody;
		private NavMeshAgent agent;
		private Animator animator;

		protected virtual void Start ()
		{
			ReplayManager.Instance.OnReplayTimeChange += Replay;
			ReplayManager.Instance.OnReplayStart += OnReplayStart;
			ReplayManager.Instance.OnReplayStop += OnReplayStop;

			ReplayManager.Instance.OnRecordingStart += () => 
			{
				data.AddEnable (false);
				data.AddEnable (true);
			};


			rigidBody = GetComponent<Rigidbody> ();
			agent = GetComponent<NavMeshAgent> ();
			animator = GetComponent<Animator> ();
		}

		void OnEnable ()
		{
			if(recordEnable)
				data.AddEnable (true);
			StartCoroutine (Recording ());
		}

		void OnDisable ()
		{
			if(recordEnable)
				data.AddEnable (false);
		}

		IEnumerator Recording ()
		{
			while (true) 
			{
				yield return new WaitForSeconds (1 / ReplayManager.Instance.recordRate);

				if (ReplayManager.Instance.isRecording && !ReplayManager.Instance.noRecordStates.Contains (GlobalVariables.Instance.GameState)) 
				{
					isRecording = true;

					if(recordPosition)
						data.position.Add (transform.position);
					
					if(recordRotation)
						data.rotation.Add (transform.rotation);
					
					if(recordScale)
						data.scale.Add (transform.localScale);
				} 
				else
					isRecording = false;
			}
		}

		public void OnReplayStart ()
		{
			ReplayManager.Instance.SetCurveConstant (data.enabled);

			if (rigidBody != null)
				rigidBody.isKinematic = true;

			if (agent)
				agent.enabled = false;

			if (animator)
				animator.enabled = false;	
		}

		public void OnReplayStop ()
		{
			if (rigidBody != null)
				rigidBody.isKinematic = false;

			if (agent)
				agent.enabled = true;

			if (animator)
				animator.enabled = true;	
		}

		public void Replay (float t)
		{
			ReplayManager.Instance.SetCurveConstant (data.enabled);

			if(recordPosition)
				transform.position = data.position.Get (t);
			if(recordRotation)
				transform.rotation = data.rotation.Get (t);
			if(recordScale)
				transform.localScale = data.scale.Get (t);

			if(recordEnable)
				data.SetEnable (t, transform.gameObject);
		}

		public void OnDestroy ()
		{
			ReplayManager.Instance.OnReplayTimeChange -= Replay;
			ReplayManager.Instance.OnReplayStart -= OnReplayStart;
			ReplayManager.Instance.OnReplayStop -= OnReplayStop;
		}
	}
}
