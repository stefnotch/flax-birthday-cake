using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace NinaBirthday.SugarThingy
{
	[ExecuteInEditMode]
	public class ImprovedRandomOrientation : Script
	{
		private static readonly Random _rng = new Random();

		public Vector3 AngleFrom;
		public Vector3 AngleTo;

		private void Start()
		{
			Vector3 delta = AngleTo - AngleFrom;
			this.Actor.LocalOrientation = Quaternion.Euler(_rng.NextVector3() * delta + AngleFrom);
		}
	}
}