using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEngine;

namespace NinaBirthday.MetaSurf
{

	public class MetaSurfaceCellContainer
	{
		public Vector3 Position;
		public float Value;
		public MetaSurfaceCell[,,] Values;
	}

	public class MetaSurfaceCell
	{
		public Vector3 Position;
		public float Value;
		public int EdgeXIndex;
		public int EdgeYIndex;
		public int EdgeZIndex;
	}
}
