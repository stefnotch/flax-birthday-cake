using FlaxEngine;
using NinaBirthday.MetaSurf;
using System;
using System.Collections.Generic;

namespace NinaBirthday
{
	public class MeshMerger : Script
	{
		public readonly List<Actor> ModelActors = new List<Actor>();

		public bool Recursive = false;

		private MetaSurface metaSurface;
		private MetaBalls balls = new MetaBalls();

		[Serialize]
		private ModelActor _mergedActor;

		public void Run()
		{
			const int resolution = 38;
			const float threshold = 12;
			metaSurface = new MetaSurface(balls.eval, balls.vertex);
			metaSurface.thres = threshold;
			metaSurface.Resolution = new Vector3(resolution);
			metaSurface.BoundingBox = new BoundingBox(Vector3.One * -1, Vector3.One);

			Destroy(_mergedActor);

			var model = Content.CreateVirtualAsset<Model>();
			model.SetupLODs(1);

			_mergedActor = New<ModelActor>();
			_mergedActor.Model = model;
			this.Actor.AddChild(_mergedActor, false);
			_mergedActor.LocalPosition = Vector3.Zero;


			var mesh = _mergedActor.Model.LODs[0].Meshes[0];
			UpdateMesh(mesh);
			Debug.Log("Created");
		}

		private void UpdateMesh(Mesh mesh)
		{
			balls.reset();
			metaSurface.Polygonize();
			mesh.UpdateMesh(balls.vertices, balls.indices, balls.normals);
		}
	}
}
