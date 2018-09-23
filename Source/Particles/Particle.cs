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

		public Vector3 Target;

		public float RandomMotion = 0.1f;

		private float Speed;
		private static Random _rng = new Random();
		private MaterialInstance _instance;
		private float _maxLength;

		public Action<Actor> ResetCallback;

		private void OnEnable()
		{
			ModelActor modelActor = Actor as ModelActor ?? Actor.GetChild<ModelActor>();
			ReplaceWithMaterialInstance(modelActor.Entries[0]);

			_maxTimeToLive = TimeToLive;
			Reset();
		}

		private void Reset()
		{
			Speed = _rng.NextFloat(50) + 30;
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
				if (ResetCallback != null)
				{
					ResetCallback(Actor);
					Reset();
				}
				else
				{
					Destroy(Actor);
				}
			}
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
			this.Actor.LocalPosition += (direction * Speed + random) * Time.DeltaTime;
		}
	}
}