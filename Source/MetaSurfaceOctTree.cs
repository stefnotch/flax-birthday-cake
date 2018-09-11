using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEngine;

namespace NinaBirthday.MetaSurf
{
	public class MetaSurfaceCell
	{
		public Vector3 Position;
		public float Value;
		public int EdgeXIndex = -1;
		public int EdgeYIndex = -1;
		public int EdgeZIndex = -1;
	}

	public class MetaSurfaceOctTree
	{
		public static int MinElementsToSubdivide = 1;

		private MetaSurfaceOctTree[] _children;
		private List<MetaSurfaceCell> _cells;
		private BoundingBox _boundingBox;

		public MetaSurfaceOctTree(BoundingBox boundingBox)
		{
			_boundingBox = boundingBox;
			_cells = new List<MetaSurfaceCell>();
		}

		public MetaSurfaceOctTree(BoundingBox boundingBox, List<MetaSurfaceCell> cells)
		{
			_boundingBox = boundingBox;
			_cells = cells;
		}

		public static MetaSurfaceOctTree From(List<ImplicitShape> shapes)
		{
			BoundingBox boundingBox = BoundingBox.Empty;
			foreach (var shape in shapes)
			{
				boundingBox = BoundingBox.Merge(boundingBox, shape.BoundingBox);
			}
			var octree = new MetaSurfaceOctTree(boundingBox);

			/*foreach (var cell in _cells)
			{
				if (octree.AddCell(cell)) break;
			}*/
			return octree;
		}

		/*public MetaSurfaceCell this[int x, int y, int z]
		{
			get
			{

			}
		}*/

		public bool IsEmpty => _cells.Count == 0;

		public void RecursiveSubdivide()
		{
			if (Subdivide())
			{
				foreach (var child in _children)
				{
					child.RecursiveSubdivide();
				}
			}
		}

		/// <summary>
		/// Subdivies a cell into 8 children
		/// </summary>
		/// <returns>If it succeeded</returns>
		public bool Subdivide()
		{
			if (_cells.Count <= 0) return false;
			if (_cells.Count <= MinElementsToSubdivide) return false;

			// Create the child bounding boxes
			Vector3 center = _boundingBox.Center;
			Vector3[] corners = _boundingBox.GetCorners();

			var children = new List<MetaSurfaceOctTree>(corners.Length);

			for (int i = 0; i < corners.Length; i++)
			{
				Vector3 min = Vector3.Min(corners[i], center);
				Vector3 max = Vector3.Max(corners[i], center);
				children[i] = new MetaSurfaceOctTree(new BoundingBox(min, max));
			}

			// And set their vertices
			foreach (var child in children)
			{
				foreach (var cell in _cells)
				{
					if (child.AddCell(cell)) break;
				}
			}

			_children = children.Where(c => !c.IsEmpty).ToArray();

			return true;
		}

		/// <summary>
		/// Attempts to add a vertex to this <see cref="OctTreeCell"/>
		/// </summary>
		/// <param name="cell">The vertex to add</param>
		/// <returns>If it succeeded</returns>
		private bool AddCell(MetaSurfaceCell cell)
		{
			if (ContainsCell(cell))
			{
				_cells.Add(cell);
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Checks if this <see cref="OctTreeCell"/> contains the given vertex
		/// </summary>
		/// <param name="cell">The vertex to check</param>
		/// <returns>If it's a part of this <see cref="OctTreeCell"/></returns>
		private bool ContainsCell(MetaSurfaceCell cell)
		{
			return
				_boundingBox.Minimum.X <= cell.Position.X && cell.Position.X < _boundingBox.Maximum.X &&
				_boundingBox.Minimum.Y <= cell.Position.Y && cell.Position.Y < _boundingBox.Maximum.Y &&
				_boundingBox.Minimum.Z <= cell.Position.Z && cell.Position.Z < _boundingBox.Maximum.Z;
		}
	}
}
