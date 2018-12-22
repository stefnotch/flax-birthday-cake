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
		private MaterialInstance _materialInstance;
		public string ParamName { get; set; } = "Color";

		private static readonly Random _rng = new Random();

		private void Start()
		{
			if (Material)
			{
				_materialInstance = Material.CreateVirtualInstance();
				_materialInstance.GetParam(ParamName).Value = new Color(_rng.NextFloat(), _rng.NextFloat(), _rng.NextFloat());
				if (Actor is StaticModel modelActor)
				{
					modelActor.Entries[0].Material = _materialInstance;
				}
			}
		}

		private void OnDestroy()
		{
			Destroy(ref _materialInstance);
		}
	}
}