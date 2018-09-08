using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace NinaBirthday.MetaSurf
{       /*
metasurf - a library for implicit surface polygonization
Copyright (C) 2011-2015  John Tsiombikas <nuclear@member.fsf.org>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
	public class MetaSurface
	{
		public int[] Indices => _indices.ToArray();
		public Vector3[] Vertices => _vertices.ToArray();
		public Vector3[] Normals => _normals.ToArray();

		private readonly List<int> _indices = new List<int>();
		private readonly List<Vector3> _vertices = new List<Vector3>();
		private readonly List<Vector3> _normals = new List<Vector3>();
		private int _indexCounter;

		public readonly List<ImplicitShape> ImplicitShapes = new List<ImplicitShape>();

		public enum FlipType
		{
			Greater,
			Lesser
		}

		public MetaSurface()
		{
			Threshold = 0f;
			Resolution = new Vector3(40);

			_dx = _dy = _dz = 0.001f;
			Flip = FlipType.Greater;
		}

		public Vector3 Resolution;
		public float Threshold;
		public FlipType Flip;

		private float _dx, _dy, _dz;
		private bool _computeNormals = true;

		private void Reset()
		{
			_indices.Clear();
			_vertices.Clear();
			_normals.Clear();
			_indexCounter = 0;
		}

		public float Evaluate(float x, float y, float z, bool onlyComputeNormal = false)
		{
			float value = 0;
			Vector3 position = new Vector3(x, y, z);
			if (onlyComputeNormal)
			{
				foreach (var shape in ImplicitShapes)
				{
					//if (shape.BoundingBox.Contains(ref position) != ContainmentType.Disjoint && shape.ShouldComputeNormal)
					{
						value += shape.Evaluate(x, y, z);
					}
				}
			}
			else
			{
				foreach (var shape in ImplicitShapes)
				{
					//if (shape.BoundingBox.Contains(ref position) != ContainmentType.Disjoint)
					{
						value += shape.Evaluate(x, y, z);
					}
				}
			}

			return value;
		}

		public void Polygonize()
		{
			Reset();

			foreach (var implicitShape in this.ImplicitShapes)
			{
				PolygonizeShape(implicitShape);
			}
		}

		private void PolygonizeShape(ImplicitShape shape)
		{
			Vector3 cellPos = Vector3.Zero;
			Vector3 delta = shape.BoundingBox.Size / Resolution;

			float minX = shape.BoundingBox.Minimum.X;
			float minY = shape.BoundingBox.Minimum.Y;
			float minZ = shape.BoundingBox.Minimum.Z;

			cellPos[0] = minX;
			for (int i = 0; i < Resolution[0] - 1; i++)
			{
				cellPos[1] = minY;
				for (int j = 0; j < Resolution[1] - 1; j++)
				{
					cellPos[2] = minZ;
					for (int k = 0; k < Resolution[2] - 1; k++)
					{
						process_cell(ref cellPos, ref delta);
						cellPos[2] += delta[2];
					}
					cellPos[1] += delta[1];
				}
				cellPos[0] += delta[0];
			}
		}


		private void process_cell(ref Vector3 pos, ref Vector3 sz)
		{
			Vector3[] p = new Vector3[8];
			float[] val = new float[8];

			Vector3[] offs = new Vector3[8]{
				new Vector3(0.0f, 0.0f, 0.0f),
				new Vector3(0.0f, 0.0f, 1.0f),
				new Vector3(0.0f, 1.0f, 1.0f),
				new Vector3(0.0f, 1.0f, 0.0f),
				new Vector3(1.0f, 0.0f, 0.0f),
				new Vector3(1.0f, 0.0f, 1.0f),
				new Vector3(1.0f, 1.0f, 1.0f),
				new Vector3(1.0f, 1.0f, 0.0f)
			};

			for (int i = 0; i < 8; i++)
			{
				p[i] = pos + (sz * offs[i]);
				val[i] = Evaluate(p[i][0], p[i][1], p[i][2]);
			}

			process_cube(p, val);
		}


		// ---- marching cubes implementation ---- 
		private static uint mc_bitcode(float[] val, float thres)
		{
			uint res = 0;

			for (int i = 0; i < 8; i++)
			{
				if (val[i] > thres)
				{
					res |= ((uint)1) << i;
				}
			}
			return res;
		}

		private static readonly int[,] pidx = new int[12, 2] {
				{0, 1}, {1, 2}, {2, 3}, {3, 0}, {4, 5}, {5, 6},
				{6, 7}, {7, 4}, {0, 4}, {1, 5}, {2, 6}, {3, 7}
			};

		private void process_cube(Vector3[] pos, float[] val)
		{
			Vector3[] vert = new Vector3[12];
			for (int i = 0; i < vert.Length; i++)
			{
				vert[i] = Vector3.Zero;
			}

			uint code = mc_bitcode(val, Threshold);

			if (Flip == FlipType.Lesser)
			{
				code = ~code & 0xff;
			}

			if (MCubes.mc_edge_table[code] == 0)
			{
				return;
			}

			for (int i = 0; i < 12; i++)
			{
				if ((MCubes.mc_edge_table[code] & (1 << i)) != 0)
				{
					int p0 = pidx[i, 0];
					int p1 = pidx[i, 1];

					float t = (Threshold - val[p0]) / (val[p1] - val[p0]);
					vert[i] = pos[p0] + ((pos[p1] - pos[p0]) * t);
				}
			}

			for (int i = 0; MCubes.mc_tri_table[code, i] != -1; i += 3)
			{
				for (int j = 0; j < 3; j++)
				{
					Vector3 vector = vert[MCubes.mc_tri_table[code, i + j]];
					Vector3 v = new Vector3(vector[0], vector[1], vector[2]);

					if (_computeNormals)
					{
						float dfdx, dfdy, dfdz;
						dfdx = Evaluate(v[0] - _dx, v[1], v[2], true) - Evaluate(v[0] + _dx, v[1], v[2], true);
						dfdy = Evaluate(v[0], v[1] - _dy, v[2], true) - Evaluate(v[0], v[1] + _dy, v[2], true);
						dfdz = Evaluate(v[0], v[1], v[2] - _dz, true) - Evaluate(v[0], v[1], v[2] + _dz, true);

						var normalVector = new Vector3(dfdx, dfdy, dfdz);
						if (Flip == FlipType.Lesser)
						{
							normalVector = -normalVector;
						}
						_normals.Add(normalVector);
					}
					else
					{
						_normals.Add(Vector3.Forward);
					}
					// TODO multithreadied polygon emmit 
					_vertices.Add(v);
					_indices.Add(_indexCounter);
					_indexCounter++;
				}
			}
		}

	}
}
