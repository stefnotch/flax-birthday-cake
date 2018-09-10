using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace NinaBirthday.SugarThingy
{
	[ExecuteInEditMode]
	public class RandomColor : Script
	{
		public Material Material { get; set; }

		public string ParamName { get; set; } = "Color";

		private static readonly Random _rng = new Random();

		private void Start()
		{
			if (Material)
			{
				Material.GetParam(ParamName).Value = new Color(_rng.NextFloat(), _rng.NextFloat(), _rng.NextFloat());
			}
		}
	}
}
