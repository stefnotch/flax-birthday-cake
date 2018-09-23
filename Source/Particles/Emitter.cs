using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace LowQualityParticles
{
	[ExecuteInEditMode]
	public class Emitter : Script
	{
		private bool _enabledInEditor;

		private static Random _rng = new Random();

		public float Radius = 0f;
		public Prefab Particle;
		public bool EnabledInEditor { get => _enabledInEditor; set => _enabledInEditor = value; }
		public int ParticleCount = 10;

		private void Start()
		{
			// Here you can add code that needs to be called when script is created
		}

		private void Update()
		{
			if (EnabledInEditor /* TODO: In game, always run*/)
			{
				int spawnCount = ParticleCount - this.Actor.ChildrenCount;

				if (spawnCount >= 1)
				{
					SpawnParticles(spawnCount);
				}
			}
		}

		private void SpawnParticles(int count)
		{
			if (!Particle) return;

			for (int i = 0; i < count; i++)
			{
				var spawnedParticle = PrefabManager.SpawnPrefab(Particle, this.Actor);
				var particleScript = spawnedParticle.GetScript<Particle>() ?? spawnedParticle.AddScript<Particle>();
				particleScript.ResetCallback = ResetParticle;
				spawnedParticle.HideFlags = HideFlags.FullyHidden;

				ResetParticle(spawnedParticle);
			}
		}

		private void ResetParticle(Actor particle)
		{
			Vector3 pos = _rng.NextVector3() * (_rng.NextFloat(2 * Radius) - Radius);
			particle.LocalPosition = pos;
		}
	}
}