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
		public MaterialBase Material { get; set; }
		/*[ShowInEditor]
		[NoSerialize]
		private MetaSurface MetaSurface = new MetaSurface()
		{
			Threshold = 12
		};*/

		[ShowInEditor]
		[NoSerialize]
		private MetaSurface MetaSurface = new MetaSurface();

		private readonly List<ImplicitShape> ImplicitShapes = new List<ImplicitShape>();

		private Mesh _mesh;

		private bool ShouldUpdateMesh = true;
		private float _previousTime;

		public float MeshUpdateTime { get; set; } = 0.1f;

		private void Start()
		{
			System.Linq.Expressions.Expression<Func<float, float, float, float>> ex = (x, y, z) =>
				Mathf.Pow(Mathf.Sqrt(x * x + z * z) - 9, 2) + 1 * Mathf.Pow(y + Mathf.Sin(7f * Mathf.Atan2(z, x)), 2) - 5f;

			var compiled = ex.Compile();
			//MetaSurface.ImplicitShapes.Clear();
			MetaSurface.Evaluate = compiled;

			var model = Content.CreateVirtualAsset<Model>();
			model.SetupLODs(1);
			if (Material)
			{
				model.SetupMaterialSlots(1);
			}
			Actor.Model = model;
			_mesh = model.LODs[0].Meshes[0];
			if (Material)
			{
				Actor.Entries[0].Material = Material;
			}
		}

		private void Update()
		{
			if (_mesh != null && ShouldUpdateMesh && Time.GameTime - _previousTime > MeshUpdateTime)
			{
				ShouldUpdateMesh = false;
				Debug.Log("Up");
				MetaSurface.Polygonize(new BoundingBox(new Vector3(-6), new Vector3(6)));
				var vertices = MetaSurface.Vertices;
				var triangles = MetaSurface.Indices;
				var normals = MetaSurface.Normals;
				if (vertices.Length > 0)
				{
					_mesh.UpdateMesh(vertices, triangles, normals);
				}
				_previousTime = Time.GameTime;
			}
		}

		public void RemoveShape(ImplicitShape shape)
		{
			ImplicitShapes.Remove(shape);
		}

		public void AddShape(ImplicitShape shape)
		{
			ImplicitShapes.Add(shape);
		}

		public void UpdateMesh()
		{
			ShouldUpdateMesh = true;
		}
	}
}
