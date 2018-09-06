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
	public class ImplicitSurface : ScriptT<ModelActor>
	{
		[ShowInEditor]
		[NoSerialize]
		private MetaSurface MetaSurface = new MetaSurface()
		{
			Threshold = 12
		};

		private Mesh _mesh;

		private bool ShouldUpdateMesh;
		private float _previousTime;

		public float MeshUpdateTime { get; set; } = 0.1f;

		private void Start()
		{
			//MetaSurface.ImplicitShapes.Clear();

			var model = Content.CreateVirtualAsset<Model>();
			model.SetupLODs(1);
			this.Actor.Model = model;
			_mesh = model.LODs[0].Meshes[0];
		}

		private void Update()
		{
			if (_mesh != null && ShouldUpdateMesh && Time.GameTime - _previousTime > MeshUpdateTime)
			{
				ShouldUpdateMesh = false;

				MetaSurface.Polygonize();
				var vertices = MetaSurface.Vertices;
				if (vertices.Length > 0)
				{
					_mesh.UpdateMesh(vertices, MetaSurface.Indices, MetaSurface.Normals);
				}
				_previousTime = Time.GameTime;
			}
		}

		public void RemoveShape(MetaBall metaBall)
		{
			MetaSurface.ImplicitShapes.Remove(metaBall);
		}

		public void AddShape(IImplicitShape shape)
		{
			MetaSurface.ImplicitShapes.Add(shape);
		}

		public void UpdateMesh()
		{
			ShouldUpdateMesh = true;
		}
	}
}
