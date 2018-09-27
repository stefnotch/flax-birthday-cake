using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Rendering;
using FlaxEngine.Utilities;

namespace LowQualityParticles
{
	[ExecuteInEditMode]
	public class Particle : Script
	{
		public float TimeToLive = 10f;
		private float _maxTimeToLive;

		private Vector3 _target;

		public float RandomMotion = 0.1f;
		public float SpeedMultiplier = 1;
		private float Speed;
		private static Random _rng = new Random();
		private MaterialInstance _instance;
		private float _maxLength;

		public Func<Particle, bool> ResetCallback;

		private void OnEnable()
		{
			ModelActor modelActor = Actor as ModelActor ?? Actor.GetChild<ModelActor>();
			modelActor.Model.WaitForLoaded();
			ReplaceWithMaterialInstance(modelActor.Entries[0]);

			_maxTimeToLive = TimeToLive;
			Reset();
		}

		private void Reset()
		{
			Speed = _rng.NextFloat(30) + 50;
			TimeToLive = _maxTimeToLive;
			_maxLength = (Target - Actor.LocalPosition).Length;
		}

		private void OnDisable()
		{
			Destroy(ref _instance);
		}

		private void ReplaceWithMaterialInstance(ModelEntryInfo modelEntryInfo)
		{
			if (modelEntryInfo == null) return;
			if (!modelEntryInfo.Material) return;

			modelEntryInfo.Material = _instance = modelEntryInfo.Material.CreateVirtualInstance();
		}

		private MaterialParameter TimeParam
		{
			get
			{
				ModelActor modelActor = Actor as ModelActor ?? Actor.GetChild<ModelActor>();
				if (modelActor)
				{
					return modelActor.Entries[0].Material?.GetParam("Time");
				}
				return null;
			}
		}

		public Vector3 Target
		{
			get => _target;
			set
			{
				_target = value;
				_maxLength = (_target - Actor.LocalPosition).Length;
			}
		}

		private void Update()
		{
			var t = TimeParam;
			if (t != null)
			{
				float time = (_maxTimeToLive - TimeToLive) / _maxTimeToLive;
				float dist = (_maxLength - (Target - Actor.LocalPosition).Length) / _maxLength;
				t.Value = Mathf.Min(time, dist);
			}

			TimeToLive -= Time.DeltaTime;
			var actor = this.Actor;
			if (TimeToLive <= 0)
			{
				if (ResetCallback != null && ResetCallback(this))
				{
					Reset();
				}
				else
				{
					Death();
				}
			}
		}

		public void Death()
		{
			Destroy(Actor);
		}

		private void FixedUpdate()
		{
			Vector3 direction = (Target - Actor.LocalPosition);
			if (direction.LengthSquared < 5f)
			{
				TimeToLive = 0;
			}
			direction.Normalize();
			Vector3 random = _rng.NextVector3() * RandomMotion;
			Vector3 postitionAdd = (direction * Speed * SpeedMultiplier * _maxLength / 60f + random) * Time.DeltaTime;

			Vector3 finalPosition = Actor.LocalPosition + postitionAdd;

			bool signChange =
				Mathf.Sign(direction.X) != Mathf.Sign(Target.X - finalPosition.X) ||
				Mathf.Sign(direction.Y) != Mathf.Sign(Target.Y - finalPosition.Y) ||
				Mathf.Sign(direction.Z) != Mathf.Sign(Target.Z - finalPosition.Z);

			if (signChange)
			{
				TimeToLive = 0;
			}

			this.Actor.LocalPosition = finalPosition;
		}
	}
}