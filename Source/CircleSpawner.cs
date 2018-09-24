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

		[Serialize]
		private Actor _container;

		public Prefab PrefabToSpawn { get; set; }

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

		[NoSerialize]
		public Actor Container
		{
			get => _container;
#if FLAX_EDITOR
			set
			{
				if (value && !value.HasChildren)
				{
					_container = value;
				}
			}
#else
			set => _container = value;
#endif
		}

		private bool _canSpawn;

		private void OnEnable()
		{
			// Here you can add code that needs to be called when script is created
			_canSpawn = true;
			Spawn();
		}

		public void Spawn()
		{
			if (!_canSpawn) return;
			if (!PrefabToSpawn) return;

			if (!Container)
			{
				Container = New<EmptyActor>();
				if (!Container) return;
				Actor.AddChild(Container, false);
				Container.LocalTransform = Transform.Identity;
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
				spawnedActor.LocalPosition += new Vector3(_rng.NextFloat(2) - 1, 0, _rng.NextFloat(2) - 1) * RandomOffsetRadius;
				//spawnedActor.LookAt(this.Actor);
				angle += Mathf.TwoPi / Count;
			}
		}

		private void OnDisable()
		{
			_canSpawn = false;

			Destroy(ref _container);
		}
	}
}