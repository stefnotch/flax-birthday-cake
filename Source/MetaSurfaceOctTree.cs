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

		public BoundingBox BoundingBox { get; private set; }

		private MetaSurfaceOctTree[] _children;
		private List<MetaSurfaceCell> _elements;

		public MetaSurfaceOctTree(BoundingBox boundingBox)
		{
			BoundingBox = boundingBox;
			_elements = new List<MetaSurfaceCell>();
		}

		public MetaSurfaceOctTree(BoundingBox boundingBox, List<MetaSurfaceCell> elements = null)
		{
			BoundingBox = boundingBox;
			if (elements == null)
			{
				_elements = new List<MetaSurfaceCell>();
			}
			else
			{
				_elements = elements;
			}
			RecursiveSubdivide();
		}

		public void AddBoundingBox(BoundingBox boundingBox)
		{
			//Subdivide
		}

		public void RemoveBoundingBox(BoundingBox boundingBox)
		{
			if (_children.Length == 0) return;

			//Un-subdivide
		}

		/*public MetaSurfaceCell this[int x, int y, int z]
		{
			get
			{

			}
		}*/

		public bool IsEmpty => _elements.Count == 0;

		private void RecursiveSubdivide()
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
			if (_elements.Count <= 0) return false;
			if (_elements.Count <= MinElementsToSubdivide) return false;

			// Create the child bounding boxes
			Vector3 center = BoundingBox.Center;
			Vector3[] corners = BoundingBox.GetCorners();

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
				foreach (var cell in _elements)
				{
					if (child.AddElement(cell)) break;
				}
			}

			_children = children.Where(c => !c.IsEmpty).ToArray();

			return _children.Length > 0;
		}

		/// <summary>
		/// Attempts to add a vertex to this <see cref="OctTreeCell"/>
		/// </summary>
		/// <param name="element">The vertex to add</param>
		/// <returns>If it succeeded</returns>
		private bool AddElement(MetaSurfaceCell element)
		{
			if (ContainsElement(element))
			{
				_elements.Add(element);
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
		/// <param name="element">The vertex to check</param>
		/// <returns>If it's a part of this <see cref="OctTreeCell"/></returns>
		private bool ContainsElement(MetaSurfaceCell element)
		{
			return
				BoundingBox.Minimum.X <= element.Position.X && element.Position.X < BoundingBox.Maximum.X &&
				BoundingBox.Minimum.Y <= element.Position.Y && element.Position.Y < BoundingBox.Maximum.Y &&
				BoundingBox.Minimum.Z <= element.Position.Z && element.Position.Z < BoundingBox.Maximum.Z;
		}
	}
}
