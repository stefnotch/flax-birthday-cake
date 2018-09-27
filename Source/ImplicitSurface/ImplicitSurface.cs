using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;
using NinaBirthday.FunctionCompiler;
using NinaBirthday.MetaSurf;

namespace NinaBirthday.ImplicitSurface
{
	[ExecuteInEditMode]
	public class ImplicitSurface : ScriptT<ModelActor>
	{
		public MaterialBase Material { get; set; }

		public string Expression
		{
			get => _expressionString;
			set
			{
				_expressionString = value;
				RecompileExpression(_expressionString);
			}
		}

		public Model Model
		{
			get;
			set;
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

		[HideInEditor]
		[NoSerialize]
		public BoundingBox BoundingBox
		{
			get => new BoundingBox(-BoundingBoxSize, BoundingBoxSize);
		}

		[ShowInEditor]
		[NoSerialize]
		private MetaSurface MetaSurface = new MetaSurface();

		private Mesh _mesh;

		private RuntimeASTCompiler _runtimeASTCompiler;

		private bool ShouldUpdateMesh = true;
		private float _previousTime;
		private string _expressionString;

		[Serialize]
		private Vector3 _boundingBoxSize = Vector3.One;

		public float MeshUpdateTime { get; set; } = 0.1f;

		private void Start()
		{
			if (Model && !Model.IsVirtual)
			{
				Actor.Model = Model;
			}
			else
			{
				_runtimeASTCompiler = new RuntimeASTCompiler();

				if (_expressionString == null)
				{
					MetaSurface.Evaluate = (x, y, z) =>
					   Mathf.Pow(Mathf.Sqrt(x * x + z * z) - 9, 2) + 1 * Mathf.Pow(y + Mathf.Sin(7f * Mathf.Atan2(z, x)), 2) - 5f;
				}
				else
				{
					Expression = _expressionString;
				}
				CreateVirtualModel();
			}

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
				MetaSurface.Polygonize(BoundingBox);
				var vertices = MetaSurface.Vertices;
				var triangles = MetaSurface.Indices;
				var normals = MetaSurface.Normals;
				if (vertices.Length > 0)
				{
					Debug.Log("Actual Update");
					_mesh.UpdateMesh(vertices, triangles, normals);
				}
				else
				{
					Debug.Log("Fake Update");
				}
				_previousTime = Time.GameTime;
			}
		}

		public void UpdateMesh()
		{
			ShouldUpdateMesh = true;
		}

		public void CreateVirtualModel()
		{
			Model = Content.CreateVirtualAsset<Model>();
			Model.SetupLODs(1);
			if (Material)
			{
				Model.SetupMaterialSlots(1);
			}
			Actor.Model = Model;
			_mesh = Model.LODs[0].Meshes[0];
		}

		private void RecompileExpression(string expression)
		{
			if (_runtimeASTCompiler == null) return;

			Func<float, float, float, float> compiledExpression = null;
			try
			{
				compiledExpression = _runtimeASTCompiler.CompileFunction(expression);
			}
			catch (Exception e)
			{
				Debug.Log(e);
				throw;
			}
			if (compiledExpression != null)
			{
				MetaSurface.Evaluate = compiledExpression;
				UpdateMesh();
			}
		}

		private void OnDebugDrawSelected()
		{
			var localBB = new BoundingBox(
					-BoundingBoxSize * Actor.Scale + Actor.Position,
					BoundingBoxSize * Actor.Scale + Actor.Position
				);
			DebugDraw.DrawBox(Actor.Box, Color.White);
			DebugDraw.DrawBox(localBB, Color.Red);
		}
	}
}