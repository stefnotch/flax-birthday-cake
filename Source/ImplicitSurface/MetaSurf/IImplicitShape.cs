using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinaBirthday.MetaSurf
{
	public interface IImplicitShape
	{
		float Evaluate(MetaSurface ms, float x, float y, float z);

		bool ShouldComputeNormal { get; set; }

		BoundingBox BoundingBox { get; set; }
	}
}
