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
		private MetaSurfaceOctTree[] _children;
		private List<MetaSurfaceCell> _elements;

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
		}

		public static int MinElementsToSubdivide = 1;

		public BoundingBox BoundingBox { get; private set; }

		public bool IsEmpty => _elements.Count == 0;


		private int _boundingBoxCount = 0;

		/// <summary>
		/// Used for adding an <see cref="ImplicitShape"/>
		/// </summary>
		public void AddBoundingBox(ref BoundingBox boundingBox)
		{
			//Subdivide

			if (BoundingBox.Intersects(ref boundingBox))
			{
				_boundingBoxCount++;

				Subdivide(ref boundingBox);

				foreach (var child in _children)
				{
					AddBoundingBox(ref boundingBox);
				}
			}
		}

		/// <summary>
		/// Used for removing an <see cref="ImplicitShape"/>
		/// </summary>
		public void RemoveBoundingBox(ref BoundingBox boundingBox)
		{
			if (_children.Length == 0) return;

			if (BoundingBox.Intersects(ref boundingBox))
			{
				_boundingBoxCount--;


				//Un-subdivide
				for (int i = 0; i < _children.Length; i++)
				{
					_children[i].RemoveBoundingBox(ref boundingBox);
					if (_children[i]._boundingBoxCount <= 0)
					{
						// Remove child
						_children[i] = null;
					}
				}
			}
		}

		/*public MetaSurfaceCell this[int x, int y, int z]
		{
			get
			{

			}
		}*/

		/// <summary>
		/// Subdivides a gives tree as much as it can
		/// </summary>
		public void SubdivideAll()
		{
			var bb = BoundingBox;
			if (Subdivide(ref bb))
			{
				foreach (var child in _children)
				{
					child.SubdivideAll();
				}
			}
		}

		/// <summary>
		/// Subdivies a cell into 0-8 children
		/// </summary>
		/// <returns>If it succeeded</returns>
		public bool Subdivide(ref BoundingBox boundingBox)
		{
			if (_elements.Count <= 0) return false;
			if (_elements.Count <= MinElementsToSubdivide) return false;

			// Create the child bounding boxes
			Vector3 center = BoundingBox.Center;
			Vector3[] corners = BoundingBox.GetCorners();

			var children = new MetaSurfaceOctTree[corners.Length];

			for (int i = 0; i < corners.Length; i++)
			{
				Vector3 min = Vector3.Min(corners[i], center);
				Vector3 max = Vector3.Max(corners[i], center);

				BoundingBox childBB = new BoundingBox(min, max);
				if (childBB.Intersects(boundingBox))
				{
					children[i] = new MetaSurfaceOctTree(childBB);
				}
			}

			int childCount = 8;
			// And set their vertices
			for (int i = 0; i < _children.Length; i++)
			{
				var child = _children[i];
				if (child == null)
				{
					childCount--;
					continue;
				}

				foreach (var cell in _elements)
				{
					if (child.AddElement(cell)) break;
				}

				if (child.IsEmpty)
				{
					childCount--;
					_children[i] = null;
				}
			}

			_children = children;

			return childCount > 0;
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
