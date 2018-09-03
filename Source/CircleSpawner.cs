using FlaxEngine;
using System;
using System.Collections.Generic;

namespace NinaBirthday
{
	[ExecuteInEditMode]
	public class CircleSpawner : Script
	{
		public Prefab PrefabToSpawn { get; set; }

		[Range(0, 1000)]
		public float Radius
		{
			get => _radius;
			set { _radius = value; Spawn(); }
		}

		[Range(0, 100)]
		public int Count
		{
			get => _count;
			set { _count = value; Spawn(); }
		}

		private float _radius;
		private int _count;

		[Serialize]
		private Actor _container;

		private void OnEnable()
		{
			// Here you can add code that needs to be called when script is created
			Spawn();
		}

		public void Spawn()
		{
			if (PrefabToSpawn == null) return;

			if (_container == null)
			{
				_container = New<EmptyActor>();
				this.Actor.AddChild(_container, false);
			}
			else
			{
				_container.DestroyChildren();
			}

			float angle = 0;
			for (int i = 0; i < Count; i++)
			{
				var spawnedActor = PrefabManager.SpawnPrefab(PrefabToSpawn, _container);
				spawnedActor.LocalPosition += Vector3.Forward * Quaternion.RotationY(angle) * Radius;
				//spawnedActor.LookAt(this.Actor);
				angle += Mathf.TwoPi / Count;
			}
		}

		private void OnDisable()
		{
			Destroy(_container);
		}
	}
}
