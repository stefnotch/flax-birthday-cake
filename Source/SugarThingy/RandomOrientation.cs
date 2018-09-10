using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace NinaBirthday.SugarThingy
{
	[ExecuteInEditMode]
	public class RandomOrientation : Script
	{
		private static readonly Random _rng = new Random();
		private void Start()
		{
			this.Actor.LocalOrientation = _rng.NextQuaternion();
		}
	}
}
