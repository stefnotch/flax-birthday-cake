using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;
using NinaBirthday.MetaSurf;

namespace NinaBirthday.ImplicitSurface
{
	[ExecuteInEditMode]
	public class ImplicitObject : Script
	{
		private ImplicitSurface _surface;
		private Vector3 _position;

		[ShowInEditor]
		[NoSerialize]
		private MetaBall _metaBall;

		private void OnEnable()
		{
			if (Actor.Parent != null)
			{
				_surface = Actor.Parent.GetScript<ImplicitSurface>();
			}

			if (_surface != null)
			{
				_metaBall = new MetaBall(Actor.LocalPosition, 0.25f);
				_surface.AddShape(_metaBall);
				_surface.UpdateMesh();
			}
		}

		private void Update()
		{
			if (Actor.Position != _position && _surface != null)
			{
				_position = Actor.Position;
				_metaBall.Position = Actor.LocalPosition;
				_surface.UpdateMesh();
			}
		}

		private void OnDisable()
		{
			if (_surface != null)
			{
				_surface.RemoveShape(_metaBall);
			}
		}
	}
}
