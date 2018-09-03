using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	public delegate float msurf_eval_func_t(MetaSurface ms, float a, float b, float c);
	public delegate void msurf_vertex_func_t(MetaSurface ms, Vector3 vertex);
	public delegate void msurf_normal_func_t(MetaSurface ms, Vector3 normal);

	public class MetaSurface
	{
		public enum FlipType
		{
			Greater,
			Lesser
		}

		public MetaSurface(msurf_eval_func_t eval, msurf_vertex_func_t vertex, msurf_normal_func_t normal = null)
		{
			thres = 0f;
			this.eval = eval;
			this.EmitVertex = vertex;
			this.EmitNormal = normal;

			BoundingBox = new BoundingBox(new Vector3(-1), new Vector3(1));
			Resolution = new Vector3(40);

			dx = dy = dz = 0.001f;
			Flip = FlipType.Greater;
		}

		// Can be changed
		public BoundingBox BoundingBox;

		public msurf_eval_func_t eval;
		public msurf_vertex_func_t EmitVertex;
		public msurf_normal_func_t EmitNormal;

		public Vector3 Resolution;
		public float thres;


		// Internal
		public float dx, dy, dz;
		public FlipType Flip;


		public void Polygonize()
		{
			Vector3 cellPos = Vector3.Zero;
			Vector3 delta = this.BoundingBox.Size / this.Resolution;

			if (this.eval == null || this.EmitVertex == null)
			{
				throw new ArgumentNullException("you need to set eval and vertex callbacks before calling msurf_polygonize\n");
			}

			float minX = this.BoundingBox.Minimum.X;
			float minY = this.BoundingBox.Minimum.Y;
			float minZ = this.BoundingBox.Minimum.Z;

			cellPos[0] = minX;
			for (int i = 0; i < this.Resolution[0] - 1; i++)
			{
				cellPos[1] = minY;
				for (int j = 0; j < this.Resolution[1] - 1; j++)
				{
					cellPos[2] = minZ;
					for (int k = 0; k < this.Resolution[2] - 1; k++)
					{
						process_cell(cellPos, delta);
						cellPos[2] += delta[2];
					}
					cellPos[1] += delta[1];
				}
				cellPos[0] += delta[0];
			}
		}


		private void process_cell(Vector3 cellPos, Vector3 sz)
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
				p[i] = cellPos + (sz * offs[i]);
				val[i] = this.eval(this, p[i][0], p[i][1], p[i][2]);
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


		private void process_cube(Vector3[] pos, float[] val)
		{

			int[,] pidx = new int[12, 2]{
				{0, 1}, {1, 2}, {2, 3}, {3, 0}, {4, 5}, {5, 6},
				{6, 7}, {7, 4}, {0, 4}, {1, 5}, {2, 6}, {3, 7}
			};

			Vector3[] vert = new Vector3[12];
			for (int i = 0; i < vert.Length; i++)
			{
				vert[i] = Vector3.Zero;
			}

			uint code = mc_bitcode(val, this.thres);

			if (this.Flip == FlipType.Greater)
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

					float t = (this.thres - val[p0]) / (val[p1] - val[p0]);
					vert[i] = pos[p0] + ((pos[p1] - pos[p0]) * t);
				}
			}

			for (int i = 0; MCubes.mc_tri_table[code, i] != -1; i += 3)
			{

				for (int j = 0; j < 3; j++)
				{
					Vector3 vector = vert[MCubes.mc_tri_table[code, i + j]];
					Vector3 v = new Vector3(vector[0], vector[1], vector[2]);

					if (this.EmitNormal != null)
					{
						float dfdx, dfdy, dfdz;
						dfdx = this.eval(this, v[0] - this.dx, v[1], v[2]) - this.eval(this, v[0] + this.dx, v[1], v[2]);
						dfdy = this.eval(this, v[0], v[1] - this.dy, v[2]) - this.eval(this, v[0], v[1] + this.dy, v[2]);
						dfdz = this.eval(this, v[0], v[1], v[2] - this.dz) - this.eval(this, v[0], v[1], v[2] + this.dz);

						var normalVector = new Vector3(dfdx, dfdy, dfdz);
						if (this.Flip == FlipType.Greater)
						{
							normalVector = -normalVector;
						}
						this.EmitNormal(this, normalVector);
					}
					// TODO multithreadied polygon emmit 
					this.EmitVertex(this, v);
				}
			}
		}

	}
}
