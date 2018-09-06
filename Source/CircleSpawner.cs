using System;
using System.Collections.Generic;
using FlaxEngine;

namespace NinaBirthday
{
	[ExecuteInEditMode]
	public class CircleSpawner : Script
	{
		public Prefab PrefabToSpawn { get; set; }
		public Actor Container;

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


		private void OnEnable()
		{
			// Here you can add code that needs to be called when script is created
			Spawn();
		}

		public void Spawn()
		{
			if (PrefabToSpawn == null) return;

			if (Container == null)
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
				spawnedActor.LocalPosition += Vector3.Forward * Quaternion.RotationY(angle) * Radius;
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
