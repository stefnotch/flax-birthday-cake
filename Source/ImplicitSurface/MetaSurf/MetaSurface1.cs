using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace NinaBirthday.MetaSurf
{
	public class MetaSurface1
	{
		public int[] Indices => _indices.ToArray();
		public Vector3[] Vertices => _vertices.ToArray();
		public Vector3[] Normals => _normals.ToArray();

		private readonly List<int> _indices = new List<int>();
		private readonly List<Vector3> _vertices = new List<Vector3>();
		private readonly List<Vector3> _normals = new List<Vector3>();
		private int _indexCounter;
		//private MetaSurfaceOctTree _octree;
		private int Resolution = 40;

		const float Epsilon = 0.001f;

		public MetaSurface1()
		{

			// Setting up all the cells
			VVV();

		}

		private float Evaluate(float x, float y, float z)
		{
			/*float sum = 0;
			for (int i = 0; i < _shapes.Count; i++)
			{
				sum += _shapes[i].Evaluate(x, y, z);
			}
			return sum;*/
			return Mathf.Sin(x * 0.1f) + Mathf.Sin(y * 0.1f) + Mathf.Sin(z * 0.1f);
		}

		/*private Vector3 Normal(ref Vector3 point)
		{
			float centerPoint = Evaluate(point.X, point.Y, point.Z);
			float dx = Evaluate(point.X + Epsilon, point.Y, point.Z);
			float dy = Evaluate(point.X, point.Y + Epsilon, point.Z);
			float dz = Evaluate(point.X, point.Y, point.Z + Epsilon);
			float len = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);

			return new Vector3(dx / len, dy / len, dz / len);
		}*/
		private Vector3 Normal(ref Vector3 point)
		{
			float centerPoint = Evaluate(point.X, point.Y, point.Z);
			float dx = (Evaluate(point.X + Epsilon, point.Y, point.Z) - centerPoint) / Epsilon;
			float dy = (Evaluate(point.X, point.Y + Epsilon, point.Z) - centerPoint) / Epsilon;
			float dz = (Evaluate(point.X, point.Y, point.Z + Epsilon) - centerPoint) / Epsilon;

			float len = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);

			return new Vector3(dx / len, dy / len, dz / len);
		}

		private Vector3 SurfacePoint(MetaSurfaceCell a, MetaSurfaceCell b)
		{
			float t = -a.Value / (b.Value - a.Value);

			return new Vector3(
					a.Position.X + t * (b.Position.X - a.Position.X),
					a.Position.Y + t * (b.Position.Y - a.Position.Y),
					a.Position.Z + t * (b.Position.Z - a.Position.Z)
				);
		}

		private int GetEdgeIndex(ref int edgeIndex, MetaSurfaceCell a, MetaSurfaceCell b)
		{
			if (edgeIndex == -1)
			{
				Vector3 position = SurfacePoint(a, b);
				edgeIndex = EmitVertex(position, Normal(ref position));
			}

			return edgeIndex;
		}

		private int EmitVertex(Vector3 position, Vector3 normal)
		{
			_vertices.Add(position);
			_normals.Add(normal);
			//_indices.Add(_indexCounter);
			return _indexCounter++;
		}

		private MetaSurfaceCell[,,] Values;
		private List<ImplicitShape> _shapes;
		public void Polygonize(List<ImplicitShape> shapes)
		{
			Reset();
			_shapes = shapes;
			BoundingBox boundingBox = BoundingBox.Empty;
			foreach (var shape in shapes)
			{
				boundingBox = BoundingBox.Merge(boundingBox, shape.BoundingBox);
			}

			Vector3 center = Vector3.Zero;

			float radiusX = boundingBox.Size.X;
			float radiusY = boundingBox.Size.Y;
			float radiusZ = boundingBox.Size.Z;


			for (int i = 0; i <= Resolution; i++)
			{
				float x = radiusX * ((2f * i) / Resolution - 1f) + center.X;
				for (int j = 0; j <= Resolution; j++)
				{
					float y = radiusY * ((2f * j) / Resolution - 1f) + center.Y;
					for (int k = 0; k <= Resolution; k++)
					{
						float z = radiusZ * ((2f * k) / Resolution - 1f) + center.Z;

						var cell = this.Values[i, j, k];
						cell.Position.X = x;
						cell.Position.Y = y;
						cell.Position.Z = z;
						cell.Value = Evaluate(x, y, z);
						cell.EdgeXIndex = -1;
						cell.EdgeYIndex = -1;
						cell.EdgeZIndex = -1;
					}
				}
			}

			int[] vertexIndices = new int[12];
			for (int i = 0; i < Resolution; i++)
			{
				for (int j = 0; j < Resolution; j++)
				{
					for (int k = 0; k < Resolution; k++)
					{
						var c0 = Values[i, j + 1, k];
						var c1 = Values[i + 1, j + 1, k];
						var c2 = Values[i + 1, j, k];
						var c3 = Values[i, j, k];
						var c4 = Values[i, j + 1, k + 1];
						var c5 = Values[i + 1, j + 1, k + 1];
						var c6 = Values[i + 1, j, k + 1];
						var c7 = Values[i, j, k + 1];

						int cubeIndex = 0;
						if (c0.Value < 0) cubeIndex |= 1;
						if (c1.Value < 0) cubeIndex |= 2;
						if (c2.Value < 0) cubeIndex |= 4;
						if (c3.Value < 0) cubeIndex |= 8;
						if (c4.Value < 0) cubeIndex |= 16;
						if (c5.Value < 0) cubeIndex |= 32;
						if (c6.Value < 0) cubeIndex |= 64;
						if (c7.Value < 0) cubeIndex |= 128;

						int edges = MCubes.mc_edge_table[cubeIndex];

						if ((edges & 1) != 0)
						{
							vertexIndices[0] = GetEdgeIndex(ref c0.EdgeXIndex, c0, c1);
						}
						if ((edges & 2) != 0)
						{
							vertexIndices[1] = GetEdgeIndex(ref c2.EdgeXIndex, c2, c1);
						}
						if ((edges & 4) != 0)
						{
							vertexIndices[2] = GetEdgeIndex(ref c3.EdgeXIndex, c3, c2);
						}
						if ((edges & 8) != 0)
						{
							vertexIndices[3] = GetEdgeIndex(ref c3.EdgeXIndex, c3, c0);
						}
						if ((edges & 16) != 0)
						{
							vertexIndices[4] = GetEdgeIndex(ref c4.EdgeXIndex, c4, c5);
						}
						if ((edges & 32) != 0)
						{
							vertexIndices[5] = GetEdgeIndex(ref c6.EdgeXIndex, c6, c5);
						}
						if ((edges & 64) != 0)
						{
							vertexIndices[6] = GetEdgeIndex(ref c7.EdgeXIndex, c7, c6);
						}
						if ((edges & 128) != 0)
						{
							vertexIndices[7] = GetEdgeIndex(ref c7.EdgeXIndex, c7, c4);
						}
						if ((edges & 256) != 0)
						{
							vertexIndices[8] = GetEdgeIndex(ref c0.EdgeXIndex, c0, c4);
						}
						if ((edges & 512) != 0)
						{
							vertexIndices[9] = GetEdgeIndex(ref c1.EdgeXIndex, c1, c5);
						}
						if ((edges & 1024) != 0)
						{
							vertexIndices[10] = GetEdgeIndex(ref c2.EdgeXIndex, c2, c6);
						}
						if ((edges & 2048) != 0)
						{
							vertexIndices[11] = GetEdgeIndex(ref c3.EdgeXIndex, c3, c7);
						}


						for (int n = 0; n < 16 && MCubes.mc_tri_table[cubeIndex, n] != -1; n += 3)
						{
							this.EmitTriangle(
									vertexIndices[MCubes.mc_tri_table[cubeIndex, n]],
									vertexIndices[MCubes.mc_tri_table[cubeIndex, n + 1]],
									vertexIndices[MCubes.mc_tri_table[cubeIndex, n + 2]]
								);
						}

					}
				}
			}
		}

		private void EmitTriangle(int v1, int v2, int v3)
		{
			_indices.Add(v1);
			_indices.Add(v2);
			_indices.Add(v3);
		}

		private void VVV()
		{
			Values = new MetaSurfaceCell[Resolution + 1, Resolution + 1, Resolution + 1];
			for (int i = 0; i <= Resolution; i++)
			{
				for (int j = 0; j <= Resolution; j++)
				{
					for (int k = 0; k <= Resolution; k++)
					{
						this.Values[i, j, k] = new MetaSurfaceCell()
						{
							Position = Vector3.Zero,
							Value = 0,
							EdgeXIndex = -1,
							EdgeYIndex = -1,
							EdgeZIndex = -1
						};
					}
				}
			}
		}

		private void Reset()
		{
			//_octree = new MetaSurfaceOctTree();
			_indices.Clear();
			_vertices.Clear();
			_normals.Clear();
			_indexCounter = 0;
		}
	}
}
