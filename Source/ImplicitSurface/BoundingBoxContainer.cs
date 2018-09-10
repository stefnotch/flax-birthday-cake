using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace NinaBirthday.Source.ImplicitSurface
{
	[ExecuteInEditMode]
	public class BoundingBoxContainer : Script
	{
		public BoundingBox BoundingBox
		{
			get;
			set;
		} = new BoundingBox(-Vector3.One, Vector3.One);
	}
}
