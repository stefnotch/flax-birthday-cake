using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace NinaBirthday
{
	[ExecuteInEditMode]
	public class CircleSpawner : Script
	{
		private static Random _rng = new Random();
		private float _radius;
		private int _count;
		private float _randomOffsetRadius;

		public Prefab PrefabToSpawn { get; set; }
		public Actor Container;

		[Range(0, 1000)]
		public float CircleRadius
		{
			get => _radius;
			set { _radius = value; Spawn(); }
		}

		[Range(0, 100)]
		public float RandomOffsetRadius
		{
			get => _randomOffsetRadius;
			set { _randomOffsetRadius = value; Spawn(); }
		}

		[Range(0, 100)]
		public int Count
		{
			get => _count;
			set { _count = value; Spawn(); }
		}

		private void OnEnable()
		{
			// Here you can add code that needs to be called when script is created
			Spawn();
		}

		public void Spawn()
		{
			if (PrefabToSpawn == null) return;

			if (Container == null || Container.HasChildren)
			{
				Container = New<EmptyActor>();
				this.Actor.AddChild(Container, false);
			}
			else
			{
				Container.DestroyChildren();
			}

			float angle = 0;
			for (int i = 0; i < Count; i++)
			{
				var spawnedActor = PrefabManager.SpawnPrefab(PrefabToSpawn, Container);
				spawnedActor.LocalPosition += Vector3.Forward * Quaternion.RotationY(angle) * CircleRadius;
				spawnedActor.LocalPosition += _rng.NextVector3() * RandomOffsetRadius;
				//spawnedActor.LookAt(this.Actor);
				angle += Mathf.TwoPi / Count;
			}
		}

		private void OnDisable()
		{
			Destroy(Container);
		}
	}
}
