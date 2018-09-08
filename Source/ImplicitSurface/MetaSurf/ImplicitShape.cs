using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace NinaBirthday.MetaSurf
{
	public abstract class ImplicitShape
	{
		public abstract float Evaluate(float x, float y, float z);

		public bool ShouldComputeNormal { get; set; } = true;

		public BoundingBox BoundingBox { get; set; }
	}
}
