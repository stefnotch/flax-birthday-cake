using FlaxEngine;
using FlaxEngine.Utilities;
using System;
using System.Collections.Generic;

namespace NinaBirthday
{
	[ExecuteInEditMode]
	public class SpawnMover : Script
	{
		public float Distance { get; set; } = 3;
		private static Random _rng = new Random();

		private void OnEnable()
		{
			this.Actor.LocalPosition += _rng.NextVector3() * Distance;
		}
	}
}
