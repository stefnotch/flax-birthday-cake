using System;
using System.Collections.Generic;
using FlaxEngine;
using NinaBirthday.FunctionCompiler;
using NinaBirthday.MetaSurf;
using NinaBirthday.Source;

namespace NinaBirthday
{
	[ExecuteInEditMode]
	public class ImplicitChocolate : ScriptT<ModelActor>
	{
		private TextureToSDF.SDFGrid _sdf;

		[Serialize]
		private Texture _texture;

		[Serialize]
		private Material _material;

		private MaterialInstance _materialInstance;

		[NoSerialize]
		public Material Material
		{
			get => _material;
			set
			{
				_material = value;
				MaterialChanged();
			}
		}

		[NoSerialize]
		public Vector3 BoundingBoxSize
		{
			get => _boundingBoxSize;
			set
			{
				_boundingBoxSize = value;
				UpdateMesh();
			}
		}

		public float Height = 10f;
		public float Radius = 10f;

		[NoSerialize]
		public Texture Texture
		{
			get => _texture;
			set
			{
				_texture = value;
				if (_texture)
				{
					_sdf = TextureToSDF.SDFGrid.FromTexture(_texture);
				}
			}
		}

		[HideInEditor]
		[NoSerialize]
		public BoundingBox BoundingBox
		{
			get => new BoundingBox(-BoundingBoxSize * 0.5f, BoundingBoxSize * 0.5f);
		}

		[ShowInEditor]
		[NoSerialize]
		private MetaSurface MetaSurface = new MetaSurface();

		private Mesh _mesh;

		private bool ShouldUpdateMesh = true;
		private float _previousTime;

		[Serialize]
		private Vector3 _boundingBoxSize = Vector3.One;

		public float MeshUpdateTime { get; set; } = 0.1f;
		public float Width { get; private set; } = 3f;

		private void Start()
		{
			//MetaSurface.Resolution = 50;
			var model = Content.CreateVirtualAsset<Model>();
			model.SetupLODs(1);
			if (Material)
			{
				model.SetupMaterialSlots(1);
			}
			Actor.Model = model;
			_mesh = model.LODs[0].Meshes[0];

			MaterialChanged();

			if (Texture)
			{
				_sdf = TextureToSDF.SDFGrid.FromTexture(Texture);
			}

			MetaSurface.Evaluate = Evaluate;
		}

		private void MaterialChanged()
		{
			if (Material)
			{
				if (_materialInstance)
				{
					Destroy(ref _materialInstance);
				}
				_materialInstance = Material.CreateVirtualInstance();
				Actor.Model.WaitForLoaded();
				Actor.Entries[0].Material = _materialInstance;
			}
		}

		private float Evaluate(float x, float y, float z)
		{
			if (_sdf == null) return -1;

			float angle = (Mathf.Atan2(z, x) + Mathf.Pi) * _sdf.Width / Mathf.TwoPi;

			y *= Height;

			int posX = (int)angle;
			int posY = (int)y;
			if (posX >= 0 && posY >= 0 && posX < _sdf.Width && posY < _sdf.Height)
			{
				float dist = x * x + z * z;

				float delimiter = dist - Radius;
				float chocolateHeight = _sdf[posX, posY] / 2f;
				return (delimiter - Width < chocolateHeight && chocolateHeight < delimiter + Width) ? 1 : -1;
			}
			else
			{
				return -1;
			}
		}

		private void Update()
		{
			if (_mesh != null && ShouldUpdateMesh && Time.GameTime - _previousTime > MeshUpdateTime)
			{
				ShouldUpdateMesh = false;
				//Debug.Log("Up");
				MetaSurface.Polygonize(BoundingBox);
				var vertices = MetaSurface.Vertices;
				var triangles = MetaSurface.Indices;
				var normals = MetaSurface.Normals;
				if (vertices.Length > 0)
				{
					Debug.Log("Actual Update");
					_mesh.UpdateMesh(vertices, triangles, normals);
				}
				_previousTime = Time.GameTime;
			}
		}

		public void UpdateMesh()
		{
			ShouldUpdateMesh = true;
		}

		private void OnDisable()
		{
			Destroy(ref _materialInstance);
		}
	}
}