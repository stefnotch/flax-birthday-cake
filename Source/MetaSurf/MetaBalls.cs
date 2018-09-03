using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinaBirthday.MetaSurf
{
	public struct MetaBall
	{
		public float energy;
		public float x, y, z;
	}

	class MetaBalls
	{
		public Vector3[] normals => _normals.ToArray();
		public int[] indices => _indices.ToArray();
		public Vector3[] vertices => _vertices.ToArray();

		private List<Vector3> _normals;
		private List<int> _indices;
		private List<Vector3> _vertices;
		private int _indexCounter;

		MetaBall[] balls = new MetaBall[] {
			new MetaBall()
			{
				energy = 1, x=0, y=0, z=0
			},
			new MetaBall()
			{
				energy = 0.25f, x = 0.45f, y=0, z = 0.25f
			},
			new MetaBall()
			{
				energy = 0.15f, x = -0.3f, y = 0.2f, z = 0.1f
			}
		};

		internal void reset()
		{
			_indexCounter = 0;
			_vertices = new List<Vector3>();
			_normals = new List<Vector3>();
			_indices = new List<int>();
		}

		public void vertex(MetaSurface ms, Vector3 vertex)
		{
			float x = vertex.X;
			float y = vertex.Y;
			float z = vertex.Z;

			const float dt = 0.001f;
			float dfdx = eval(ms, x - dt, y, z) - eval(ms, x + dt, y, z);
			float dfdy = eval(ms, x, y - dt, z) - eval(ms, x, y + dt, z);
			float dfdz = eval(ms, x, y, z - dt) - eval(ms, x, y, z + dt);

			// TODO: 
			_indices.Add(_indexCounter);
			_normals.Add(new Vector3(dfdx, dfdy, dfdz));
			_vertices.Add(new Vector3(x, y, z));

			_indexCounter++;
		}

		public float eval(MetaSurface ms, float x, float y, float z)
		{
			float val = 0.0f;

			for (int i = 0; i < balls.Length; i++)
			{
				float dx = balls[i].x - x;
				float dy = balls[i].y - y;
				float dz = balls[i].z - z;
				float dist_sq = dx * dx + dy * dy + dz * dz;

				if (dist_sq < 1e-6)
				{
					val += 100.0f;
				}
				else
				{
					val += balls[i].energy / dist_sq;
				}
			}
			return val;
		}
	}
}
