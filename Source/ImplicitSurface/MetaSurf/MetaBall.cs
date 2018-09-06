using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;

namespace NinaBirthday.MetaSurf
{
	public class MetaBall : IImplicitShape
	{
		private float _energy;
		private Vector3 _position;
		public MetaBall(Vector3 position, float energy)
		{
			_position = position;
			_energy = energy;
			ComputeBoundingBox();
		}

		public bool ShouldComputeNormal { get; set; } = true;

		public BoundingBox BoundingBox { get; set; }

		public float Energy
		{
			get => _energy;
			set { _energy = value; ComputeBoundingBox(); }
		}

		public Vector3 Position
		{
			get => _position;
			set { _position = value; ComputeBoundingBox(); }
		}

		private void ComputeBoundingBox()
		{
			// Something, something (*4 isn't guaranteed to yield perfectly correct results)
			Vector3 size = new Vector3(_energy) * 4;
			this.BoundingBox = new BoundingBox(_position - size, _position + size);
		}

		public float Evaluate(MetaSurface ms, float x, float y, float z)
		{
			float val = 0.0f;

			float dx = _position.X - x;
			float dy = _position.Y - y;
			float dz = _position.Z - z;
			float distanceSquared = dx * dx + dy * dy + dz * dz;

			if (distanceSquared < 1e-6f)
			{
				val += 100.0f;
			}
			else
			{
				val += _energy / distanceSquared;
			}
			return val;
		}
	}
}
